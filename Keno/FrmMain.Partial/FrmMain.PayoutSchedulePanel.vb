' Last Edit: 2026-03-13 - 1ST/LST row added to RtbPayoutSchedule when ChkFirstLastPlay is checked.
Partial Class FrmMain

    Private Const HitsColWidth As Integer = 8
    Private Const WinColWidth As Integer = 19

    Private Sub UpdatePayoutScheduleDisplay()
        Dim pickCount = _selectedNumbers.Count
        Dim betAmount = GetSelectedBetAmount()
        If betAmount <= 0D Then betAmount = 1D

        Dim entries As Dictionary(Of Integer, Decimal)
        Dim modeTitle As String
        Dim markedCount As Integer

        If _isBullseyeActive Then
            entries = GetBullseyePayoutScheduleEntries()
            modeTitle = "Bullseye"
            markedCount = 8
        ElseIf _selectedQuadrants.Count > 0 Then
            Dim qType = GetQuadrantSelectionType()
            entries = GetAreaPayoutScheduleEntries(qType)
            modeTitle = GetGameModeLabel()
            markedCount = 20
        ElseIf pickCount > 0 Then
            entries = GetPayoutScheduleEntries(pickCount)
            modeTitle = $"Pick {pickCount}"
            markedCount = pickCount
        Else
            RtbPayoutSchedule.Clear()
            GbxPayoutSchedule.Text = "Payout Schedule"
            Return
        End If

        GbxPayoutSchedule.Text = $"Payout — {modeTitle}"

        Dim monoFont As New Font("Consolas", 8.5F)
        Dim boldFont As New Font("Consolas", 8.5F, FontStyle.Bold)

        Try
            Dim rows = entries.Where(Function(kvp) kvp.Value > 0D) _
                              .OrderByDescending(Function(kvp) kvp.Key) _
                              .ToList()

            RtbPayoutSchedule.Clear()

            ' Header
            AppendScheduleLine(boldFont, SystemColors.ControlText, RtbPayoutSchedule.BackColor,
                               "HITS".PadRight(HitsColWidth) & "WIN".PadLeft(WinColWidth))

            ' Separator
            AppendScheduleLine(monoFont, SystemColors.GrayText, RtbPayoutSchedule.BackColor,
                               New String("-"c, HitsColWidth + WinColWidth))

            ' Rows
            Dim showHit = _gameState <> GameState.Idle
            For Each kvp In rows
                Dim winAmount = kvp.Value * betAmount
                Dim hitStr = kvp.Key.ToString().PadRight(HitsColWidth)
                Dim winStr = winAmount.ToString("C2").PadLeft(WinColWidth)
                Dim isWinRow = showHit AndAlso kvp.Key = _lastMatchedCount

                AppendScheduleLine(If(isWinRow, boldFont, monoFont),
                                   If(isWinRow, Color.Black, SystemColors.ControlText),
                                   If(isWinRow, Color.Gold, RtbPayoutSchedule.BackColor),
                                   hitStr & winStr)
            Next

            ' First/Last Ball bonus row
            If ChkFirstLastPlay.Checked AndAlso _selectedQuadrants.Count = 0 AndAlso pickCount > 0 Then
                Dim flBonus = GetFirstLastBallBonus(pickCount)
                If flBonus > 0D Then
                    Dim isFlWin = showHit AndAlso _lastFirstLastBonus > 0D
                    AppendScheduleLine(monoFont, SystemColors.GrayText, RtbPayoutSchedule.BackColor,
                                       New String("-"c, HitsColWidth + WinColWidth))
                    AppendScheduleLine(If(isFlWin, boldFont, monoFont),
                                       If(isFlWin, Color.Black, Color.Teal),
                                       If(isFlWin, Color.Gold, RtbPayoutSchedule.BackColor),
                                       "1ST/LST".PadRight(HitsColWidth) & ("+" & flBonus.ToString("C2")).PadLeft(WinColWidth))
                End If
            End If

            ' Footer separator
            AppendScheduleLine(monoFont, SystemColors.GrayText, RtbPayoutSchedule.BackColor,
                               New String("-"c, HitsColWidth + WinColWidth))

            ' Footer
            Dim hitLabel As String = If(showHit, _lastMatchedCount.ToString(), "-")
            AppendScheduleLine(boldFont, SystemColors.ControlText, RtbPayoutSchedule.BackColor,
                               $"MARKED:{markedCount}  HIT:{hitLabel}")
        Finally
            monoFont.Dispose()
            boldFont.Dispose()
        End Try
    End Sub

    Private Sub AppendScheduleLine(font As Font, foreColor As Color, backColor As Color, text As String)
        RtbPayoutSchedule.SelectionStart = RtbPayoutSchedule.TextLength
        RtbPayoutSchedule.SelectionLength = 0
        RtbPayoutSchedule.SelectionFont = font
        RtbPayoutSchedule.SelectionColor = foreColor
        RtbPayoutSchedule.SelectionBackColor = backColor
        RtbPayoutSchedule.AppendText(text & Environment.NewLine)
    End Sub

End Class
