Imports System.Threading

''' <summary>
''' One TCP connection, used for both accepted (server side) and outgoing (client side) connections.
''' Incoming bytes are appended to <see cref="RxQueue"/> and announced with <see cref="DataReceived"/>;
''' outgoing bytes are queued with <see cref="SendAsync(Byte())"/> or written directly with <see cref="Send(Byte())"/>.
''' </summary>
''' <remarks>
''' <para>DataReceived is raised on a thread-pool thread, never concurrently with itself, and a burst of data
''' may be reported by a single event: drain RxQueue completely in the handler. Exceptions thrown by a handler
''' are swallowed (and logged when DebugEnable is set).</para>
''' <para>Data that was received before the connection closed stays in RxQueue after ConnectionClosed, so a
''' final message is not lost; it is cleared the next time the session is opened.</para>
''' <para>Do not mix <see cref="Send(Byte())"/> and <see cref="SendAsync(Byte())"/> on one connection at the
''' same time: the bytes can interleave.</para>
''' </remarks>
Public Class TCPSession
    Implements IDisposable

    Public Shared DebugEnable As Boolean = False
    ''' <summary>Size of each buffer allocated for a session that was not given pooled buffers.</summary>
    Public Shared DefaultBufferSize As Integer = 128 * 1024
    ''' <summary>The longest Close waits for queued SendAsync data to drain before closing anyway.</summary>
    Public Shared CloseDrainTimeoutMs As Integer = 5000

    Public Event DataReceived(ByVal sender As Object, ByVal e As EventArgs)
    Public Event ConnectionClosed(ByVal sender As Object, ByVal e As EventArgs)

    Private ReadOnly pRxQueue As New SyncdQueue(Of Byte)
    Private ReadOnly pTxQueue As New SyncdQueue(Of Byte)

    'The current connection (Nothing when not connected). Everything tied to one socket lives in a Link, so a
    'late completion from an old connection can never touch the state of a newer one.
    Private pLink As Link
    'Pooled buffers handed in by the constructor, owned here until a connection is attached
    Private pPendingRx As BufferManager.Buffer
    Private pPendingTx As BufferManager.Buffer

    Private pTotalBytesTX As Long
    Private pTotalBytesRX As Long

    Private pNotify As Integer 'DataReceived scheduling: 0 idle, 1 worker running, 2 worker running + rerun wanted
    Private ReadOnly pEventLock As New Object
    Private disposedValue As Boolean

#Region "Link (per-connection state)"
    Private NotInheritable Class Link
        Public ReadOnly Socket As Net.Sockets.Socket
        Public ReadOnly RemoteEP As Net.IPEndPoint
        Public ReadOnly RxBuffer As BufferManager.Buffer
        Public ReadOnly TxBuffer As BufferManager.Buffer
        Public ReadOnly RxArgs As New Net.Sockets.SocketAsyncEventArgs
        Public ReadOnly TxArgs As New Net.Sockets.SocketAsyncEventArgs

        Public Closed As Integer    '1 once closed
        Public RxOps As Integer     'receive operations/handlers still touching RxArgs/RxBuffer
        Public TxOps As Integer     'send operations/handlers still touching TxArgs/TxBuffer
        Public Sending As Integer   '1 while the send pump owns TxBuffer
        Public TxOffset As Integer  'unsent part of the chunk currently in TxBuffer
        Public TxRemaining As Integer
        Private Cleaned As Integer

        Public Sub New(S As Net.Sockets.Socket, Rx As BufferManager.Buffer, Tx As BufferManager.Buffer)
            Socket = S
            RxBuffer = Rx : TxBuffer = Tx
            RxArgs.UserToken = Me : TxArgs.UserToken = Me
            Try
                RemoteEP = TryCast(S.RemoteEndPoint, Net.IPEndPoint)
            Catch ex As Exception When TypeOf ex Is Net.Sockets.SocketException OrElse TypeOf ex Is ObjectDisposedException
            End Try
        End Sub

        'Frees the SocketAsyncEventArgs and gives the buffers back, but only once the link is closed and no
        'operation can still be reading or writing them (a pooled buffer must not be reused while the kernel
        'may still write into it).
        Public Sub TryCleanup()
            If Volatile.Read(Closed) = 0 Then Return
            If Volatile.Read(RxOps) <> 0 OrElse Volatile.Read(TxOps) <> 0 Then Return
            If Interlocked.Exchange(Cleaned, 1) <> 0 Then Return
            RxArgs.Dispose()
            TxArgs.Dispose()
            RxBuffer.Release()
            TxBuffer.Release()
        End Sub
    End Class
#End Region

#Region "Properties"
    ''' <summary>True while the connection is open.</summary>
    Public ReadOnly Property Connected As Boolean
        Get
            Dim L As Link = Volatile.Read(pLink)
            If L Is Nothing OrElse Volatile.Read(L.Closed) <> 0 Then Return False
            Try
                Return L.Socket.Connected
            Catch ex As ObjectDisposedException
                Return False
            End Try
        End Get
    End Property

    ''' <summary>True while queued SendAsync data is being written to the socket.</summary>
    Public ReadOnly Property Sending() As Boolean
        Get
            Dim L As Link = Volatile.Read(pLink)
            Return L IsNot Nothing AndAlso Volatile.Read(L.Sending) <> 0
        End Get
    End Property

    Public ReadOnly Property Stats As String
        Get
            Return String.Format("(Rx:{0},Tx:{1})", TotalRx, TotalTx)
        End Get
    End Property
    Public ReadOnly Property TotalRx() As Long
        Get
            Return Interlocked.Read(pTotalBytesRX)
        End Get
    End Property
    Public ReadOnly Property TotalTx() As Long
        Get
            Return Interlocked.Read(pTotalBytesTX)
        End Get
    End Property

    Public ReadOnly Property RxQueue As SyncdQueue(Of Byte)
        Get
            Return Me.pRxQueue
        End Get
    End Property
    Public ReadOnly Property TxQueue As SyncdQueue(Of Byte)
        Get
            Return Me.pTxQueue
        End Get
    End Property

    ''' <summary>The peer's address while connected, otherwise Nothing.</summary>
    Public ReadOnly Property RemoteEndPoint() As Net.IPEndPoint
        Get
            If Not Connected Then Return Nothing
            Return Volatile.Read(pLink)?.RemoteEP
        End Get
    End Property
#End Region

#Region "Receive queue helpers"
    ''' <summary>Removes and returns everything in RxQueue, or Nothing if it is empty.</summary>
    Public Function GetRxQueueAsBytes() As Byte()
        Dim Count As Integer = RxQueue.Count
        If Count <= 0 Then Return Nothing
        Dim Buffer(Count - 1) As Byte
        Dim Got As Integer = RxQueue.DequeueArray(Buffer, 0, Buffer.Length)
        If Got < Buffer.Length Then ReDim Preserve Buffer(Got - 1)
        If Got = 0 Then Return Nothing
        Return Buffer
    End Function
    Public Function GetRxQueueAsBytes(Buffer() As Byte) As Integer
        Return RxQueue.DequeueArray(Buffer, 0, Buffer.Length)
    End Function
    Public Function GetRxQueueAsString() As String
        Dim Buffer() As Byte = GetRxQueueAsBytes()
        If Buffer Is Nothing Then Return ""
        Return Text.Encoding.ASCII.GetString(Buffer)
    End Function
#End Region

#Region "Construction / Open / Connect"
    Public Sub New()
    End Sub
    ''' <summary>Creates a session that will use the given (pooled) buffers when a connection is attached.</summary>
    Public Sub New(ByVal RxBuffer As BufferManager.Buffer, ByVal TxBuffer As BufferManager.Buffer)
        Me.pPendingRx = RxBuffer : Me.pPendingTx = TxBuffer
    End Sub

    ''' <summary>Takes over a connected socket and starts receiving.</summary>
    Public Sub Open(ByVal Socket As Net.Sockets.Socket)
        Attach(Socket)
        BeginReceive()
    End Sub

    'Takes over the socket but does not start receiving yet, so the owner can finish wiring up (attach event
    'handlers, send a greeting) before any data can arrive. Pair with BeginReceive.
    Friend Sub Attach(ByVal Socket As Net.Sockets.Socket)
        If Socket Is Nothing Then Throw New ArgumentNullException(NameOf(Socket))
        If disposedValue Then Throw New ObjectDisposedException(Me.GetType().Name)
        Close() 'replace any existing connection

        Dim Rx As BufferManager.Buffer = Interlocked.Exchange(pPendingRx, Nothing)
        Dim Tx As BufferManager.Buffer = Interlocked.Exchange(pPendingTx, Nothing)
        If Rx Is Nothing Then Rx = New BufferManager.Buffer(New Byte(DefaultBufferSize - 1) {}, 0, DefaultBufferSize, Nothing)
        If Tx Is Nothing Then Tx = New BufferManager.Buffer(New Byte(DefaultBufferSize - 1) {}, 0, DefaultBufferSize, Nothing)

        pRxQueue.Clear() : pTxQueue.Clear()
        Interlocked.Exchange(pTotalBytesRX, 0L) : Interlocked.Exchange(pTotalBytesTX, 0L)

        Dim L As New Link(Socket, Rx, Tx)
        AddHandler L.RxArgs.Completed, AddressOf OnRxCompleted
        AddHandler L.TxArgs.Completed, AddressOf OnTxCompleted
        Volatile.Write(pLink, L)
        Log("Attach " & If(L.RemoteEP Is Nothing, "?", L.RemoteEP.ToString()))
    End Sub

    Friend Sub BeginReceive()
        Dim L As Link = Volatile.Read(pLink)
        If L Is Nothing OrElse Volatile.Read(L.Closed) <> 0 Then Return
        StartReceive(L)
    End Sub

    'Gives back buffers that were handed in but never attached to a connection (a connection returns its own)
    Friend Sub ReleaseBuffers()
        Dim Rx As BufferManager.Buffer = Interlocked.Exchange(pPendingRx, Nothing)
        Dim Tx As BufferManager.Buffer = Interlocked.Exchange(pPendingTx, Nothing)
        If Rx IsNot Nothing Then Rx.Release()
        If Tx IsNot Nothing Then Tx.Release()
    End Sub

    Public Sub Connect(ByVal Address As Net.IPAddress, ByVal Port As Integer)
        Connect(New Net.IPEndPoint(Address, Port))
    End Sub
    ''' <summary>Closes any current connection, connects (blocking) to EP and starts receiving.</summary>
    Public Sub Connect(ByVal EP As Net.IPEndPoint)
        If EP Is Nothing Then Throw New ArgumentNullException(NameOf(EP))
        Close()
        Dim S As New Net.Sockets.Socket(EP.AddressFamily, Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
        Try
            S.Connect(EP)
            Open(S)
        Catch
            S.Close()
            Throw
        End Try
    End Sub
#End Region

#Region "Close"
    ''' <summary>
    ''' Closes the connection. Queued SendAsync data is given up to CloseDrainTimeoutMs to drain first, then
    ''' TxWait more milliseconds are allowed before the socket is shut down. Raises ConnectionClosed once.
    ''' Safe to call repeatedly and from any thread.
    ''' </summary>
    Public Sub Close(Optional ByVal TxWait As Integer = 0)
        Dim L As Link = Interlocked.Exchange(pLink, Nothing)
        If L IsNot Nothing Then CloseLink(L, True, TxWait)
    End Sub

    'Closes immediately without waiting for queued sends (used when stopping a server)
    Friend Sub CloseNow()
        Dim L As Link = Interlocked.Exchange(pLink, Nothing)
        If L IsNot Nothing Then CloseLink(L, False, 0)
    End Sub

    'Used by the I/O completion paths after a socket error or a graceful remote close
    Private Sub Abort(ByVal L As Link)
        Interlocked.CompareExchange(pLink, Nothing, L)
        CloseLink(L, False, 0)
    End Sub

    Private Sub CloseLink(ByVal L As Link, ByVal Graceful As Boolean, ByVal TxWait As Integer)
        If Graceful Then
            Dim Watch As Diagnostics.Stopwatch = Diagnostics.Stopwatch.StartNew()
            While Volatile.Read(L.Closed) = 0 AndAlso (Volatile.Read(L.Sending) <> 0 OrElse pTxQueue.Count > 0) AndAlso Watch.ElapsedMilliseconds < CloseDrainTimeoutMs
                Thread.Sleep(1)
            End While
            If TxWait > 0 Then Thread.Sleep(TxWait)
        End If

        If Interlocked.Exchange(L.Closed, 1) <> 0 Then Return 'someone else closed it
        Log("Close")

        Try
            L.Socket.Shutdown(Net.Sockets.SocketShutdown.Both)
        Catch ex As Exception When TypeOf ex Is Net.Sockets.SocketException OrElse TypeOf ex Is ObjectDisposedException
        End Try
        Try
            L.Socket.Close()
        Catch ex As Exception When TypeOf ex Is Net.Sockets.SocketException OrElse TypeOf ex Is ObjectDisposedException
        End Try

        'Unsent data is dropped, but received data is kept for the application to read (see class remarks).
        'Do not touch the queue if a newer connection has already been attached.
        If Volatile.Read(pLink) Is Nothing Then pTxQueue.Clear()

        L.TryCleanup()
        RaiseEvent ConnectionClosed(Me, EventArgs.Empty)
    End Sub
#End Region

#Region "Receiving"
    Private Sub StartReceive(ByVal L As Link)
        Do
            Interlocked.Increment(L.RxOps)
            If Volatile.Read(L.Closed) <> 0 Then
                Interlocked.Decrement(L.RxOps)
                L.TryCleanup()
                Return
            End If

            Dim Pending As Boolean
            Try
                L.RxArgs.SetBuffer(L.RxBuffer.Buffer, L.RxBuffer.Offset, L.RxBuffer.Length)
                Pending = L.Socket.ReceiveAsync(L.RxArgs)
            Catch ex As Exception When TypeOf ex Is Net.Sockets.SocketException OrElse TypeOf ex Is ObjectDisposedException OrElse TypeOf ex Is InvalidOperationException
                Interlocked.Decrement(L.RxOps)
                Abort(L)
                L.TryCleanup()
                Return
            End Try
            If Pending Then Return 'OnRxCompleted continues

            'Completed synchronously: handle it here instead of recursing
            Dim Keep As Boolean = ProcessReceive(L)
            Interlocked.Decrement(L.RxOps)
            If Not Keep Then
                L.TryCleanup()
                Return
            End If
        Loop
    End Sub

    Private Sub OnRxCompleted(ByVal sender As Object, ByVal e As Net.Sockets.SocketAsyncEventArgs)
        Dim L As Link = DirectCast(e.UserToken, Link)
        Dim Keep As Boolean = ProcessReceive(L)
        Interlocked.Decrement(L.RxOps)
        If Keep Then StartReceive(L) Else L.TryCleanup()
    End Sub

    'Returns True to keep receiving
    Private Function ProcessReceive(ByVal L As Link) As Boolean
        Dim A As Net.Sockets.SocketAsyncEventArgs = L.RxArgs
        If A.SocketError <> Net.Sockets.SocketError.Success OrElse A.BytesTransferred <= 0 Then
            Abort(L) 'error, or the peer closed its side
            Return False
        End If

        'Ignore data from a connection that is no longer the session's current one
        If Volatile.Read(L.Closed) <> 0 OrElse Not Object.ReferenceEquals(Volatile.Read(pLink), L) Then Return False

        pRxQueue.EnqueueArray(A.Buffer, A.Offset, A.BytesTransferred)
        Interlocked.Add(pTotalBytesRX, CLng(A.BytesTransferred))
        NotifyDataReceived()
        Return True
    End Function

    'Schedules DataReceived without ever running two handlers at once and without losing a wake-up that
    'arrives while a handler is running (state 2 makes the running worker go around again).
    Private Sub NotifyDataReceived()
        Do
            Select Case Volatile.Read(pNotify)
                Case 0
                    If Interlocked.CompareExchange(pNotify, 1, 0) = 0 Then
                        ThreadPool.QueueUserWorkItem(AddressOf NotifyWorker)
                        Return
                    End If
                Case 1
                    If Interlocked.CompareExchange(pNotify, 2, 1) = 1 Then Return
                Case Else
                    Return
            End Select
        Loop
    End Sub

    Private Sub NotifyWorker(ByVal state As Object)
        Do
            If pRxQueue.Count > 0 Then RaiseDataReceived()
            If Interlocked.CompareExchange(pNotify, 0, 1) = 1 Then Exit Do 'nobody asked for more
            Volatile.Write(pNotify, 1) 'it was 2: more data arrived meanwhile, go around again
        Loop
    End Sub

    Private Sub RaiseDataReceived()
        SyncLock pEventLock
            Try
                RaiseEvent DataReceived(Me, EventArgs.Empty)
            Catch ex As Exception
                Log("DataReceived handler threw: " & ex.Message)
            End Try
        End SyncLock
    End Sub

    ''' <summary>
    ''' Raises DataReceived on the calling thread until RxQueue is empty (stops early if a handler consumes
    ''' nothing, so a handler that ignores the data cannot cause an endless loop).
    ''' </summary>
    Public Sub Flush()
        While pRxQueue.Count > 0
            Dim Before As Integer = pRxQueue.Count
            RaiseDataReceived()
            If pRxQueue.Count >= Before Then Exit While
        End While
    End Sub
#End Region

#Region "Sending"
    ''' <summary>Queues data to be sent in the background. Throws InvalidOperationException if not connected.</summary>
    Public Sub SendAsync(ByVal Data() As Byte)
        If Data Is Nothing Then Throw New ArgumentNullException(NameOf(Data))
        SendAsync(Data, Data.Length)
    End Sub
    Public Sub SendAsync(ByVal Data() As Byte, Len As Integer)
        CheckLength(Data, Len)
        Dim L As Link = Volatile.Read(pLink)
        If L Is Nothing OrElse Volatile.Read(L.Closed) <> 0 Then Throw New InvalidOperationException("Not Connected")
        pTxQueue.EnqueueArray(Data, 0, Len)
        StartSend(L)
    End Sub

    ''' <summary>Sends data immediately on the calling thread. Throws InvalidOperationException if not connected.</summary>
    Public Sub Send(ByVal Data() As Byte)
        If Data Is Nothing Then Throw New ArgumentNullException(NameOf(Data))
        Send(Data, Data.Length)
    End Sub
    Public Sub Send(ByVal Data() As Byte, Len As Integer)
        CheckLength(Data, Len)
        Dim L As Link = Volatile.Read(pLink)
        If L Is Nothing OrElse Volatile.Read(L.Closed) <> 0 Then Throw New InvalidOperationException("Not Connected")

        Try
            Dim Sent As Integer = 0
            While Sent < Len 'a single Send may accept only part of the data
                Dim N As Integer = L.Socket.Send(Data, Sent, Len - Sent, Net.Sockets.SocketFlags.None)
                If N <= 0 Then Throw New IO.IOException("Connection closed while sending")
                Sent += N
            End While
            Interlocked.Add(pTotalBytesTX, CLng(Len))
        Catch ex As Exception When TypeOf ex Is Net.Sockets.SocketException OrElse TypeOf ex Is ObjectDisposedException
            Throw New IO.IOException("Error Sending: " + ex.Message, ex)
        End Try
    End Sub

    Private Shared Sub CheckLength(ByVal Data() As Byte, ByVal Len As Integer)
        If Data Is Nothing Then Throw New ArgumentNullException(NameOf(Data))
        If Len < 0 OrElse Len > Data.Length Then Throw New ArgumentOutOfRangeException(NameOf(Len))
    End Sub

    'Exactly one thread at a time "owns" the send pump (L.Sending = 1); it keeps going until the queue is empty.
    Private Sub StartSend(ByVal L As Link)
        If Interlocked.CompareExchange(L.Sending, 1, 0) <> 0 Then Return
        PumpSend(L)
    End Sub

    'Caller owns L.Sending. Writes queued data one TxBuffer-sized chunk at a time.
    Private Sub PumpSend(ByVal L As Link)
        Do
            If Volatile.Read(L.Closed) <> 0 Then
                Volatile.Write(L.Sending, 0)
                Return
            End If

            If L.TxRemaining <= 0 Then
                Dim N As Integer = Math.Min(L.TxBuffer.Length, pTxQueue.Count)
                If N > 0 Then N = pTxQueue.DequeueArray(L.TxBuffer.Buffer, L.TxBuffer.Offset, N)
                If N <= 0 Then
                    Volatile.Write(L.Sending, 0)
                    'A producer may have queued data after we saw the queue empty but before the flag was cleared
                    If pTxQueue.Count > 0 AndAlso Interlocked.CompareExchange(L.Sending, 1, 0) = 0 Then Continue Do
                    Return
                End If
                L.TxOffset = L.TxBuffer.Offset
                L.TxRemaining = N
            End If

            Interlocked.Increment(L.TxOps)
            If Volatile.Read(L.Closed) <> 0 Then
                Interlocked.Decrement(L.TxOps)
                Volatile.Write(L.Sending, 0)
                L.TryCleanup()
                Return
            End If

            Dim Pending As Boolean
            Try
                L.TxArgs.SetBuffer(L.TxBuffer.Buffer, L.TxOffset, L.TxRemaining)
                Pending = L.Socket.SendAsync(L.TxArgs)
            Catch ex As Exception When TypeOf ex Is Net.Sockets.SocketException OrElse TypeOf ex Is ObjectDisposedException OrElse TypeOf ex Is InvalidOperationException
                Interlocked.Decrement(L.TxOps)
                Volatile.Write(L.Sending, 0)
                Abort(L)
                L.TryCleanup()
                Return
            End Try
            If Pending Then Return 'OnTxCompleted continues

            Dim Keep As Boolean = ProcessSend(L)
            Interlocked.Decrement(L.TxOps)
            If Not Keep Then
                Volatile.Write(L.Sending, 0)
                L.TryCleanup()
                Return
            End If
        Loop
    End Sub

    Private Sub OnTxCompleted(ByVal sender As Object, ByVal e As Net.Sockets.SocketAsyncEventArgs)
        Dim L As Link = DirectCast(e.UserToken, Link)
        Dim Keep As Boolean = ProcessSend(L)
        Interlocked.Decrement(L.TxOps)
        If Keep Then
            PumpSend(L)
        Else
            Volatile.Write(L.Sending, 0)
            L.TryCleanup()
        End If
    End Sub

    'Returns True to keep sending. A partial send leaves the rest in TxOffset/TxRemaining for the next pass.
    Private Function ProcessSend(ByVal L As Link) As Boolean
        Dim A As Net.Sockets.SocketAsyncEventArgs = L.TxArgs
        If A.SocketError <> Net.Sockets.SocketError.Success OrElse A.BytesTransferred <= 0 Then
            Abort(L)
            Return False
        End If
        L.TxOffset += A.BytesTransferred
        L.TxRemaining -= A.BytesTransferred
        Interlocked.Add(pTotalBytesTX, CLng(A.BytesTransferred))
        Return True
    End Function
#End Region

    Private Shared Sub Log(ByVal Message As String)
        If DebugEnable Then Console.WriteLine("TCPSession:" & Message)
    End Sub

#Region "IDisposable Support"
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            Me.disposedValue = True
            If disposing Then
                Try
                    Me.Close(100)
                Catch ex As Exception
                    Log("Error closing: " & ex.Message)
                End Try
                ReleaseBuffers()
            End If
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
