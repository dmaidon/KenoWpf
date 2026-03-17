' Last Edit: 2026-03-15 - WinPayoutSchedule: gold banner, payout list, 5-second auto-close paused on hover.
Class WinPayoutSchedule

    Private _gameType As String = "Regular"
    Private _pickedCount As Integer
    Private _matchedCount As Integer
    Private _payout As Decimal
    Private _betAmount As Decimal

    Private _secondsLeft As Integer = 5
    Private ReadOnly _timer As New System.Windows.Threading.DispatcherTimer()

    ' ── Row model ─────────────────────────────────────────────────────────────
    Private Class PayoutRow
        Public Property Matched As String
        Public Property Win As String
        Public Property IsHighlighted As Boolean
    End Class

    ' ── Factory ───────────────────────────────────────────────────────────────
    Friend Shared Sub ShowForWin(owner As Window, gameType As String,
                                 pickedCount As Integer, matchedCount As Integer,
                                 payout As Decimal, betAmount As Decimal)
        Dim win As New WinPayoutSchedule With {
            ._gameType = gameType,
            ._pickedCount = pickedCount,
            ._matchedCount = matchedCount,
            ._payout = payout,
            ._betAmount = betAmount,
            .Owner = owner
        }
        win.ShowDialog()
    End Sub

    Public Sub New()
        InitializeComponent()
        _timer.Interval = TimeSpan.FromSeconds(1)
        AddHandler _timer.Tick, AddressOf Timer_Tick
        AddHandler Me.MouseEnter, AddressOf Win_MouseEnter
        AddHandler Me.MouseLeave, AddressOf Win_MouseLeave
    End Sub

    Private Sub WinPayoutSchedule_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        TbkWin.Text = $"You Won {_payout:C2}!"

        Dim gameTitle As String
        Select Case _gameType
            Case "Regular"
                gameTitle = $"Pick {_pickedCount} Payout Schedule"
            Case "Bullseye"
                gameTitle = "Bullseye Payout Schedule"
            Case "TopBottom"
                gameTitle = "Top / Bottom Payout Schedule"
            Case "LeftRight"
                gameTitle = "Left / Right Payout Schedule"
            Case Else
                gameTitle = "Quadrant Payout Schedule"
        End Select
        TbkGameInfo.Text = $"{gameTitle}  —  Matched: {_matchedCount}"

        Dim entries = GetPayoutScheduleEntries(_pickedCount)
        Dim rows = entries.Where(Function(kvp) kvp.Value > 0D) _
                          .OrderByDescending(Function(kvp) kvp.Key) _
                          .Select(Function(kvp)
                                      Dim winAmt = (kvp.Value * _betAmount).ToString("C2")
                                      Return New PayoutRow() With {
                                          .Matched = kvp.Key.ToString(),
                                          .Win = winAmt,
                                          .IsHighlighted = (kvp.Key = _matchedCount)
                                      }
                                  End Function) _
                          .ToList()

        LvPayouts.ItemsSource = rows

        Dim highlighted = rows.FirstOrDefault(Function(r) r.IsHighlighted)
        If highlighted IsNot Nothing Then
            LvPayouts.ScrollIntoView(highlighted)
        End If

        UpdateCountdown()
        _timer.Start()
    End Sub

    Private Sub Timer_Tick(sender As Object, e As EventArgs)
        _secondsLeft -= 1

        If _secondsLeft <= 0 Then
            _timer.Stop()
            Close()
            Return
        End If

        UpdateCountdown()
    End Sub

    Private Sub UpdateCountdown()
        TbkCountdown.Text = $"Closing in {_secondsLeft}s…"
    End Sub

    Private Sub Win_MouseEnter(sender As Object, e As MouseEventArgs)
        _timer.Stop()
        TbkCountdown.Text = String.Empty
    End Sub

    Private Sub Win_MouseLeave(sender As Object, e As MouseEventArgs)
        _secondsLeft = 5
        UpdateCountdown()
        _timer.Start()
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
        _timer.Stop()
        Close()
    End Sub

End Class