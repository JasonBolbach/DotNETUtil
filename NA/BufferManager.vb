Public Class BufferManager
    Implements IDisposable
    Public Class Buffer
        Private BM As BufferManager
        Public Buffer() As Byte
        Public Offset As Integer
        Public Length As Integer
        Public Sub Release()
            If BM Is Nothing Then
                Erase Buffer
                Buffer = Nothing
            Else
                BM.ReleaseBuffer(Me)
            End If
        End Sub
        Public Sub New(ByVal Buffer() As Byte, ByVal Offset As Integer, ByVal Length As Integer, Manager As BufferManager)
            Me.Buffer = Buffer : Me.Offset = Offset : Me.Length = Length : Me.BM = Manager
        End Sub
    End Class

    Private pBuffer() As Byte
    Private pBufferPool As SyncdStack(Of Integer)
    Private pBufferList As ArrayList

    Private pBufferSize As Integer
    Private pNumBuffers As Integer

    Public ReadOnly Property TotalBuffers() As Integer
        Get
            Return pBufferList.Count
        End Get
    End Property
    Public ReadOnly Property UsedBuffers() As Integer
        Get
            Return pBufferList.Count - pBufferPool.Count
        End Get
    End Property
    Public ReadOnly Property AvailableBuffers() As Integer
        Get
            Return pBufferPool.Count
        End Get
    End Property

    Public Sub Setup(NumBuffers As Integer, BufferSize As Integer)
        pNumBuffers = NumBuffers : pBufferSize = BufferSize

        ReDim pBuffer((NumBuffers * BufferSize) - 1)
        pBufferPool = New SyncdStack(Of Integer)
        pBufferList = New ArrayList

        pBufferPool.Clear() : pBufferList.Clear()

        For I As Integer = NumBuffers - 1 To 0 Step -1
            pBufferPool.Push(I * BufferSize)
            pBufferList.Add(I * BufferSize)
        Next
    End Sub

    Public Function GetBuffer() As Buffer
        Dim Offset As Integer
        If Not pBufferPool.TryPop(Offset) Then Return Nothing
        Return New Buffer(pBuffer, Offset, pBufferSize, Me)
    End Function
    Public Sub ReleaseBuffer(B As Buffer)
        'Releasing the same buffer twice must not put it in the pool twice
        pBufferPool.PushIfAbsent(B.Offset)
    End Sub

    Public Sub New()
    End Sub
    Public Sub New(NumBuffers As Integer, BufferSize As Integer)
        Setup(NumBuffers, BufferSize)
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                Erase pBuffer
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
