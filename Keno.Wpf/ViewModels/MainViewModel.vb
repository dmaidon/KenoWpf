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

    ' ── ticket options ───────────────────────────────────────────────────────
    Private _betPreset As Integer = 1
    Private _specialWager As Double?
    Private _consecutiveGames As Integer = 1
    Private _drawDelayMs As Integer = 500
    Private _useMultiplier As Boolean
    Private _useFirstLast As Boolean
    Private _useWayTicket As Boolean
    Private _usePowerball As Boolean
    Private _flyoutsEnabled As Boolean = True

    ''' <summary>Preset wager in dollars; 0 when a special wager is in use.</summary>
    Public Property BetPreset As Integer
        Get
            Return _betPreset
        End Get
        Set(value As Integer)
            If SetField(_betPreset, value) AndAlso value > 0 Then SpecialWager = Nothing
        End Set
    End Property

    ''' <summary>Custom wager; entering one of $0.05 or more clears the preset.</summary>
    Public Property SpecialWager As Double?
        Get
            Return _specialWager
        End Get
        Set(value As Double?)
            If SetField(_specialWager, value) AndAlso value.GetValueOrDefault() >= 0.05 Then BetPreset = 0
        End Set
    End Property

    ''' <summary>Number of consecutive games (1 when none selected).</summary>
    Public Property ConsecutiveGames As Integer
        Get
            Return _consecutiveGames
        End Get
        Set(value As Integer)
            SetField(_consecutiveGames, value)
        End Set
    End Property

    ''' <summary>Pause between balls in ms: 1000 slow, 500 medium, 200 fast, 0 super sonic.</summary>
    Public Property DrawDelayMs As Integer
        Get
            Return _drawDelayMs
        End Get
        Set(value As Integer)
            SetField(_drawDelayMs, value)
        End Set
    End Property

    Public Property UseMultiplier As Boolean
        Get
            Return _useMultiplier
        End Get
        Set(value As Boolean)
            SetField(_useMultiplier, value)
        End Set
    End Property

    Public Property UseFirstLast As Boolean
        Get
            Return _useFirstLast
        End Get
        Set(value As Boolean)
            SetField(_useFirstLast, value)
        End Set
    End Property

    Public Property UseWayTicket As Boolean
        Get
            Return _useWayTicket
        End Get
        Set(value As Boolean)
            SetField(_useWayTicket, value)
        End Set
    End Property

    Public Property UsePowerball As Boolean
        Get
            Return _usePowerball
        End Get
        Set(value As Boolean)
            SetField(_usePowerball, value)
        End Set
    End Property

    Public Property FlyoutsEnabled As Boolean
        Get
            Return _flyoutsEnabled
        End Get
        Set(value As Boolean)
            SetField(_flyoutsEnabled, value)
        End Set
    End Property

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
