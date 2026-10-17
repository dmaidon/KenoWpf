''' <summary>Display state for the main window's bank panel, status bar and jackpot ticker.</summary>
Public Class MainViewModel
    Inherits ObservableObject

    Private _bankText As String = "$0"
    Private _winningsText As String = "$0"
    Private _wagerTotalText As String = "$0"
    Private _picksText As String = "Picks: 0"
    Private _matchesText As String = "Matches: 0"
    Private _payoutText As String = "Payout: $0.00"
    Private _jackpotText As String = "Progressive Jackpot: $0"
    Private _streakText As String = "Streak: —"
    Private _streakBrush As Brush = SystemColors.ControlTextBrush
    Private _bestStreakText As String = "Best: 0W"

    Public Property BankText As String
        Get
            Return _bankText
        End Get
        Set(value As String)
            SetField(_bankText, value)
        End Set
    End Property

    Public Property WinningsText As String
        Get
            Return _winningsText
        End Get
        Set(value As String)
            SetField(_winningsText, value)
        End Set
    End Property

    Public Property WagerTotalText As String
        Get
            Return _wagerTotalText
        End Get
        Set(value As String)
            SetField(_wagerTotalText, value)
        End Set
    End Property

    Public Property PicksText As String
        Get
            Return _picksText
        End Get
        Set(value As String)
            SetField(_picksText, value)
        End Set
    End Property

    Public Property MatchesText As String
        Get
            Return _matchesText
        End Get
        Set(value As String)
            SetField(_matchesText, value)
        End Set
    End Property

    Public Property PayoutText As String
        Get
            Return _payoutText
        End Get
        Set(value As String)
            SetField(_payoutText, value)
        End Set
    End Property

    Public Property JackpotText As String
        Get
            Return _jackpotText
        End Get
        Set(value As String)
            SetField(_jackpotText, value)
        End Set
    End Property

    Public Property StreakText As String
        Get
            Return _streakText
        End Get
        Set(value As String)
            SetField(_streakText, value)
        End Set
    End Property

    Public Property StreakBrush As Brush
        Get
            Return _streakBrush
        End Get
        Set(value As Brush)
            SetField(_streakBrush, value)
        End Set
    End Property

    Public Property BestStreakText As String
        Get
            Return _bestStreakText
        End Get
        Set(value As String)
            SetField(_bestStreakText, value)
        End Set
    End Property

End Class
