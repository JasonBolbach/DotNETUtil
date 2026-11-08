Public Class SyncdQueue
    '    Implements IDisposable

    '    Private Const pMinSize As Integer = 128
    '    Private pLock As New Object
    '    Private pMaxCount As Integer = 0
    '    Private pCount As Integer = 0
    '    Private pData(pMinSize - 1) As Object
    '    Private pHeadPtr As Integer = 0
    '    Private pTailPtr As Integer = 0

    '    Public ReadOnly Property MaxCount() As Integer
    '        Get
    '            Return pMaxCount
    '        End Get
    '    End Property
    '    Public ReadOnly Property Count As Integer
    '        Get
    '            SyncLock pLock
    '                Return pCount
    '            End SyncLock
    '        End Get
    '    End Property

    '    Public Function Peek() As Object
    '        Dim Obj As Object = Nothing
    '        SyncLock pLock
    '            If pHeadPtr = pTailPtr Then Return Nothing
    '            Obj = pData(pTailPtr)
    '        End SyncLock
    '        Return Obj
    '    End Function
    '    Public Function ElementAt(Index As Integer) As Object
    '        Dim Obj As Object = Nothing
    '        SyncLock pLock
    '            If Index > (pCount - 1) Then Return Nothing
    '            Obj = pData(pTailPtr + Index)
    '        End SyncLock
    '        Return Obj
    '    End Function

    '    Public Overrides Function ToString() As String
    '        Return "(" + Count.ToString + "," + MaxCount.ToString + ")"
    '    End Function

    '    Public Sub Enqueue(ByVal obj As Object)
    '        SyncLock pLock
    '            If pData.Count = pCount Then UpSize()
    '            pData(pHeadPtr) = obj
    '            pHeadPtr += 1
    '            If pHeadPtr = pData.Count Then pHeadPtr = 0
    '            pCount += 1
    '            If pCount > pMaxCount Then pMaxCount = pCount
    '        End SyncLock
    '    End Sub
    '    Public Sub Enqueue(ByVal obj() As Object)
    '        SyncLock pLock
    '            If pData.Count - pCount <= obj.Length Then UpSize(obj.Length * 2)
    '            pData(pHeadPtr) = obj
    '            pHeadPtr += 1
    '            If pHeadPtr = pData.Count Then pHeadPtr = 0
    '            pCount += 1
    '            If pCount > pMaxCount Then pMaxCount = pCount
    '        End SyncLock
    '    End Sub

    '    Public Function Dequeue() As Object
    '        Dim Obj As Object = Nothing
    '        SyncLock pLock
    '            If pHeadPtr = pTailPtr Then Return Nothing
    '            pCount -= 1
    '            Obj = pData(pTailPtr)
    '            pTailPtr += 1
    '            If pTailPtr = pData.Length Then pTailPtr = 0

    '            If pData.Count / 4 > Count Then DnSize()
    '        End SyncLock
    '        Return Obj
    '    End Function

    '    Public Sub Clear()
    '        SyncLock pLock
    '            pMaxCount = 0
    '            pCount = 0
    '            Erase pData
    '            ReDim pData(15)
    '            pHeadPtr = 0
    '            pTailPtr = 0
    '        End SyncLock
    '    End Sub

    '    Private Sub UpSize(Optional Size As Integer = 0)
    '        If pCount = pData.Count Then
    '            Dim NewData() As Object
    '            If Size <= 0 Then
    '                ReDim NewData((pData.Count * 2) - 1)
    '            Else
    '                ReDim NewData((pData.Count + Size) - 1)
    '            End If
    '            Console.WriteLine($"UpSize: {NewData.Length}")
    '            For I As Integer = 0 To pCount - 1
    '                NewData(I) = pData(pTailPtr)
    '                pTailPtr += 1
    '                If pTailPtr = pData.Count Then pTailPtr = 0
    '            Next
    '            pTailPtr = 0
    '                pHeadPtr = pData.Count
    '                Erase pData
    '                pData = NewData
    '            End If
    '    End Sub
    '    Private Sub DnSize()
    '        If (pCount * 2) >= pMinSize Then
    '            Dim NewData((pCount * 2) - 1)
    '            For I As Integer = 0 To pCount - 1
    '                NewData(I) = pData(pTailPtr)
    '                pTailPtr += 1
    '                If pTailPtr = pData.Count Then pTailPtr = 0
    '            Next
    '            pTailPtr = 0
    '            pHeadPtr = pCount
    '            Erase pData
    '            pData = NewData
    '        End If
    '    End Sub
    '    Public Sub TrimToSize()
    '    End Sub

    '#Region "IDisposable Support"
    '    Private disposedValue As Boolean ' To detect redundant calls

    '    ' IDisposable
    '    Protected Overridable Sub Dispose(disposing As Boolean)
    '        If Not Me.disposedValue Then
    '            If disposing Then
    '                Erase pData
    '                pData = Nothing
    '            End If

    '            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
    '            ' TODO: set large fields to null.
    '        End If
    '        Me.disposedValue = True
    '    End Sub

    '    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    '    'Protected Overrides Sub Finalize()
    '    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    '    Dispose(False)
    '    '    MyBase.Finalize()
    '    'End Sub

    '    ' This code added by Visual Basic to correctly implement the disposable pattern.
    '    Public Sub Dispose() Implements IDisposable.Dispose
    '        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '        Dispose(True)
    '        GC.SuppressFinalize(Me)
    '    End Sub
    '#End Region
    Inherits Concurrent.ConcurrentQueue(Of Byte)
End Class

