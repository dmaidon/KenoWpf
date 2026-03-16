' Last Edit: 2026-03-15 - WinConsecutiveSummary: per-game ListView populated from results tuple; totals panel.
Class WinConsecutiveSummary

    ' ── Row model ─────────────────────────────────────────────────────────────
    Private Class GameRow
        Public Property GameNum As String
        Public Property Bet As String
        Public Property Matched As String
        Public Property Payout As String
        Public Property Result As String
        Public Property IsWin As Boolean
    End Class

    ' ── Factory ───────────────────────────────────────────────────────────────
    Friend Shared Sub ShowSummary(owner As Window,
                                  results As List(Of (Index As Integer, Matched As Integer, Payout As Decimal, Multiplier As Integer, FirstLastBonus As Decimal)),
                                  betAmount As Decimal,
                                  bonus As Decimal,
                                  subtotal As Decimal,
                                  totalPayout As Decimal)
        Dim win As New WinConsecutiveSummary With {
            .Owner = owner
        }
        win.Populate(results, betAmount, bonus, subtotal, totalPayout)
        win.ShowDialog()
    End Sub

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub Populate(results As List(Of (Index As Integer, Matched As Integer, Payout As Decimal, Multiplier As Integer, FirstLastBonus As Decimal)),
                         betAmount As Decimal,
                         bonus As Decimal,
                         subtotal As Decimal,
                         totalPayout As Decimal)
        Dim wins = 0
        Dim rows = results.Select(Function(r)
                                      Dim won = r.Payout > 0D
                                      If won Then wins += 1
                                      Return New GameRow() With {
                                          .GameNum = r.Index.ToString(),
                                          .Bet = betAmount.ToString("C2"),
                                          .Matched = r.Matched.ToString(),
                                          .Payout = r.Payout.ToString("C2"),
                                          .Result = If(won, ChrW(&H2713) & " Win", "—"),
                                          .IsWin = won
                                      }
                                  End Function).ToList()

        LvGames.ItemsSource = rows

        TbkWins.Text = $"{wins} / {results.Count}"
        TbkSubtotal.Text = subtotal.ToString("C2")
        TbkBonus.Text = If(bonus > 1D, $"{bonus:0.#}×", "1× (no bonus)")
        TbkTotal.Text = totalPayout.ToString("C2")
        TbkTotal.Foreground = If(totalPayout > 0D, Brushes.ForestGreen, Brushes.Black)
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
        Close()
    End Sub

End Class