Public Class SyncdStack
    Inherits Stack

    Private pLock As New Object
    Private pMaxCount As Integer = 0

    Public ReadOnly Property MaxCount() As Integer
        Get
            Return pMaxCount
        End Get
    End Property
    Public Overrides Function ToString() As String
        Return "(" + Count.ToString + "," + MaxCount.ToString + ")"
    End Function
    Public Overrides Sub Push(ByVal obj As Object)
        SyncLock pLock
            MyBase.Push(obj)
            If MyBase.Count > pMaxCount Then pMaxCount = MyBase.Count
        End SyncLock
    End Sub
    Public Overrides Function Pop() As Object
        SyncLock pLock
            If MyBase.Count > 0 Then Return MyBase.Pop()
            Return Nothing
        End SyncLock
    End Function
    Public Overrides Sub Clear()
        SyncLock pLock
            MyBase.Clear()
            Me.pMaxCount = 0
        End SyncLock
    End Sub
    Public Overrides ReadOnly Property Count As Integer
        Get
            SyncLock pLock
                Return MyBase.Count
            End SyncLock
        End Get
    End Property
End Class