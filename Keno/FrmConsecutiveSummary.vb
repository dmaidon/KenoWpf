' Last Edit: 2026-03-13 - Results tuple extended with FirstLastBonus for log transparency.
Friend Class FrmConsecutiveSummary
    Inherits Form

    Friend Shared Sub ShowSummary(owner As Form,
                                  results As List(Of (Index As Integer, Matched As Integer, Payout As Decimal, Multiplier As Integer, FirstLastBonus As Decimal)),
                                  betAmount As Decimal,
                                  bonus As Decimal,
                                  subtotal As Decimal,
                                  totalPayout As Decimal)
        Using frm As New FrmConsecutiveSummary(results, betAmount, bonus, subtotal, totalPayout)
            frm.ShowDialog(owner)
        End Using
    End Sub

    Private WithEvents BtnClose As New Button()

    Private Sub New(results As List(Of (Index As Integer, Matched As Integer, Payout As Decimal, Multiplier As Integer, FirstLastBonus As Decimal)),
                    betAmount As Decimal,
                    bonus As Decimal,
                    subtotal As Decimal,
                    totalPayout As Decimal)

        Text = "Consecutive Games Summary"
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        StartPosition = FormStartPosition.CenterParent
        Font = New Font("Segoe UI", 8)
        ClientSize = New Size(390, 510)

        Dim lblTitle As New Label With {
            .Text = "Consecutive Games — Results",
            .Font = New Font("Segoe UI", 13, FontStyle.Bold),
            .Dock = DockStyle.Top,
            .Height = 42,
            .TextAlign = ContentAlignment.MiddleCenter,
            .BackColor = Color.SteelBlue,
            .ForeColor = Color.White
        }

        ' Per-game ListView
        Dim lv As New ListView With {
            .View = View.Details,
            .FullRowSelect = True,
            .GridLines = True,
            .Location = New Point(8, 52),
            .Size = New Size(370, 265),
            .HeaderStyle = ColumnHeaderStyle.Nonclickable,
            .MultiSelect = False,
            .Font = New Font("Segoe UI", 8)
        }
        lv.Columns.Add("Game", 55, HorizontalAlignment.Center)
        lv.Columns.Add("Bet", 75, HorizontalAlignment.Right)
        lv.Columns.Add("Matched", 72, HorizontalAlignment.Center)
        lv.Columns.Add("Payout", 100, HorizontalAlignment.Right)
        lv.Columns.Add("Result", -2, HorizontalAlignment.Center)

        Dim wins = 0
        For Each r In results
            Dim won = r.Payout > 0D
            If won Then wins += 1
            Dim item As New ListViewItem(r.Index.ToString())
            item.SubItems.Add(betAmount.ToString("C2"))
            item.SubItems.Add(r.Matched.ToString())
            item.SubItems.Add(r.Payout.ToString("C2"))
            item.SubItems.Add(If(won, ChrW(&H2713) & " Win", "—"))
            If won Then item.BackColor = Color.LightGoldenrodYellow
            lv.Items.Add(item)
        Next

        ' Summary panel
        Dim tlpSummary As New TableLayoutPanel With {
            .ColumnCount = 2,
            .RowCount = 4,
            .Location = New Point(12, 322),
            .Size = New Size(356, 120),
            .Font = New Font("Segoe UI", 8)
        }
        tlpSummary.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 58.0F))
        tlpSummary.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 42.0F))
        For r = 0 To 3
            tlpSummary.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Next

        AddSummaryRow(tlpSummary, "Games Won:", $"{wins} / {results.Count}", 0)
        AddSummaryRow(tlpSummary, "Subtotal (before bonus):", subtotal.ToString("C2"), 1)
        AddSummaryRow(tlpSummary, "Bonus Multiplier:", If(bonus > 1D, $"{bonus:0.#}×", "1× (no bonus)"), 2)

        tlpSummary.Controls.Add(New Label With {
            .Text = "Total Payout:",
            .Dock = DockStyle.Fill,
            .Margin = New Padding(4, 6, 8, 4),
            .TextAlign = ContentAlignment.MiddleRight,
            .Font = New Font("Segoe UI", 8, FontStyle.Bold)
        }, 0, 3)
        tlpSummary.Controls.Add(New Label With {
            .Text = totalPayout.ToString("C2"),
            .Dock = DockStyle.Fill,
            .Margin = New Padding(4, 6, 4, 4),
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New Font("Segoe UI", 8, FontStyle.Bold),
            .ForeColor = If(totalPayout > 0D, Color.ForestGreen, SystemColors.ControlText)
        }, 1, 3)

        BtnClose.Text = "Close"
        BtnClose.DialogResult = DialogResult.OK
        BtnClose.Location = New Point(270, 464)
        BtnClose.Size = New Size(98, 34)

        AcceptButton = BtnClose
        Controls.Add(lblTitle)
        Controls.Add(lv)
        Controls.Add(tlpSummary)
        Controls.Add(BtnClose)
    End Sub

    Private Shared Sub AddSummaryRow(tlp As TableLayoutPanel, caption As String, value As String, row As Integer)
        tlp.Controls.Add(New Label With {
            .Text = caption,
            .Dock = DockStyle.Fill,
            .Margin = New Padding(4, 4, 8, 4),
            .TextAlign = ContentAlignment.MiddleRight,
            .ForeColor = SystemColors.GrayText
        }, 0, row)
        tlp.Controls.Add(New Label With {
            .Text = value,
            .Dock = DockStyle.Fill,
            .Margin = New Padding(4, 4, 4, 4),
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New Font("Segoe UI", 8, FontStyle.Bold)
        }, 1, row)
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As EventArgs) Handles BtnClose.Click
        Close()
    End Sub

End Class