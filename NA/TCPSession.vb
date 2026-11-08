Public Class TCPSession
    Implements IDisposable
    Public Shared DebugEnable As Boolean = False
    Public Shared DefaultBufferSize As Integer = 128 * 1024
    Public Shared RxIndex = 0

    Private pSocket As Net.Sockets.Socket
    Private pRxQueue As New SyncdQueue(Of Byte)
    Private pTxQueue As New SyncdQueue(Of Byte)
    Private pRxBuffer, pTxBuffer As BufferManager.Buffer
    Private pConnectedTime As DateTime

    Private pConnected As Boolean
    Private pTotalBytesTX As Long
    Private pTotalBytesRX As Long
    Private pInSend As Boolean = False

    Private WithEvents pRxSAEA As Net.Sockets.SocketAsyncEventArgs
    Private WithEvents pTxSAEA As Net.Sockets.SocketAsyncEventArgs

    Public Event DataReceived(ByVal sender As Object, ByVal e As EventArgs)
    Public Event ConnectionClosed(ByVal sender As Object, ByVal e As EventArgs)

    Friend Sub ReleaseBuffers()
        If pRxBuffer IsNot Nothing Then pRxBuffer.Release()
        If pTxBuffer IsNot Nothing Then pTxBuffer.Release()
        pRxBuffer = Nothing : pTxBuffer = Nothing
    End Sub

    Public ReadOnly Property Connected As Boolean
        Get
            Dim Result As Boolean = True

            If Not pConnected Then Return False
            If pSocket Is Nothing Then Return False

            Try
                If pSocket.Connected = False Then Return False
            Catch ex As Exception
                Return False
            End Try

            Try
                If (Now - pConnectedTime).TotalMilliseconds > 5000 Then
                    If pSocket.Poll(100, Net.Sockets.SelectMode.SelectWrite) = False Then Return False
                End If
            Catch ex As Exception
                Return False
            End Try

            Return True
        End Get
    End Property
    Public ReadOnly Property Sending() As Boolean
        Get
            Return Me.pInSend
        End Get
    End Property
    Public ReadOnly Property Stats As String
        Get
            Return String.Format("(Rx:{0},Tx:{1})", pTotalBytesRX, pTotalBytesTX)
        End Get
    End Property
    Public ReadOnly Property TotalRx() As Integer
        Get
            Return pTotalBytesRX
        End Get
    End Property
    Public ReadOnly Property TotalTx() As Integer
        Get
            Return pTotalBytesTX
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

    Public Function GetRxQueueAsBytes() As Byte()
        Dim Count As Integer = RxQueue.Count
        If Count <= 0 Then Return Nothing
        Dim Buffer(Count - 1) As Byte

        RxQueue.DequeueArray(Buffer, 0, Buffer.Length)

        Return Buffer
    End Function
    Public Function GetRxQueueAsBytes(Buffer() As Byte) As Integer
        Return RxQueue.DequeueArray(Buffer, 0, Buffer.Length)
    End Function
    Public Function GetRxQueueAsString() As String
        Dim Buffer() As Byte = GetRxQueueAsBytes()
        If Buffer Is Nothing Then Return ""
        If Buffer.Length <= 0 Then Return ""
        Dim Str As String = System.Text.ASCIIEncoding.ASCII.GetString(Buffer)
        Erase Buffer
        Return Str
    End Function

    Public ReadOnly Property RemoteEndPoint() As Net.IPEndPoint
        Get
            If Not Connected Then Return Nothing
            Return pSocket.RemoteEndPoint
        End Get
    End Property

    Private Sub pRxSAEA_Completed(ByVal sender As Object, ByVal e As System.Net.Sockets.SocketAsyncEventArgs) Handles pRxSAEA.Completed
        HandleRx()
    End Sub
    Private Sub pTxSAEA_Completed(ByVal sender As Object, ByVal e As System.Net.Sockets.SocketAsyncEventArgs) Handles pTxSAEA.Completed
        pInSend = pTxQueue.Count > 0
        HandleTx()
    End Sub

    Public Sub New()
        pConnected = False
    End Sub
    Public Sub New(ByVal RxBuffer As BufferManager.Buffer, ByVal TxBuffer As BufferManager.Buffer)
        pConnected = False
        Me.pRxBuffer = RxBuffer : Me.pTxBuffer = TxBuffer
    End Sub

    Public Sub Open(ByVal Socket As Net.Sockets.Socket)
        If pRxBuffer Is Nothing Then
            Dim Buff(DefaultBufferSize - 1) As Byte
            Me.pRxBuffer = New BufferManager.Buffer(Buff, 0, Buff.Length, Nothing)
        End If
        If pTxBuffer Is Nothing Then
            Dim Buff(DefaultBufferSize - 1) As Byte
            Me.pTxBuffer = New BufferManager.Buffer(Buff, 0, Buff.Length, Nothing)
        End If

        pTotalBytesRX = 0 : pTotalBytesTX = 0
        pTxQueue.Clear() : pRxQueue.Clear()
        pSocket = Socket
        pConnected = True
        Me.pConnectedTime = Now
        StartReceive()
    End Sub
    Public Sub Close(Optional ByVal TxWait As Integer = 0)
        Console.WriteLine("TCPSession:Close")
        If pConnected = False Then Exit Sub
        Me.pConnected = False

        While Sending
            Threading.Thread.Sleep(1)
        End While
        Threading.Thread.Sleep(TxWait)

        Me.pInSend = False

        Try
            If pSocket IsNot Nothing Then
                pSocket.Shutdown(Net.Sockets.SocketShutdown.Both)
            End If
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try

        Try
            If pSocket IsNot Nothing Then
                pSocket.Close()
                Me.pSocket = Nothing
            End If
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try

        If pRxSAEA IsNot Nothing Then
            pRxSAEA.Dispose()
            pRxSAEA = Nothing
        End If

        If pTxSAEA IsNot Nothing Then
            pTxSAEA.Dispose()
            pTxSAEA = Nothing
        End If

        pRxQueue.Clear() : pTxQueue.Clear()
        ReleaseBuffers()

        RaiseEvent ConnectionClosed(Me, New EventArgs)
    End Sub

    Private Sub StartReceive()
        If DebugEnable Then Console.WriteLine("TCPClient:StartReceive")
        If pSocket Is Nothing Then Exit Sub
        Dim WillRaise As Boolean = False

        If pRxSAEA Is Nothing Then pRxSAEA = New Net.Sockets.SocketAsyncEventArgs

        pRxSAEA.AcceptSocket = pSocket
        pRxSAEA.SetBuffer(pRxBuffer.Buffer, pRxBuffer.Offset, pRxBuffer.Length)

        Try
            WillRaise = pSocket.ReceiveAsync(pRxSAEA)
        Catch ex As Exception
        End Try

        If Not WillRaise Then HandleRx()
    End Sub

    Public Sub SendAsync(ByVal Data() As Byte)
        SendAsync(Data, Data.Length)
    End Sub
    Public Sub SendAsync(ByVal Data() As Byte, Len As Integer)
        TxQueue.EnqueueArray(Data, 0, Len)
        If pTxSAEA Is Nothing Then pTxSAEA = New Net.Sockets.SocketAsyncEventArgs
        If Not Sending Then HandleTx()
    End Sub
    Public Sub Send(ByVal Data() As Byte)
        Send(Data, Data.Length)
    End Sub
    Public Sub Send(ByVal Data() As Byte, Len As Integer)
        If Not Connected Then
            Throw New Exception("Not Connected")
            Exit Sub
        End If

        Try
            pSocket.Send(Data, Len, Net.Sockets.SocketFlags.None)
            Me.pTotalBytesTX += Len
        Catch ex As Exception
            Throw New Exception("Error Sending: " + ex.Message, ex)
        End Try
    End Sub

    Public Sub Flush()
        While Me.RxQueue.Count > 0
            DataReceivedEventRaiser()
        End While
    End Sub


    Private pInEventRaiser = False
    Private Sub DataReceivedEventRaiser()
        If pInEventRaiser Then Exit Sub
        pInEventRaiser = True
        RaiseEvent DataReceived(Me, New EventArgs)
        pInEventRaiser = False
    End Sub

    Private Sub HandleRx()
        RxIndex += 1
        If pRxSAEA Is Nothing Then Exit Sub

        Try
            If Not pRxSAEA.SocketError = Net.Sockets.SocketError.Success Then
                Me.Close()
                Exit Sub
            End If
        Catch ex As Exception
            Me.Close()
            Exit Sub
        End Try

        If pRxSAEA.BytesTransferred = 0 Then
            Me.Close()
            Exit Sub
        End If

        If Not pRxSAEA.AcceptSocket.Connected Then
            Me.Close()
            Exit Sub
        End If

        While True
            pRxQueue.EnqueueArray(pRxSAEA.Buffer, pRxBuffer.Offset, pRxSAEA.BytesTransferred)
            Threading.Interlocked.Add(Me.pTotalBytesRX, pRxSAEA.BytesTransferred)

            Dim Pending As Boolean = True
            Try
                pRxSAEA.AcceptSocket = pSocket
                pRxSAEA.SetBuffer(pRxBuffer.Buffer, pRxBuffer.Offset, pRxBuffer.Length)
                Pending = pSocket.ReceiveAsync(pRxSAEA)
            Catch ex As Exception
                Exit While
            End Try

            Threading.ThreadPool.QueueUserWorkItem(AddressOf DataReceivedEventRaiser, Nothing)

            If Pending Then Exit While
        End While
    End Sub
    Private Sub HandleTx()
        If pTxSAEA Is Nothing Then Exit Sub
        pInSend = True

        If pTxQueue.Count <= 0 Then
            pInSend = False
            Exit Sub
        End If

        If Not pTxSAEA.SocketError = Net.Sockets.SocketError.Success Then
            pInSend = False
            Me.Close()
            Exit Sub
        End If

        Try
            If Not pSocket.Connected Then
                pInSend = False
                Me.Close()
                Exit Sub
            End If
        Catch ex As Exception
            pInSend = False
            Me.Close()
            Exit Sub
        End Try
        Dim QC As Integer = TxQueue.Count
        Dim Count As Integer = IIf(pTxBuffer.Length <= QC, pTxBuffer.Length, QC)
        TxQueue.DequeueArray(pTxBuffer.Buffer, pTxBuffer.Offset, Count)

        pTxSAEA = New Net.Sockets.SocketAsyncEventArgs
        pTxSAEA.AcceptSocket = pSocket
        pTxSAEA.SetBuffer(pTxBuffer.Buffer, pTxBuffer.Offset, Count)
        pSocket.SendAsync(pTxSAEA)

        Threading.Interlocked.Add(Me.pTotalBytesTX, Count)
    End Sub

    Public Sub Connect(ByVal Address As Net.IPAddress, ByVal Port As Integer)
        Try
            Connect(New Net.IPEndPoint(Address, Port))
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
    Public Sub Connect(ByVal EP As Net.IPEndPoint)
        Close()
        Try
            pSocket = New Net.Sockets.Socket(EP.AddressFamily, Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
        Catch ex As Exception
            Throw New Exception("Error Connecting: " + ex.ToString(), ex)
        End Try

        Try
            pSocket.Connect(EP)
        Catch ex As Exception
            Throw New Exception("Error Connecting: " + ex.ToString(), ex)
        End Try

        Me.Open(pSocket)
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                Try
                    Me.Close(100)
                Catch ex As Exception
                End Try
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
