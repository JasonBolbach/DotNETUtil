''' <summary>
''' A thread-safe LIFO stack. Every member takes the same internal lock, so single calls are atomic;
''' use the combined operations (<see cref="TryPop"/>, <see cref="PushIfAbsent"/>) rather than a
''' Count/Contains check followed by Pop/Push, which another thread can interleave with.
''' </summary>
Public Class SyncdStack(Of T)
    Private ReadOnly pLock As New Object
    Private ReadOnly pStack As New Stack(Of T)
    Private pMaxCount As Integer = 0

    ''' <summary>The largest number of items held at once since creation or the last Clear.</summary>
    Public ReadOnly Property MaxCount() As Integer
        Get
            SyncLock pLock
                Return pMaxCount
            End SyncLock
        End Get
    End Property

    Public ReadOnly Property Count As Integer
        Get
            SyncLock pLock
                Return pStack.Count
            End SyncLock
        End Get
    End Property

    Public Overrides Function ToString() As String
        SyncLock pLock
            Return "(" + pStack.Count.ToString + "," + pMaxCount.ToString + ")"
        End SyncLock
    End Function

    Public Sub Push(ByVal Item As T)
        SyncLock pLock
            PushCore(Item)
        End SyncLock
    End Sub

    ''' <summary>Pushes Item unless an equal item is already on the stack. Returns True if it was pushed.</summary>
    Public Function PushIfAbsent(ByVal Item As T) As Boolean
        SyncLock pLock
            If pStack.Contains(Item) Then Return False
            PushCore(Item)
            Return True
        End SyncLock
    End Function

    ''' <summary>Removes and returns the top item. Throws InvalidOperationException if the stack is empty.</summary>
    Public Function Pop() As T
        Dim Item As T
        If Not TryPop(Item) Then Throw New InvalidOperationException("Stack is empty")
        Return Item
    End Function

    ''' <summary>Removes the top item into Item. Returns False (and leaves Item at its default) if the stack is empty.</summary>
    Public Function TryPop(ByRef Item As T) As Boolean
        SyncLock pLock
            If pStack.Count = 0 Then
                Item = Nothing
                Return False
            End If
            Item = pStack.Pop()
            Return True
        End SyncLock
    End Function

    ''' <summary>Returns the top item without removing it. Throws InvalidOperationException if the stack is empty.</summary>
    Public Function Peek() As T
        Dim Item As T
        If Not TryPeek(Item) Then Throw New InvalidOperationException("Stack is empty")
        Return Item
    End Function

    Public Function TryPeek(ByRef Item As T) As Boolean
        SyncLock pLock
            If pStack.Count = 0 Then
                Item = Nothing
                Return False
            End If
            Item = pStack.Peek()
            Return True
        End SyncLock
    End Function

    Public Function Contains(ByVal Item As T) As Boolean
        SyncLock pLock
            Return pStack.Contains(Item)
        End SyncLock
    End Function

    ''' <summary>A snapshot of the items, top first.</summary>
    Public Function ToArray() As T()
        SyncLock pLock
            Return pStack.ToArray()
        End SyncLock
    End Function

    ''' <summary>Discards all items and resets MaxCount.</summary>
    Public Sub Clear()
        SyncLock pLock
            pStack.Clear()
            pMaxCount = 0
        End SyncLock
    End Sub

    Private Sub PushCore(ByVal Item As T)
        pStack.Push(Item)
        If pStack.Count > pMaxCount Then pMaxCount = pStack.Count
    End Sub
End Class
