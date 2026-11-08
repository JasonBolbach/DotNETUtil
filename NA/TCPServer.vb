Public Class TCPServer
    Public Class NewConnectionEventArgs
        Inherits EventArgs
        Public Client As TCPSession
        Public Sub New(ByVal Client As TCPSession)
            Me.Client = Client
        End Sub
    End Class
    Public Event NewConnection(ByVal sender As Object, ByVal e As NewConnectionEventArgs)

    Public DebugEnable As Boolean = True
    Private pListenSocket As Net.Sockets.Socket = Nothing
    Private pLocalEP As Net.IPEndPoint = Nothing
    Private pBackLog As Integer
    Private pListening As Boolean = False

    Private pBM As BufferManager

    Private pClients As ArrayList
    Private pClientsLock As New Object

    Private WithEvents pSAEA_Listener As Net.Sockets.SocketAsyncEventArgs

    Public ReadOnly Property BufferPoolStats() As String
        Get
            Return "Total: " + pBM.TotalBuffers.ToString + ", Used: " + pBM.UsedBuffers.ToString + ", Available: " + pBM.AvailableBuffers.ToString
        End Get
    End Property

    Public Sub Setup(ByVal MaxConnections As Integer, ByVal BufferSize As Integer, BackLog As Integer)
        StopListen()
        Me.pBackLog = BackLog
        Me.pBM = New BufferManager(MaxConnections * 2, BufferSize)
        pClients = New ArrayList
    End Sub

    Public Sub StartListen(ByVal LocalEP As Net.IPEndPoint, ByVal BackLog As Integer)
        StopListen()
        If DebugEnable Then Console.WriteLine("TCPServer:StartListen")
        Me.pLocalEP = LocalEP
        Me.pBackLog = BackLog
        Try
            pListenSocket = New Net.Sockets.Socket(pLocalEP.AddressFamily, Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
            pListenSocket.Bind(pLocalEP)
            pListenSocket.Listen(pBackLog)
            pListening = True
            pSAEA_Listener = New Net.Sockets.SocketAsyncEventArgs
            StartAccept(pSAEA_Listener)
        Catch ex As Exception
            Throw New Exception("Error Starting ClientServer: " + ex.Message, ex)
        End Try
    End Sub
    Public Sub StopListen()
        If Not pListening Then Exit Sub
        pListening = False
        If DebugEnable Then Console.Write("TCPServer:StopListen:")

        While pClients.Count > 0
            Try
                pClients(0).Close()
            Catch ex As Exception
            End Try
        End While

        Try
            pSAEA_Listener.Dispose()
        Catch ex As Exception
        Finally
            pSAEA_Listener = Nothing
        End Try

        Try
            'pListenSocket.Shutdown(Net.Sockets.SocketShutdown.Both)
        Catch ex As Exception
        End Try

        Try
            pListenSocket.Close()
        Catch ex As Exception
        End Try

        Try
            pListenSocket.Dispose()
        Catch ex As Exception
        End Try

        Try
            pListenSocket = Nothing
        Catch ex As Exception
        End Try
    End Sub

    Private Sub StartAccept(SAEA_Listener As Net.Sockets.SocketAsyncEventArgs)
        SAEA_Listener.AcceptSocket = Nothing
        If pListenSocket Is Nothing Then Exit Sub
        If pListening = False Then Exit Sub
        Try
            If Not pListenSocket.AcceptAsync(SAEA_Listener) Then Handle_Accept(SAEA_Listener)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub pSAEA_Listener_Completed(ByVal sender As Object, ByVal e As System.Net.Sockets.SocketAsyncEventArgs) Handles pSAEA_Listener.Completed
        Handle_Accept(e)
    End Sub

    Private Sub Handle_Accept(ByVal SAEA As Net.Sockets.SocketAsyncEventArgs)
        If Not SAEA.SocketError = Net.Sockets.SocketError.Success Then StartAccept(SAEA)
        If pListenSocket Is Nothing Then Exit Sub

        If DebugEnable Then Console.WriteLine("Accept: {0}", SAEA.UserToken)

        If pBM.AvailableBuffers < 2 Then
            Try
                SAEA.AcceptSocket.Shutdown(Net.Sockets.SocketShutdown.Both)
                SAEA.AcceptSocket.Close()
            Catch ex As Exception
            End Try
            Exit Sub
        End If

        Dim RxBuffer As BufferManager.Buffer = pBM.GetBuffer()
        Dim TxBuffer As BufferManager.Buffer = pBM.GetBuffer()
        Dim TS As New TCPSession(RxBuffer, TxBuffer)

        SyncLock pClientsLock
            pClients.Add(TS)
            AddHandler CType(TS, TCPSession).ConnectionClosed, AddressOf TCPClient_Close
        End SyncLock

        Try
            TS.Open(SAEA.AcceptSocket)
        Catch ex As Exception
            SAEA.AcceptSocket.Shutdown(Net.Sockets.SocketShutdown.Both)
            SAEA.AcceptSocket.Close()
            RxBuffer.Release() : TxBuffer.Release()
            Exit Sub
        End Try

        StartAccept(SAEA)

        RaiseEvent NewConnection(Me, New TCPServer.NewConnectionEventArgs(TS))
    End Sub
    Private Sub TCPClient_Close(sender As Object, e As EventArgs)
        If DebugEnable Then Console.WriteLine("TCPServer:TCPClient_Close")

        SyncLock pClientsLock
            If pClients.Contains(sender) Then
                RemoveHandler CType(sender, TCPSession).ConnectionClosed, AddressOf TCPClient_Close
                SyncLock pClientsLock
                    Dim Client As TCPSession = sender
                    Client.ReleaseBuffers()
                    pClients.Remove(sender)
                End SyncLock
            End If
        End SyncLock
    End Sub

    Public Sub New()
        Me.Setup(64, 64 * 1024, 128)
    End Sub
    Public Sub New(MaxConnections As Integer, BufferSize As Integer, BackLog As Integer)
        Me.Setup(MaxConnections, BufferSize, BackLog)
    End Sub
End Class