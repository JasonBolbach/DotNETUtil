''' <summary>
''' A pollable timer with incremental snooze features.
''' </summary>
''' <remarks></remarks>
Public Class PollTimer
    Private pSnoozeIncrement As Integer
    Private pMaxDuration As Integer
    Private pStartTime As DateTime
    Private pDefaultDuration As Integer
    Private pDuration As Integer

    Public Sub Snooze()
        If pDuration < pMaxDuration Then pDuration += pSnoozeIncrement
        If pDuration > pMaxDuration Then pDuration = pMaxDuration
        pStartTime = Now
    End Sub
    Public Sub Snooze(Duration As Integer)
        If pDuration < pMaxDuration Then pDuration += Duration
        If pDuration > pMaxDuration Then pDuration = pMaxDuration
        pStartTime = Now
    End Sub

    Public Sub Reset()
        pDuration = pDefaultDuration
        pStartTime = Now
    End Sub
    Public Sub Reset(Duration As Integer)
        pDuration = Duration
        pStartTime = Now
    End Sub

    Public ReadOnly Property Elapsed() As Boolean
        Get
            If (Now - pStartTime).TotalMilliseconds > pDuration Then Return True
            Return False
        End Get
    End Property

    Public Property SnoozeIncrement() As Integer
        Get
            Return pSnoozeIncrement
        End Get
        Set(value As Integer)
            pSnoozeIncrement = value
        End Set
    End Property
    Public Property MaxDuration() As Integer
        Get
            Return pMaxDuration
        End Get
        Set(value As Integer)
            pMaxDuration = value
        End Set
    End Property
    Public Property DefaultDuration() As Integer
        Get
            Return pDefaultDuration
        End Get
        Set(value As Integer)
            pDefaultDuration = value
        End Set
    End Property

    Public ReadOnly Property Duration() As Integer
        Get
            Return pDuration
        End Get
    End Property

    Public Sub New()
    End Sub
    Public Sub New(Duration As Integer, Snooze As Integer, Max As Integer)
        Me.pDefaultDuration = Duration
        Me.pSnoozeIncrement = Snooze
        Me.pMaxDuration = Max
        Reset()
    End Sub
End Class
