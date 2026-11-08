''' <summary>
''' Asynchronous TCP listener. Each accepted connection becomes a <see cref="TCPSession"/> that uses a pair of
''' pooled buffers; when the pool is exhausted new connections are refused (and accepting continues).
''' </summary>
''' <remarks>
''' <para>NewConnection is raised on a thread-pool thread, after the session is connected (RemoteEndPoint, Send and
''' Close all work) but <b>before it starts receiving</b>. A handler can therefore attach DataReceived and
''' ConnectionClosed, or send a greeting, without missing anything: no data is read from the socket until the
''' handler returns. If the handler closes the session, nothing is ever received.</para>
''' <para>An exception thrown by a NewConnection handler is swallowed (and logged when DebugEnable is set) so it
''' cannot stop the server from accepting; the session still starts receiving.</para>
''' </remarks>
Public Class TCPServer
    Implements IDisposable

    Public Class NewConnectionEventArgs
        Inherits EventArgs
        Public ReadOnly Property Client As TCPSession
        Public Sub New(ByVal Client As TCPSession)
            Me.Client = Client
        End Sub
    End Class
    Public Event NewConnection(ByVal sender As Object, ByVal e As NewConnectionEventArgs)

    ''' <summary>Write diagnostics to the console.</summary>
    Public DebugEnable As Boolean = False

    'Guards the listen state (socket, accept args, flag, buffer pool, limits)
    Private ReadOnly pLifecycleLock As New Object
    Private pListenSocket As Net.Sockets.Socket = Nothing
    Private pAcceptArgs As Net.Sockets.SocketAsyncEventArgs = Nothing
    Private pListening As Boolean = False
    Private pBackLog As Integer
    Private pMaxConnections As Integer

    Private pBM As BufferManager

    'Guards pClients
    Private ReadOnly pClients As New List(Of TCPSession)
    Private ReadOnly pClientsLock As New Object

    Private disposedValue As Boolean

#Region "Properties"
    Public ReadOnly Property BufferPoolStats() As String
        Get
            Dim BM As BufferManager = pBM
            Return "Total: " + BM.TotalBuffers.ToString + ", Used: " + BM.UsedBuffers.ToString + ", Available: " + BM.AvailableBuffers.ToString
        End Get
    End Property

    ''' <summary>True while the server is accepting connections.</summary>
    Public ReadOnly Property IsListening As Boolean
        Get
            SyncLock pLifecycleLock
                Return pListening
            End SyncLock
        End Get
    End Property

    ''' <summary>The address and port actually bound (useful when listening on port 0), or Nothing if not listening.</summary>
    Public ReadOnly Property LocalEndPoint As Net.IPEndPoint
        Get
            SyncLock pLifecycleLock
                If Not pListening OrElse pListenSocket Is Nothing Then Return Nothing
                Return TryCast(pListenSocket.LocalEndPoint, Net.IPEndPoint)
            End SyncLock
        End Get
    End Property

    ''' <summary>Maximum simultaneous connections (set by Setup).</summary>
    Public ReadOnly Property MaxConnections As Integer
        Get
            Return pMaxConnections
        End Get
    End Property

    ''' <summary>Number of currently connected sessions.</summary>
    Public ReadOnly Property ClientCount As Integer
        Get
            SyncLock pClientsLock
                Return pClients.Count
            End SyncLock
        End Get
    End Property

    ''' <summary>A snapshot of the currently connected sessions.</summary>
    Public Function Clients() As TCPSession()
        SyncLock pClientsLock
            Return pClients.ToArray()
        End SyncLock
    End Function
#End Region

#Region "Setup / Start / Stop"
    ''' <summary>
    ''' Sizes the buffer pool (two buffers per connection) and sets the default backlog. Stops the server and
    ''' closes all connections first.
    ''' </summary>
    Public Sub Setup(ByVal MaxConnections As Integer, ByVal BufferSize As Integer, BackLog As Integer)
        If MaxConnections < 1 Then Throw New ArgumentOutOfRangeException(NameOf(MaxConnections), "MaxConnections must be at least 1")
        If BufferSize < 1 Then Throw New ArgumentOutOfRangeException(NameOf(BufferSize), "BufferSize must be at least 1")
        If BackLog < 1 Then Throw New ArgumentOutOfRangeException(NameOf(BackLog), "BackLog must be at least 1")
        If CLng(MaxConnections) * 2L * BufferSize > Integer.MaxValue Then Throw New ArgumentOutOfRangeException(NameOf(BufferSize), "MaxConnections * BufferSize is too large for one buffer pool")

        StopListen()
        SyncLock pLifecycleLock
            pBackLog = BackLog
            pMaxConnections = MaxConnections
            If pBM IsNot Nothing Then pBM.Dispose()
            pBM = New BufferManager(MaxConnections * 2, BufferSize)
        End SyncLock
    End Sub

    Public Sub StartListen(ByVal LocalEP As Net.IPEndPoint)
        StartListen(LocalEP, pBackLog)
    End Sub

    ''' <summary>
    ''' Starts listening (stopping any previous listen first). A bind or listen failure closes the socket and is
    ''' rethrown unchanged (typically a SocketException), leaving the server stopped and restartable.
    ''' </summary>
    Public Sub StartListen(ByVal LocalEP As Net.IPEndPoint, ByVal BackLog As Integer)
        If LocalEP Is Nothing Then Throw New ArgumentNullException(NameOf(LocalEP))
        If BackLog < 1 Then Throw New ArgumentOutOfRangeException(NameOf(BackLog), "BackLog must be at least 1")
        If disposedValue Then Throw New ObjectDisposedException(NameOf(TCPServer))

        StopListen()
        Log("StartListen " & LocalEP.ToString())

        Dim Args As Net.Sockets.SocketAsyncEventArgs
        SyncLock pLifecycleLock
            Dim Socket As New Net.Sockets.Socket(LocalEP.AddressFamily, Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
            Try
                Socket.Bind(LocalEP)
                Socket.Listen(BackLog)
            Catch
                Socket.Close()
                Throw
            End Try

            pBackLog = BackLog
            Args = New Net.Sockets.SocketAsyncEventArgs
            AddHandler Args.Completed, AddressOf OnAcceptCompleted
            pListenSocket = Socket
            pAcceptArgs = Args
            pListening = True
        End SyncLock

        AcceptLoop(Args)
    End Sub

    ''' <summary>Stops accepting and closes the listener and every connected session. Safe to call repeatedly.</summary>
    Public Sub StopListen()
        Dim Socket As Net.Sockets.Socket
        SyncLock pLifecycleLock
            If Not pListening Then Return
            'Flip the state first: from here on no new connection is kept (see HandleNewSocket)
            pListening = False
            Socket = pListenSocket
            pListenSocket = Nothing
            pAcceptArgs = Nothing 'disposed by the accept loop once its pending operation completes
        End SyncLock
        Log("StopListen")

        CloseQuietly(Socket, False)

        Dim Snapshot() As TCPSession = Clients()
        For Each Client As TCPSession In Snapshot
            Try
                Client.CloseNow() 'do not wait for peers that are not reading their data
            Catch ex As Exception
                Log("Error closing client: " & ex.Message)
            End Try
        Next

        'A session that was already closed never raises ConnectionClosed again; make sure none are left behind
        For Each Client As TCPSession In Snapshot
            RemoveClient(Client)
        Next
    End Sub
#End Region

#Region "Accepting"
    'The socket to accept on, or Nothing if Args no longer belongs to the active listen session
    Private Function ActiveSocketFor(Args As Net.Sockets.SocketAsyncEventArgs) As Net.Sockets.Socket
        SyncLock pLifecycleLock
            If pListening AndAlso Object.ReferenceEquals(pAcceptArgs, Args) Then Return pListenSocket
            Return Nothing
        End SyncLock
    End Function

    'Accepts until an accept goes asynchronous (iterative, so a burst of synchronous completions cannot overflow the stack)
    Private Sub AcceptLoop(Args As Net.Sockets.SocketAsyncEventArgs)
        Do
            Dim Socket As Net.Sockets.Socket = ActiveSocketFor(Args)
            If Socket Is Nothing Then
                Args.Dispose()
                Return
            End If

            Args.AcceptSocket = Nothing
            Dim Pending As Boolean
            Try
                Pending = Socket.AcceptAsync(Args)
            Catch ex As ObjectDisposedException
                Continue Do 'stopped meanwhile; the check above ends the loop
            Catch ex As Exception
                Log("AcceptAsync failed: " & ex.Message)
                Threading.Thread.Sleep(10) 'do not spin on a persistent failure
                Continue Do
            End Try

            If Pending Then Return
            ProcessAccept(Args)
        Loop
    End Sub

    Private Sub OnAcceptCompleted(ByVal sender As Object, ByVal e As Net.Sockets.SocketAsyncEventArgs)
        ProcessAccept(e)
        AcceptLoop(e)
    End Sub

    Private Sub ProcessAccept(ByVal Args As Net.Sockets.SocketAsyncEventArgs)
        Dim Accepted As Net.Sockets.Socket = Args.AcceptSocket
        Args.AcceptSocket = Nothing

        If Args.SocketError <> Net.Sockets.SocketError.Success Then
            Log("Accept error: " & Args.SocketError.ToString())
            CloseQuietly(Accepted, True)
            'Aborted/shutdown errors mean we were stopped (AcceptLoop notices). Anything else is transient: back off briefly.
            If ActiveSocketFor(Args) IsNot Nothing Then Threading.Thread.Sleep(10)
            Return
        End If

        Try
            HandleNewSocket(Accepted)
        Catch ex As Exception
            Log("Error handling new connection: " & ex.Message)
            CloseQuietly(Accepted, True)
        End Try
    End Sub

    Private Sub HandleNewSocket(ByVal Accepted As Net.Sockets.Socket)
        Dim BM As BufferManager = pBM
        Dim RxBuffer As BufferManager.Buffer = BM.GetBuffer()
        Dim TxBuffer As BufferManager.Buffer = If(RxBuffer Is Nothing, Nothing, BM.GetBuffer())
        If RxBuffer Is Nothing OrElse TxBuffer Is Nothing Then
            Log("Connection refused: no free buffers")
            If RxBuffer IsNot Nothing Then RxBuffer.Release()
            If TxBuffer IsNot Nothing Then TxBuffer.Release()
            CloseQuietly(Accepted, True)
            Return
        End If

        Log("Accept: " & Accepted.RemoteEndPoint?.ToString())

        Dim Session As New TCPSession(RxBuffer, TxBuffer)
        AddHandler Session.ConnectionClosed, AddressOf TCPClient_Close
        SyncLock pClientsLock
            pClients.Add(Session)
        End SyncLock

        'If the server was stopped after this socket was accepted, StopListen may already have taken its snapshot.
        'The flag is cleared before that snapshot, so seeing it still set here means the snapshot will include us.
        If Not IsListening Then
            RemoveClient(Session)
            CloseQuietly(Accepted, True)
            Return
        End If

        Try
            Session.Attach(Accepted) 'connected, but not receiving yet
        Catch ex As Exception
            Log("Session attach failed: " & ex.Message)
            RemoveClient(Session)
            CloseQuietly(Accepted, True)
            Return
        End Try

        Try
            RaiseEvent NewConnection(Me, New NewConnectionEventArgs(Session))
        Catch ex As Exception
            Log("NewConnection handler threw: " & ex.Message)
        End Try

        'Only now does data start flowing, so the handler could not have missed any (a no-op if it closed the session)
        Session.BeginReceive()
    End Sub

    Private Sub TCPClient_Close(sender As Object, e As EventArgs)
        Log("TCPClient_Close")
        RemoveClient(DirectCast(sender, TCPSession))
    End Sub

    'Detaches, removes and frees a session's buffers. Safe to call more than once.
    Private Sub RemoveClient(Client As TCPSession)
        RemoveHandler Client.ConnectionClosed, AddressOf TCPClient_Close
        SyncLock pClientsLock
            pClients.Remove(Client)
        End SyncLock
        Client.ReleaseBuffers()
    End Sub
#End Region

    Private Shared Sub CloseQuietly(ByVal S As Net.Sockets.Socket, ByVal Shutdown As Boolean)
        If S Is Nothing Then Return
        If Shutdown Then
            Try
                S.Shutdown(Net.Sockets.SocketShutdown.Both)
            Catch ex As Exception When TypeOf ex Is Net.Sockets.SocketException OrElse TypeOf ex Is ObjectDisposedException
            End Try
        End If
        Try
            S.Close()
        Catch ex As Exception When TypeOf ex Is Net.Sockets.SocketException OrElse TypeOf ex Is ObjectDisposedException
        End Try
    End Sub

    Private Sub Log(ByVal Message As String)
        If DebugEnable Then Console.WriteLine("TCPServer:" & Message)
    End Sub

    Public Sub New()
        Me.Setup(64, 64 * 1024, 128)
    End Sub
    Public Sub New(MaxConnections As Integer, BufferSize As Integer, BackLog As Integer)
        Me.Setup(MaxConnections, BufferSize, BackLog)
    End Sub

#Region "IDisposable Support"
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            Me.disposedValue = True
            If disposing Then
                StopListen()
                If pBM IsNot Nothing Then pBM.Dispose()
            End If
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
