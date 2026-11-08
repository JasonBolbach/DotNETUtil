Public Class SyncdQueue(Of T)
    Implements IDisposable

    Private Const pMinSize As Integer = 16
    Private pLock As New Object
    Private pMaxCount As Integer = 0
    Private pCount As Integer = 0
    Private pData(pMinSize - 1) As T
    Private pHeadPtr As Integer = 0
    Private pTailPtr As Integer = pData.Length
    Private ReadOnly pGrowthScale As Double = 0.5

    Public ReadOnly Property MaxCount() As Integer
        Get
            Return pMaxCount
        End Get
    End Property
    Public ReadOnly Property Count As Integer
        Get
            SyncLock pLock
                Return pCount
            End SyncLock
        End Get
    End Property
    Public Overrides Function ToString() As String
        Return "(" + Count.ToString + "," + MaxCount.ToString + ")"
    End Function

    Public Sub Enqueue(ByVal Obj As T)
        SyncLock pLock
            pData(pHeadPtr) = Obj
            pCount += 1
            pHeadPtr += 1
            If pHeadPtr = pData.Length Then ReSize(pData.Length * pGrowthScale)
            If pCount > pMaxCount Then pMaxCount = pCount
        End SyncLock
    End Sub
    Public Sub EnqueueArray(Source() As T, Offset As Integer, Len As Integer)
        'Console.WriteLine($"EnqueueArray:{Len}")
        SyncLock pLock
            If (pData.Length - pHeadPtr) > Len Then
                Buffer.BlockCopy(Source, Offset, pData, pHeadPtr, Len)
                pCount += Len
                pHeadPtr += Len
            ElseIf (pData.Length - pHeadPtr) + (pTailPtr) > Len Then
                ReSize(pData.Length)
                EnqueueArray(Source, Offset, Len)
            Else
                ReSize((pData.Length + Len) * (1 + pGrowthScale))
                EnqueueArray(Source, Offset, Len)
            End If
            If pCount > pMaxCount Then pMaxCount = pCount
        End SyncLock
    End Sub

    Public Function Dequeue() As T
        Dim Obj As T = Nothing
        If pCount <= 0 Then
            Throw New Exception("Queue is empty!")
        End If
        SyncLock pLock
            If pHeadPtr = pTailPtr Then
                Return Nothing
            End If
            pCount -= 1
            Obj = pData(pTailPtr)
            pTailPtr += 1
            If pCount = 0 Then
                ReSize(pMinSize)
            End If
        End SyncLock
        Return Obj
    End Function
    Public Function DequeueArray(Destination() As T, Offset As Integer, Len As Integer) As Integer
        SyncLock pLock
            If Len > pCount Then
                Len = pCount
            End If
            Buffer.BlockCopy(pData, pTailPtr, Destination, Offset, Len)
            pCount -= Len
            pTailPtr += Len
            If pCount < (pData.Length / 8) And pCount > pMinSize Then
                ReSize(pData.Length / 4)
            End If
            Return Len
        End SyncLock
    End Function

    Public Sub Clear()
        SyncLock pLock
            pMaxCount = 0
            pCount = 0
            Erase pData
            ReDim pData(pMinSize - 1)
            pHeadPtr = 0
            pTailPtr = 0
        End SyncLock
    End Sub
    Private Sub ReSize(NewSize As Integer)

        If NewSize = pData.Length Then
            Buffer.BlockCopy(pData, pTailPtr, pData, 0, pCount)
            pTailPtr = 0
            pHeadPtr = pCount
            Exit Sub
        Else
            Console.WriteLine($"{pData.Length},{NewSize}")
            Dim NewpData(NewSize - 1) As T
            Buffer.BlockCopy(pData, pTailPtr, NewpData, 0, pCount)
            pTailPtr = 0
            pHeadPtr = pCount
            Erase pData
            pData = Nothing
            pData = NewpData
        End If
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls
    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                Erase pData
                pData = Nothing
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
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class

