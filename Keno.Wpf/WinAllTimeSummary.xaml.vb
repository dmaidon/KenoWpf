' Last Edit: 2026-03-15 - WinAllTimeSummary: loads AllTimeSummaryStore and populates 11 stat rows.
Class WinAllTimeSummary

    Friend Shared Sub ShowHistory(owner As Window)
        Dim win As New WinAllTimeSummary With {
            .Owner = owner
        }
        win.ShowDialog()
    End Sub

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub WinAllTimeSummary_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim s = AllTimeSummaryStore.LoadSummary()

        Dim netPL = s.TotalPayout - s.TotalWagered
        Dim winRate = If(s.TotalGamesPlayed > 0, CDec(s.TotalWins) / s.TotalGamesPlayed * 100D, 0D)
        Dim returnPct = If(s.TotalWagered > 0D, s.TotalPayout / s.TotalWagered * 100D, 0D)
        Dim losses = s.TotalGamesPlayed - s.TotalWins

        TbkSessions.Text = s.SessionsPlayed.ToString("N0")
        TbkGames.Text = s.TotalGamesPlayed.ToString("N0")
        TbkWinLoss.Text = $"{s.TotalWins:N0} / {losses:N0}"
        TbkWinRate.Text = $"{winRate:F1}%"
        TbkWagered.Text = s.TotalWagered.ToString("C2")
        TbkPayout.Text = s.TotalPayout.ToString("C2")

        TbkNetPL.Text = netPL.ToString("C2")
        TbkNetPL.Foreground = If(netPL >= 0D, Brushes.DarkGreen, Brushes.Crimson)

        TbkReturn.Text = $"{returnPct:F1}%"
        TbkReturn.Foreground = If(returnPct >= 100D, Brushes.DarkGreen, Brushes.Crimson)

        TbkBest.Text = s.BestSinglePayout.ToString("C2")
        TbkFreeGames.Text = s.TotalFreeGamesEarned.ToString("N0")
        TbkJackpots.Text = s.JackpotsWon.ToString("N0")
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
        Close()
    End Sub

End Class