' Last Edit: 2026-03-15 - WinSessionSummary: populates 8-row stats; net P/L and return % colour-coded.
Class WinSessionSummary

    Friend Shared Sub ShowSummary(owner As Window,
                                  netPL As Decimal,
                                  gamesPlayed As Integer,
                                  wins As Integer,
                                  bestPayout As Decimal,
                                  freeGamesEarned As Integer,
                                  totalWagered As Decimal)
        Dim win As New WinSessionSummary With {
            .Owner = owner
        }
        win.Populate(netPL, gamesPlayed, wins, bestPayout, freeGamesEarned, totalWagered)
        win.ShowDialog()
    End Sub

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub Populate(netPL As Decimal, gamesPlayed As Integer, wins As Integer,
                         bestPayout As Decimal, freeGamesEarned As Integer, totalWagered As Decimal)
        Dim winRate = If(gamesPlayed > 0, CDec(wins) / gamesPlayed, 0D)
        Dim totalPaidOut = totalWagered + netPL
        Dim returnPct = If(totalWagered > 0D, totalPaidOut / totalWagered, 0D)

        TbkNetPL.Text = netPL.ToString("C2")
        TbkNetPL.Foreground = If(netPL >= 0D, Brushes.ForestGreen, Brushes.Firebrick)

        TbkGames.Text = gamesPlayed.ToString()
        TbkWinRate.Text = winRate.ToString("P1")
        TbkBest.Text = bestPayout.ToString("C2")
        TbkFreeGames.Text = freeGamesEarned.ToString()
        TbkWagered.Text = totalWagered.ToString("C2")
        TbkPayout.Text = totalPaidOut.ToString("C2")

        TbkReturn.Text = returnPct.ToString("P1")
        TbkReturn.Foreground = If(returnPct >= 1D, Brushes.ForestGreen, Brushes.Firebrick)
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
        Close()
    End Sub

End Class