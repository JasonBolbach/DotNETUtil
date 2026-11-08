''' <summary>
''' A thread-safe FIFO queue backed by a growable ring buffer, with bulk enqueue/dequeue for arrays.
''' Capacity doubles as needed and halves again once the queue is mostly empty (never below 16).
''' </summary>
''' <remarks>
''' Every member takes the same internal lock, so single calls are atomic. A check-then-act sequence
''' (for example Count followed by Dequeue) is not; use <see cref="TryDequeue"/> or the array methods,
''' which return how many items they actually moved.
''' </remarks>
Public Class SyncdQueue(Of T)
    Implements IDisposable

    Private Const MinCapacity As Integer = 16
    'Largest array length the runtime will allocate for a non-byte element type
    Private Const MaxCapacity As Integer = &H7FFFFFC7

    Private ReadOnly pLock As New Object
    Private pData(MinCapacity - 1) As T
    Private pHead As Integer = 0   'index of the oldest item
    Private pCount As Integer = 0
    Private pMaxCount As Integer = 0
    Private disposedValue As Boolean

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
                Return pCount
            End SyncLock
        End Get
    End Property

    Public Overrides Function ToString() As String
        SyncLock pLock
            Return "(" + pCount.ToString + "," + pMaxCount.ToString + ")"
        End SyncLock
    End Function

    Public Sub Enqueue(ByVal Obj As T)
        SyncLock pLock
            CheckNotDisposed()
            EnsureCapacity(1L)
            pData((pHead + pCount) Mod pData.Length) = Obj
            pCount += 1
            If pCount > pMaxCount Then pMaxCount = pCount
        End SyncLock
    End Sub

    Public Sub EnqueueArray(Source() As T, Offset As Integer, Len As Integer)
        CheckRange(Source, Offset, Len, NameOf(Source))
        If Len = 0 Then Return
        SyncLock pLock
            CheckNotDisposed()
            EnsureCapacity(Len)
            Dim Tail As Integer = (pHead + pCount) Mod pData.Length
            Dim First As Integer = Math.Min(Len, pData.Length - Tail)
            Array.Copy(Source, Offset, pData, Tail, First)
            If First < Len Then Array.Copy(Source, Offset + First, pData, 0, Len - First)
            pCount += Len
            If pCount > pMaxCount Then pMaxCount = pCount
        End SyncLock
    End Sub

    ''' <summary>Removes and returns the oldest item. Throws InvalidOperationException if the queue is empty.</summary>
    Public Function Dequeue() As T
        Dim Obj As T
        If Not TryDequeue(Obj) Then Throw New InvalidOperationException("Queue is empty")
        Return Obj
    End Function

    ''' <summary>Removes the oldest item into Obj. Returns False (and leaves Obj at its default) if the queue is empty.</summary>
    Public Function TryDequeue(ByRef Obj As T) As Boolean
        SyncLock pLock
            CheckNotDisposed()
            If pCount = 0 Then
                Obj = Nothing
                Return False
            End If
            Obj = pData(pHead)
            pData(pHead) = Nothing
            pHead = (pHead + 1) Mod pData.Length
            pCount -= 1
            ShrinkIfSparse()
            Return True
        End SyncLock
    End Function

    ''' <summary>Moves up to Len of the oldest items into Destination starting at Offset. Returns the number moved.</summary>
    Public Function DequeueArray(Destination() As T, Offset As Integer, Len As Integer) As Integer
        CheckRange(Destination, Offset, Len, NameOf(Destination))
        SyncLock pLock
            CheckNotDisposed()
            Len = Math.Min(Len, pCount)
            If Len = 0 Then Return 0
            Dim First As Integer = Math.Min(Len, pData.Length - pHead)
            Array.Copy(pData, pHead, Destination, Offset, First)
            Array.Clear(pData, pHead, First)
            If First < Len Then
                Array.Copy(pData, 0, Destination, Offset + First, Len - First)
                Array.Clear(pData, 0, Len - First)
            End If
            pHead = (pHead + Len) Mod pData.Length
            pCount -= Len
            ShrinkIfSparse()
            Return Len
        End SyncLock
    End Function

    ''' <summary>Discards all items, returns to the minimum capacity and resets MaxCount.</summary>
    Public Sub Clear()
        SyncLock pLock
            CheckNotDisposed()
            ReDim pData(MinCapacity - 1)
            pHead = 0
            pCount = 0
            pMaxCount = 0
        End SyncLock
    End Sub

#Region "Internals (caller holds pLock)"
    Private Sub CheckNotDisposed()
        If disposedValue Then Throw New ObjectDisposedException(Me.GetType().Name)
    End Sub

    Private Shared Sub CheckRange(Arr() As T, Offset As Integer, Len As Integer, Name As String)
        If Arr Is Nothing Then Throw New ArgumentNullException(Name)
        If Offset < 0 Then Throw New ArgumentOutOfRangeException(NameOf(Offset))
        If Len < 0 Then Throw New ArgumentOutOfRangeException(NameOf(Len))
        If Arr.Length - Offset < Len Then Throw New ArgumentException("Offset and Len exceed the array bounds")
    End Sub

    Private Sub EnsureCapacity(ByVal Extra As Long)
        Dim Needed As Long = pCount + Extra
        If Needed <= pData.Length Then Return
        If Needed > MaxCapacity Then Throw New OutOfMemoryException("Queue would exceed the maximum array size")
        Resize(CInt(Math.Min(Math.Max(CLng(pData.Length) * 2L, Needed), CLng(MaxCapacity))))
    End Sub

    Private Sub ShrinkIfSparse()
        If pData.Length > MinCapacity AndAlso pCount <= pData.Length \ 8 Then
            Resize(Math.Max(MinCapacity, pData.Length \ 2))
        End If
    End Sub

    'Copies the items, oldest first, into a new array of NewCapacity (which must hold pCount items)
    Private Sub Resize(ByVal NewCapacity As Integer)
        Dim NewData(NewCapacity - 1) As T
        Dim First As Integer = Math.Min(pCount, pData.Length - pHead)
        Array.Copy(pData, pHead, NewData, 0, First)
        If First < pCount Then Array.Copy(pData, 0, NewData, First, pCount - First)
        pData = NewData
        pHead = 0
    End Sub
#End Region

#Region "IDisposable Support"
    ''' <summary>Releases the storage. Further use of the queue throws ObjectDisposedException.</summary>
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                SyncLock pLock
                    disposedValue = True
                    pData = New T() {}
                    pHead = 0
                    pCount = 0
                End SyncLock
            End If
            disposedValue = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
