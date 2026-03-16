' Last Edit: 2026-03-10 - Form font reduced from 10pt to 9pt for row caption sizing.
Friend Class FrmSessionSummary
    Inherits Form

    Private WithEvents BtnClose As New Button()
    Private ReadOnly _tlpMain As New TableLayoutPanel()
    Private ReadOnly _lblNetPLValue As New Label()
    Private ReadOnly _lblGamesValue As New Label()
    Private ReadOnly _lblWinRateValue As New Label()
    Private ReadOnly _lblBestPayoutValue As New Label()
    Private ReadOnly _lblFreeGamesValue As New Label()
    Private ReadOnly _lblTotalWageredValue As New Label()
    Private ReadOnly _lblTotalPayoutValue As New Label()
    Private ReadOnly _lblReturnPctValue As New Label()

    Friend Shared Sub ShowSummary(owner As Form, netPL As Decimal, gamesPlayed As Integer, wins As Integer, bestPayout As Decimal, freeGamesEarned As Integer, totalWagered As Decimal)
        Using frm As New FrmSessionSummary()
            frm.Populate(netPL, gamesPlayed, wins, bestPayout, freeGamesEarned, totalWagered)
            frm.ShowDialog(owner)
        End Using
    End Sub

    Public Sub New()
        Text = "Session Summary"
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(350, 460)
        Font = New Font("Segoe UI", 9)

        Dim lblTitle As New Label With {
            .Text = "Session Summary",
            .Font = New Font("Segoe UI", 13, FontStyle.Bold),
            .Dock = DockStyle.Top,
            .Height = 42,
            .TextAlign = ContentAlignment.MiddleCenter,
            .BackColor = Color.SteelBlue,
            .ForeColor = Color.White
        }

        _tlpMain.Dock = DockStyle.Fill
        _tlpMain.ColumnCount = 2
        _tlpMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 60.0F))
        _tlpMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 40.0F))
        _tlpMain.RowCount = 9
        For r = 0 To 7
            _tlpMain.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Next
        _tlpMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        _tlpMain.Padding = New Padding(8, 10, 8, 6)

        AddRow("Net Profit / Loss:", _lblNetPLValue, 0)
        AddRow("Games Played:", _lblGamesValue, 1)
        AddRow("Win Rate:", _lblWinRateValue, 2)
        AddRow("Best Single Payout:", _lblBestPayoutValue, 3)
        AddRow("Free Games Earned:", _lblFreeGamesValue, 4)
        AddRow("Total Wagered:", _lblTotalWageredValue, 5)
        AddRow("Total Payout:", _lblTotalPayoutValue, 6)
        AddRow("Return %:", _lblReturnPctValue, 7)

        BtnClose.Text = "Close"
        BtnClose.DialogResult = DialogResult.OK
        BtnClose.Anchor = AnchorStyles.Right Or AnchorStyles.Bottom
        BtnClose.Size = New Size(90, 34)
        BtnClose.Margin = New Padding(4, 4, 4, 4)
        _tlpMain.Controls.Add(BtnClose, 1, 8)

        AcceptButton = BtnClose
        Controls.Add(_tlpMain)
        Controls.Add(lblTitle)
    End Sub

    Private Sub AddRow(captionText As String, valueLabel As Label, row As Integer)
        Dim caption As New Label With {
            .Text = captionText,
            .AutoSize = True,
            .Dock = DockStyle.Fill,
            .Margin = New Padding(4, 8, 8, 8),
            .TextAlign = ContentAlignment.MiddleRight,
            .ForeColor = SystemColors.GrayText
        }

        valueLabel.AutoSize = True
        valueLabel.Dock = DockStyle.Fill
        valueLabel.Margin = New Padding(4, 8, 4, 8)
        valueLabel.TextAlign = ContentAlignment.MiddleLeft
        valueLabel.Font = New Font("Segoe UI", 9, FontStyle.Bold)

        _tlpMain.Controls.Add(caption, 0, row)
        _tlpMain.Controls.Add(valueLabel, 1, row)
    End Sub

    Private Sub Populate(netPL As Decimal, gamesPlayed As Integer, wins As Integer, bestPayout As Decimal, freeGamesEarned As Integer, totalWagered As Decimal)
        Dim winRate = If(gamesPlayed > 0, CDec(wins) / gamesPlayed, 0D)
        Dim totalPaidOut = totalWagered + netPL
        Dim returnPct = If(totalWagered > 0D, totalPaidOut / totalWagered, 0D)

        _lblNetPLValue.Text = netPL.ToString("C2")
        _lblNetPLValue.ForeColor = If(netPL >= 0D, Color.ForestGreen, Color.Firebrick)
        _lblGamesValue.Text = gamesPlayed.ToString()
        _lblWinRateValue.Text = winRate.ToString("P1")
        _lblBestPayoutValue.Text = bestPayout.ToString("C2")
        _lblFreeGamesValue.Text = freeGamesEarned.ToString()
        _lblTotalWageredValue.Text = totalWagered.ToString("C2")
        _lblTotalPayoutValue.Text = totalPaidOut.ToString("C2")
        _lblReturnPctValue.Text = returnPct.ToString("P1")
        _lblReturnPctValue.ForeColor = If(returnPct >= 1D, Color.ForestGreen, Color.Firebrick)
    End Sub

End Class