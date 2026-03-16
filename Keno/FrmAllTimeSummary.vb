' Last Edit: 2026-03-13 - Form font reduced from 10pt to 9pt.
Friend Class FrmAllTimeSummary
    Inherits Form

    Friend Shared Sub ShowHistory(owner As Form)
        Using frm As New FrmAllTimeSummary()
            frm.ShowDialog(owner)
        End Using
    End Sub

    Public Sub New()
        Dim s = AllTimeSummaryStore.LoadSummary()

        Text = "All-Time Play History"
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(380, 510)
        Font = New Font("Segoe UI", 9)

        Dim lblTitle As New Label With {
            .Text = "All-Time Play History",
            .Font = New Font("Segoe UI", 13, FontStyle.Bold),
            .Dock = DockStyle.Top,
            .Height = 42,
            .TextAlign = ContentAlignment.MiddleCenter,
            .BackColor = Color.SteelBlue,
            .ForeColor = Color.White
        }

        Dim tlp As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 12,
            .Padding = New Padding(8, 10, 8, 6)
        }
        tlp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 60.0F))
        tlp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 40.0F))
        For r = 0 To 10
            tlp.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Next
        tlp.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))

        Dim netPL = s.TotalPayout - s.TotalWagered
        Dim winRate = If(s.TotalGamesPlayed > 0, CDec(s.TotalWins) / s.TotalGamesPlayed * 100D, 0D)
        Dim returnPct = If(s.TotalWagered > 0D, s.TotalPayout / s.TotalWagered * 100D, 0D)
        Dim losses = s.TotalGamesPlayed - s.TotalWins

        AddRow(tlp, "Sessions Played:", s.SessionsPlayed.ToString("N0"), 0)
        AddRow(tlp, "Total Games Played:", s.TotalGamesPlayed.ToString("N0"), 1)
        AddRow(tlp, "Wins / Losses:", $"{s.TotalWins:N0} / {losses:N0}", 2)
        AddRow(tlp, "Win Rate:", $"{winRate:F1}%", 3)
        AddRow(tlp, "Total Wagered:", s.TotalWagered.ToString("C2"), 4)
        AddRow(tlp, "Total Payout:", s.TotalPayout.ToString("C2"), 5)
        AddRow(tlp, "Net P/L:", netPL.ToString("C2"), 6, If(netPL >= 0D, Color.DarkGreen, Color.Crimson))
        AddRow(tlp, "Return %:", $"{returnPct:F1}%", 7, If(returnPct >= 100D, Color.DarkGreen, Color.Crimson))
        AddRow(tlp, "Best Single Payout:", s.BestSinglePayout.ToString("C2"), 8)
        AddRow(tlp, "Free Games Earned:", s.TotalFreeGamesEarned.ToString("N0"), 9)
        AddRow(tlp, "Jackpots Won:", s.JackpotsWon.ToString("N0"), 10)

        Dim btnClose As New Button With {
            .Text = "Close",
            .DialogResult = DialogResult.OK,
            .Anchor = AnchorStyles.Right Or AnchorStyles.Bottom,
            .Size = New Size(90, 34),
            .Margin = New Padding(4)
        }
        tlp.Controls.Add(btnClose, 1, 11)
        AcceptButton = btnClose

        Controls.Add(tlp)
        Controls.Add(lblTitle)
    End Sub

    Private Shared Sub AddRow(tlp As TableLayoutPanel, caption As String, value As String, row As Integer, Optional valueColor As Color = Nothing)
        Dim lbl As New Label With {
            .Text = caption,
            .AutoSize = True,
            .Dock = DockStyle.Fill,
            .Margin = New Padding(4, 8, 8, 4),
            .TextAlign = ContentAlignment.MiddleRight,
            .ForeColor = SystemColors.GrayText
        }

        Dim val As New Label With {
            .Text = value,
            .AutoSize = True,
            .Dock = DockStyle.Fill,
            .Margin = New Padding(4, 8, 4, 4),
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .ForeColor = If(valueColor = Nothing, SystemColors.ControlText, valueColor)
        }

        tlp.Controls.Add(lbl, 0, row)
        tlp.Controls.Add(val, 1, row)
    End Sub

End Class