' Last Edit: 2026-03-16 - Way Ticket fully wired: opens WinWayTicket on check, groups drive payout/wager, clears on number-change/reset.

Class MainWindow

    ' ── colour constants matching WinForms FrmMain.KenoSelection ──────────────
    Private Shared ReadOnly BrushDefault As SolidColorBrush = Freeze(Color.FromRgb(192, 255, 255))  ' #C0FFFF

    Private Shared ReadOnly BrushUserPick As SolidColorBrush = Freeze(Colors.LightGreen)
    Private Shared ReadOnly BrushDraw As SolidColorBrush = Freeze(Colors.LightSkyBlue)
    Private Shared ReadOnly BrushMatch As SolidColorBrush = Freeze(Colors.Gold)
    Private Shared ReadOnly BrushMatchDraw As SolidColorBrush = Freeze(Colors.RoyalBlue)
    Private Shared ReadOnly BrushPowerball As SolidColorBrush = Freeze(Colors.OrangeRed)

    Private ReadOnly _kenoButtons As New Dictionary(Of Integer, Button)(80)
    Private ReadOnly _selectedNumbers As New SortedSet(Of Integer)()
    Private ReadOnly _playedLabels(19) As Label   ' 20 display slots (0-based)
    Private ReadOnly _gamePlayLabels(19) As Label  ' consecutive-game progress cells (0-based)

    Private _bankBalance As Decimal
    Private _jackpotBalance As Decimal
    Private _lastMatches As Integer = -1
    Private _lastPayout As Decimal
    Private _replayNumbers As Integer() = Array.Empty(Of Integer)()
    Private _wayTicketGroups As List(Of List(Of Integer)) = Nothing
    Private _wayTicketKingNumber As Integer = 0

    ' ── session tracking ─────────────────────────────────────────────────────
    Private _sessionStartBalance As Decimal

    Private _sessionGamesPlayed As Integer
    Private _sessionWins As Integer
    Private _sessionBestPayout As Decimal
    Private _sessionTotalWagered As Decimal
    Private _sessionFreeGamesEarned As Integer

    ' ── construction ─────────────────────────────────────────────────────────
    Public Sub New()
        InitializeComponent()
        BuildKenoGrid()
        BuildPlayedGrid()
        BuildGamePlayGrid()
        SbiCpy.Content = cpy
        _bankBalance = GetBankBalance()
        _sessionStartBalance = _bankBalance
        UpdateBankDisplay()
        _jackpotBalance = GetJackpotBalance()
        UpdateJackpotDisplay()
        AllTimeSummaryStore.EnsureAllTimeSummary()
        AllTimeSummaryStore.IncrementSessions()
    End Sub

    ' ── grid builder ─────────────────────────────────────────────────────────
    Private Sub BuildKenoGrid()
        For n = 1 To 40
            Dim num = n
            Dim btn As New Button() With {
                .Content = num.ToString(),
                .Background = BrushDefault,
                .Tag = num,
                .Style = CType(FindResource("KenoNumStyle"), Style)
            }
            AddHandler btn.Click, AddressOf KenoNumber_Click
            _kenoButtons(num) = btn
            KenoGridTop.Children.Add(btn)
        Next

        For n = 41 To 80
            Dim num = n
            Dim btn As New Button() With {
                .Content = num.ToString(),
                .Background = BrushDefault,
                .Tag = num,
                .Style = CType(FindResource("KenoNumStyle"), Style)
            }
            AddHandler btn.Click, AddressOf KenoNumber_Click
            _kenoButtons(num) = btn
            KenoGridBottom.Children.Add(btn)
        Next
    End Sub

    ' ── played-numbers grid builder ───────────────────────────────────────────
    Private Sub BuildPlayedGrid()
        For i = 0 To 19
            Dim lbl As New Label() With {
                .Content = String.Empty,
                .Style = CType(FindResource("NumSelectStyle"), Style)
            }
            _playedLabels(i) = lbl
            KenoGridPlayed.Children.Add(lbl)
        Next
    End Sub

    ' ── game-play progress grid builder ──────────────────────────────────────
    Private Sub BuildGamePlayGrid()
        For i = 0 To 19
            Dim lbl As New Label() With {
                .Content = (i + 1).ToString(),
                .Style = CType(FindResource("GamePlayStyle"), Style)
            }
            _gamePlayLabels(i) = lbl
            TlpGamePlay.Children.Add(lbl)
        Next
    End Sub

    Private Sub UpdateGamePlayDisplay(gameIndex As Integer)
        For i = 0 To 19
            _gamePlayLabels(i).Background =
                If(i < gameIndex - 1, BrushUserPick,
                   If(i = gameIndex - 1, BrushMatch,
                      SystemColors.ControlBrush))
        Next
    End Sub

    ' Sets the result cell for a single completed game.
    Private Sub UpdateGamePlayCell(gameIndex As Integer, isWinner As Boolean,
                                   firstMatched As Boolean, lastMatched As Boolean)
        Dim cell = _gamePlayLabels(gameIndex - 1)
        cell.Background = If(isWinner, BrushMatch, BrushUserPick)

        Dim tb As New TextBlock With {
            .HorizontalAlignment = HorizontalAlignment.Center,
            .VerticalAlignment = VerticalAlignment.Center
        }
        If Not firstMatched AndAlso Not lastMatched Then
            tb.Inlines.Add(New Run(gameIndex.ToString()) With {.Foreground = Brushes.Black})
        End If
        If firstMatched Then
            tb.Inlines.Add(New Run("♥") With {.Foreground = Brushes.Red})
        End If
        If lastMatched Then
            tb.Inlines.Add(New Run("♦") With {.Foreground = Brushes.Blue})
        End If
        cell.Content = tb
    End Sub

    Private Sub ResetGamePlayDisplay()
        For i = 0 To 19
            _gamePlayLabels(i).Content = (i + 1).ToString()
            _gamePlayLabels(i).Background = SystemColors.ControlBrush
        Next
    End Sub

    Private Sub UpdatePlayedGrid()
        Dim sorted = _selectedNumbers.ToArray()
        For i = 0 To 19
            _playedLabels(i).Content = If(i < sorted.Length, sorted(i).ToString(), String.Empty)
        Next
    End Sub

    ' ── number click ─────────────────────────────────────────────────────────
    Private Sub KenoNumber_Click(sender As Object, e As RoutedEventArgs)
        If TypeOf sender IsNot Button Then Return
        Dim btn = CType(sender, Button)
        Dim num = CInt(btn.Tag)

        If _selectedNumbers.Remove(num) Then
            btn.Background = BrushDefault
        ElseIf _selectedNumbers.Count < 20 Then
            _selectedNumbers.Add(num)
            btn.Background = BrushUserPick
        End If

        If ChkWayTicket.IsChecked = True Then
            ChkWayTicket.IsChecked = False
        End If

        UpdatePayoutScheduleDisplay()
        UpdatePlayedGrid()
        UpdateStatusBar()
        UpdateWayTicketSummary()
        UpdateWagerPreview()
    End Sub

    ' ── helpers
    Private Shared Function Freeze(color As Color) As SolidColorBrush
        Dim b As New SolidColorBrush(color)
        b.Freeze()
        Return b
    End Function

    Private Sub ResetGrid()
        If ChkWayTicket IsNot Nothing AndAlso ChkWayTicket.IsChecked = True Then
            ChkWayTicket.IsChecked = False
        End If
        _wayTicketGroups = Nothing
        _wayTicketKingNumber = 0
        For Each kvp In _kenoButtons
            kvp.Value.Background = BrushDefault
        Next
        _selectedNumbers.Clear()
        _lastMatches = -1
        _lastPayout = 0D
        LblWinnings.Content = "$0"
        LblWagerTotal.Content = "$0"
        ResetGamePlayDisplay()
        UpdatePlayedGrid()
        UpdateStatusBar()
    End Sub

    ' ── status bar ───────────────────────────────────────────────────────────
    Private Sub UpdateStatusBar()
        SbiPicks.Content = $"Picks: {_selectedNumbers.Count}"
        SbiMatches.Content = $"Matches: {If(_lastMatches < 0, "—", _lastMatches.ToString())}"
        SbiPayout.Content = $"Payout: {_lastPayout:C2}"
    End Sub

    ' ── payout schedule ──────────────────────────────────────────────────────
    Private Const HitsColWidth As Integer = 8

    Private Const WinColWidth As Integer = 19

    Private Sub UpdatePayoutScheduleDisplay()
        Dim pickCount = _selectedNumbers.Count

        If pickCount = 0 Then
            RtbPayoutSchedule.Document.Blocks.Clear()
            TbkPayoutHeader.Text = "Payout Schedule"
            Return
        End If

        Dim betAmount = GetSelectedBetAmount()
        If betAmount <= 0D Then betAmount = 1D
        Dim entries = GetPayoutScheduleEntries(pickCount)
        TbkPayoutHeader.Text = $"Payout — Pick {pickCount}"

        Dim doc As New FlowDocument() With {
            .PagePadding = New Thickness(4),
            .LineHeight = 14
        }

        Dim rows = entries.Where(Function(kvp) kvp.Value > 0D) _
                          .OrderByDescending(Function(kvp) kvp.Key) _
                          .ToList()

        doc.Blocks.Add(MakeScheduleLine(
            "HITS".PadRight(HitsColWidth) & "WIN".PadLeft(WinColWidth),
            Brushes.Black, bold:=True))

        doc.Blocks.Add(MakeScheduleLine(
            New String("-"c, HitsColWidth + WinColWidth),
            Brushes.Gray, bold:=False))

        For Each kvp In rows
            Dim winAmount = kvp.Value * betAmount
            Dim line = kvp.Key.ToString().PadRight(HitsColWidth) &
                       winAmount.ToString("C2").PadLeft(WinColWidth)
            doc.Blocks.Add(MakeScheduleLine(line, Brushes.Black, bold:=False))
        Next

        doc.Blocks.Add(MakeScheduleLine(
            New String("-"c, HitsColWidth + WinColWidth),
            Brushes.Gray, bold:=False))

        Dim hitText = If(_lastMatches < 0, "-", _lastMatches.ToString())
        doc.Blocks.Add(MakeScheduleLine(
            $"MARKED:{pickCount}  HIT:{hitText}",
            Brushes.Black, bold:=True))

        RtbPayoutSchedule.Document = doc
    End Sub

    Private Shared Function MakeScheduleLine(text As String, fg As Brush, bold As Boolean) As Paragraph
        Dim run As New Run(text) With {
            .Foreground = fg,
            .FontWeight = If(bold, FontWeights.Bold, FontWeights.Normal)
        }
        Return New Paragraph(run) With {
            .Margin = New Thickness(0),
            .Padding = New Thickness(0),
            .LineHeight = 14
        }
    End Function

    ' ── bank display ─────────────────────────────────────────────────────────
    Private Sub UpdateBankDisplay()
        LblBank.Content = _bankBalance.ToString("C2")
    End Sub

    Private Sub UpdateJackpotDisplay()
        TbkProgressiveJackpot.Text = $"Progressive Jackpot: {_jackpotBalance:C0}"
    End Sub

    Private Sub UpdateWagerPreview()
        Dim bet = GetSelectedBetAmount()
        Dim games = GetConsecutiveGamesCount()

        If bet <= 0D OrElse _selectedNumbers.Count = 0 Then
            LblWagerTotal.Content = "$0"
            Return
        End If

        LblWagerTotal.Content = ComputeTotalWager(bet, games).ToString("C2")
    End Sub

    ' ── play / clear ─────────────────────────────────────────────────────────
    Private Async Sub BtnPlay_Click(sender As Object, e As RoutedEventArgs)
        If _selectedNumbers.Count = 0 Then Return
        _replayNumbers = _selectedNumbers.ToArray()

        Dim bet = GetSelectedBetAmount()
        If bet <= 0D Then Return

        Dim gamesToPlay = GetConsecutiveGamesCount()
        Dim useMultiplier = ChkMultiplierKeno.IsChecked = True
        Dim useFirstLast = ChkFirstLastPlay.IsChecked = True
        Dim useWayTicket = ChkWayTicket.IsChecked = True
        Dim usePowerball = ChkPowerball.IsChecked = True

        Dim totalWager = ComputeTotalWager(bet, gamesToPlay)
        Dim totalPayout = 0D
        Dim lastMatches = 0
        Dim gameResults As New List(Of (Index As Integer, Matched As Integer, Payout As Decimal, Multiplier As Integer, FirstLastBonus As Decimal))()

        Dim isSuperSonic = (GetDrawDelayMs() = 0)
        BtnPlay.IsEnabled = False
        BtnCLEAR.IsEnabled = False
        ResetGamePlayDisplay()
        Try
            For i = 1 To gamesToPlay
                Dim result = Await DrawSingleGameAnimated()

                Dim gamePayout As Decimal
                If useWayTicket Then
                    gamePayout = GetWayTicketPayout(result.Draw, bet)
                Else
                    gamePayout = GetPayout(_selectedNumbers.Count, result.Matches) * bet
                End If

                Dim currentMultiplier = 1
                If useMultiplier Then
                    currentMultiplier = DrawMultiplier()
                    gamePayout *= currentMultiplier
                    ChkMultiplierKeno.Content = $"Multiplier: {currentMultiplier}×"
                End If

                If usePowerball Then
                    Dim pbNum = Random.Shared.Next(1, 81)
                    LblPowerballValue.Content = pbNum.ToString()
                    If _selectedNumbers.Contains(pbNum) Then gamePayout *= 4
                End If

                totalPayout += gamePayout
                AllTimeSummaryStore.RecordGame(totalWager / gamesToPlay, gamePayout)
                lastMatches = result.Matches
                gameResults.Add((i, result.Matches, gamePayout, currentMultiplier, 0D))
                UpdateGamePlayCell(i, gamePayout > 0D,
                                   _selectedNumbers.Contains(result.FirstDrawn),
                                   _selectedNumbers.Contains(result.LastDrawn))

                ' ── progressive jackpot contribution ───────────────────────────
                If bet >= 5D Then
                    AddToJackpot(bet * 0.05D)
                    _jackpotBalance = GetJackpotBalance()
                    UpdateJackpotDisplay()
                End If

                ' ── progressive jackpot win check ──────────────────────────────
                If _selectedNumbers.Count >= 8 AndAlso result.Matches = _selectedNumbers.Count Then
                    _bankBalance += _jackpotBalance
                    SaveBankBalance(_bankBalance)
                    UpdateBankDisplay()
                    MessageBox.Show($"JACKPOT! You matched all {_selectedNumbers.Count} numbers and won {_jackpotBalance:C0}!",
                                    "Progressive Jackpot Winner!", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                    ResetJackpot()
                    AllTimeSummaryStore.RecordJackpotWon()
                    _jackpotBalance = GetJackpotBalance()
                    UpdateJackpotDisplay()
                End If

                ' Between consecutive games: winner dialog controls the pause; otherwise wait 3 s
                If i < gamesToPlay AndAlso Not isSuperSonic Then
                    If gamePayout > 0D Then
                        WinPayoutSchedule.ShowForWin(Me, "Regular", _selectedNumbers.Count, result.Matches, gamePayout, bet)
                    Else
                        Await Task.Delay(3000)
                    End If
                End If
            Next
        Finally
            BtnPlay.IsEnabled = True
            BtnCLEAR.IsEnabled = True
        End Try

        Dim subtotal = totalPayout
        Dim bonus = GetConsecutiveBonus()
        totalPayout *= bonus
        _bankBalance = _bankBalance - totalWager + totalPayout
        SaveBankBalance(_bankBalance)
        UpdateBankDisplay()

        ' Update session stats
        _sessionGamesPlayed += gamesToPlay
        If totalPayout > 0D Then _sessionWins += 1
        If totalPayout > _sessionBestPayout Then _sessionBestPayout = totalPayout
        _sessionTotalWagered += totalWager

        _lastMatches = lastMatches
        _lastPayout = totalPayout
        LblWinnings.Content = totalPayout.ToString("C2")
        LblWagerTotal.Content = totalWager.ToString("C2")
        UpdatePayoutScheduleDisplay()
        UpdateStatusBar()

        ' ── game log ──────────────────────────────────────────────────────────
        Dim isFreeGame = (lastMatches = 0 AndAlso _selectedNumbers.Count >= 5)
        If gamesToPlay > 1 Then
            AppendBatch("Regular", bet,
                        gameResults.Select(Function(r) (r.Matched, r.Payout, False, r.Multiplier, r.FirstLastBonus)),
                        bonus)
        Else
            AppendGame("Regular", bet, lastMatches, totalPayout,
                       If(gameResults.Count > 0, gameResults(0).Multiplier, 1),
                       isFreeGame, 0D)
        End If

        ' ── Show result dialogs ────────────────────────────────────────────
        Try
            If gamesToPlay > 1 Then
                WinConsecutiveSummary.ShowSummary(Me, gameResults, bet, bonus, subtotal, totalPayout)
            ElseIf totalPayout > 0D Then
                WinPayoutSchedule.ShowForWin(Me, "Regular", _selectedNumbers.Count, lastMatches, totalPayout, bet)
            End If
        Catch ex As Exception
            MessageBox.Show($"Result dialog error: {ex.Message}{Environment.NewLine}{ex.StackTrace}",
                            "Result Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try

        If lastMatches = 0 AndAlso _selectedNumbers.Count >= 5 Then
            _sessionFreeGamesEarned += 1
            AllTimeSummaryStore.RecordFreeGameEarned()
            _bankBalance += 2D
            SaveBankBalance(_bankBalance)
            UpdateBankDisplay()
            WinBonusGame.ShowBonus(Me, _selectedNumbers.Count)
        End If
    End Sub

    Private Async Function DrawSingleGameAnimated() As Task(Of (Draw As HashSet(Of Integer), Matches As Integer, FirstDrawn As Integer, LastDrawn As Integer))
        Dim pool As New List(Of Integer)(80)
        For i = 1 To 80 : pool.Add(i) : Next

        Dim drawOrder As New List(Of Integer)(20)
        Dim drawSet As New HashSet(Of Integer)()

        For Each kvp In _kenoButtons
            kvp.Value.Background = If(_selectedNumbers.Contains(kvp.Key), BrushUserPick, BrushDefault)
        Next

        Dim delay = GetDrawDelayMs()
        Do While drawOrder.Count < 20
            Dim idx = Random.Shared.Next(pool.Count)
            Dim num = pool(idx)
            pool.RemoveAt(idx)
            drawOrder.Add(num)
            drawSet.Add(num)
            _kenoButtons(num).Background = If(_selectedNumbers.Contains(num), BrushMatch, BrushDraw)
            If delay > 0 Then Await Task.Delay(delay)
        Loop

        Dim matches = drawOrder.Where(Function(n) _selectedNumbers.Contains(n)).Count()

        Return (drawSet, matches, drawOrder(0), drawOrder(19))
    End Function

    ' Returns full cost for all games: base bet + $1/game for Multiplier, $1/game for First/Last,
    ' (subTickets-1)*bet/game for Way Ticket.  Powerball is a payout multiplier — no extra wager.
    Private Function ComputeTotalWager(bet As Decimal, gamesToPlay As Integer) As Decimal
        Dim perGame = bet
        If ChkMultiplierKeno.IsChecked = True Then perGame += 1D
        If ChkFirstLastPlay.IsChecked = True Then perGame += 1D
        If ChkWayTicket.IsChecked = True Then
            Dim subTickets = If(_wayTicketGroups IsNot Nothing AndAlso _wayTicketGroups.Count > 0,
                                _wayTicketGroups.Count,
                                Math.Max(1, _selectedNumbers.Count \ 3))
            perGame += (subTickets - 1) * bet
        End If

        Return perGame * gamesToPlay
    End Function

    ' Splits picks into groups of 3, evaluates each sub-ticket against the draw, sums the payouts.
    Private Function GetWayTicketPayout(draw As HashSet(Of Integer), bet As Decimal) As Decimal
        If _wayTicketGroups Is Nothing OrElse _wayTicketGroups.Count = 0 Then Return 0D
        Dim total = 0D
        For Each group In _wayTicketGroups
            Dim matches = group.Where(Function(n) draw.Contains(n)).Count()
            total += GetPayout(group.Count, matches) * bet
        Next
        Return total
    End Function

    Private Sub ApplyDrawToGrid(draw As HashSet(Of Integer))
        For Each kvp In _kenoButtons
            Dim num = kvp.Key
            Dim inPick = _selectedNumbers.Contains(num)
            Dim inDraw = draw.Contains(num)
            kvp.Value.Background = If(inPick AndAlso inDraw, BrushMatch,
                                    If(inPick, BrushUserPick,
                                    If(inDraw, BrushDraw,
                                       BrushDefault)))
        Next
    End Sub

    Private Sub BtnClear_Click(sender As Object, e As RoutedEventArgs)
        ResetGrid()
    End Sub

    ' ── consecutive games ──────────────────────────────────────────────────
    Private Function GetConsecutiveRadioButtons() As RadioButton()
        Return {RbConsecutive2, RbConsecutive3, RbConsecutive4, RbConsecutive5,
                RbConsecutive6, RbConsecutive7, RbConsecutive8, RbConsecutive9,
                RbConsecutive10, RbConsecutive11, RbConsecutive12, RbConsecutive13,
                RbConsecutive14, RbConsecutive15, RbConsecutive16, RbConsecutive17,
                RbConsecutive18, RbConsecutive19, RbConsecutive20}
    End Function

    Private Function GetConsecutiveGamesCount() As Integer
        For Each rb In GetConsecutiveRadioButtons()
            If rb IsNot Nothing AndAlso rb.IsChecked = True Then Return CInt(rb.Tag)
        Next

        Return 1
    End Function

    Private Function GetConsecutiveBonus() As Decimal
        Dim count = GetConsecutiveGamesCount()
        If count > 16 Then Return 1.75D
        If count >= 12 Then Return 1.5D
        If count >= 8 Then Return 1.25D
        If count >= 5 Then Return 1.1D

        Return 1D
    End Function

    Private Sub UpdateBonusDisplay()
        Dim bonus = GetConsecutiveBonus()
        TbkBonusMultiplier.Text = $"Bonus: {bonus}×"
    End Sub

    Private Sub RbConsecutive_Checked(sender As Object, e As RoutedEventArgs)
        UpdateBonusDisplay()
        UpdateWagerPreview()
    End Sub

    ' ── wager ───────────────────────────────────────────────────────────────
    Private Function GetWagerRadioButtons() As RadioButton()
        Return {RbWager1, RbWager2, RbWager3, RbWager5, RbWager10, RbWager15, RbWager20,
                RbWager25, RbWager30, RbWager40, RbWager50, RbWager100, RbWager150, RbWager200}
    End Function

    Private Function GetSelectedBetAmount() As Decimal
        For Each rb In GetWagerRadioButtons()
            If rb.IsChecked = True Then Return CDec(rb.Tag)
        Next

        Dim custom As Decimal
        If Decimal.TryParse(TxtSpecialWager.Text, custom) AndAlso custom > 0D Then
            Return custom
        End If

        Return 0D
    End Function

    Private Sub RbWager_Checked(sender As Object, e As RoutedEventArgs)
        If TxtSpecialWager IsNot Nothing Then TxtSpecialWager.Text = "0"
        UpdatePayoutScheduleDisplay()
        UpdateWayTicketSummary()
        UpdateWagerPreview()
    End Sub

    Private Sub TxtSpecialWager_TextChanged(sender As Object, e As TextChangedEventArgs)
        Dim custom As Decimal
        If Decimal.TryParse(TxtSpecialWager.Text, custom) AndAlso custom > 0D Then
            For Each rb In GetWagerRadioButtons()
                rb.IsChecked = False
            Next
        End If

        UpdatePayoutScheduleDisplay()
        UpdateWagerPreview()
    End Sub

    ' ── draw speed
    Private Sub RbDrawSpeed_Checked(sender As Object, e As RoutedEventArgs)
    End Sub

    Private Function GetDrawDelayMs() As Integer
        For Each rb In {RbDrawSpeedSlow, RbDrawSpeedMedium, RbDrawSpeedFast, RbDrawSpeedSS}
            If rb.IsChecked = True Then Return CInt(rb.Tag)
        Next

        Return 500
    End Function

    ' ── save favorites ───────────────────────────────────────────────────────────────────────────
    Private Sub BtnSaveFavorites_Click(sender As Object, e As RoutedEventArgs)
        If _selectedNumbers.Count = 0 Then
            MessageBox.Show("No numbers selected to save.", "Save Favorites",
                            MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If

        Dim slot = WinFavoritesSlot.ShowForSave(Me, _selectedNumbers.Count)
        If slot < 0 Then Return

        SaveFavorites(slot, _selectedNumbers)
        MessageBox.Show($"Saved {_selectedNumbers.Count} numbers to Slot {slot + 1}.", "Save Favorites",
                        MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub

    ' ── special play ─────────────────────────────────────────────────────────────────────────────
    Private Sub BtnQuickPick_Click(sender As Object, e As RoutedEventArgs)
        Dim n As Integer
        If Not Integer.TryParse(TxtQuickPickCount.Text, n) Then n = 1
        n = Math.Max(1, Math.Min(20, n))

        ResetGrid()
        Dim pool As New List(Of Integer)()
        For i = 1 To 80 : pool.Add(i) : Next
        Do While _selectedNumbers.Count < n
            Dim idx = Random.Shared.Next(pool.Count)
            Dim num = pool(idx)
            pool.RemoveAt(idx)
            _selectedNumbers.Add(num)
            _kenoButtons(num).Background = BrushUserPick
        Loop

        UpdatePayoutScheduleDisplay()
        UpdatePlayedGrid()
        UpdateStatusBar()
        UpdateWayTicketSummary()
        UpdateWagerPreview()
    End Sub

    Private Sub TxtQuickPickCount_TextChanged(sender As Object, e As TextChangedEventArgs)
        Dim val As Integer
        If Not Integer.TryParse(TxtQuickPickCount.Text, val) Then Return
        val = Math.Max(1, Math.Min(20, val))
        If TxtQuickPickCount.Text <> val.ToString() Then
            TxtQuickPickCount.Text = val.ToString()
            TxtQuickPickCount.CaretIndex = TxtQuickPickCount.Text.Length
        End If
    End Sub

    Private Sub ChkMultiplierKeno_Changed(sender As Object, e As RoutedEventArgs)
        If ChkMultiplierKeno.IsChecked <> True Then
            ChkMultiplierKeno.Content = "Multiplier: 1×"
        End If
        UpdateWagerPreview()
    End Sub

    ' Weighted draw: x1=45%, x2=30%, x3=13%, x5=9%, x10=3%
    Private Shared Function DrawMultiplier() As Integer
        Dim roll = Random.Shared.Next(100)
        If roll < 45 Then Return 1
        If roll < 75 Then Return 2
        If roll < 88 Then Return 3
        If roll < 97 Then Return 5
        Return 10
    End Function

    Private Sub ChkFirstLastPlay_Changed(sender As Object, e As RoutedEventArgs)
        ' First/Last side bet resolved during game play
        UpdateWagerPreview()
    End Sub

    Private Sub ChkWayTicket_Changed(sender As Object, e As RoutedEventArgs)
        If ChkWayTicket.IsChecked = True Then
            If _selectedNumbers.Count = 0 Then
                MessageBox.Show("Select your numbers first, then enable Way Ticket.",
                                "Way Ticket", MessageBoxButton.OK, MessageBoxImage.Information)
                ChkWayTicket.IsChecked = False
                Return
            End If
            Dim result = WinWayTicket.ShowAssignment(Me, _selectedNumbers.ToList(), GetSelectedBetAmount())
            If result IsNot Nothing Then
                _wayTicketGroups = result.Groups
                _wayTicketKingNumber = result.KingNumber
            Else
                ChkWayTicket.IsChecked = False
                _wayTicketGroups = Nothing
                _wayTicketKingNumber = 0
            End If
        Else
            _wayTicketGroups = Nothing
            _wayTicketKingNumber = 0
        End If
        UpdateWayTicketSummary()
        UpdateWagerPreview()
    End Sub

    Private Sub UpdateWayTicketSummary()
        If ChkWayTicket Is Nothing Then Return
        If ChkWayTicket.IsChecked <> True OrElse _wayTicketGroups Is Nothing OrElse _wayTicketGroups.Count = 0 Then
            LblWayTicketSummary.Content = "—"
            Return
        End If
        Dim subTickets = _wayTicketGroups.Count
        Dim total = subTickets * GetSelectedBetAmount()
        LblWayTicketSummary.Content = $"{subTickets} sub-tickets @ {total:C2}"
    End Sub

    Private Sub ChkPowerball_Changed(sender As Object, e As RoutedEventArgs)
        LblPowerballValue.Content = If(ChkPowerball.IsChecked = True, "Active", "—")
    End Sub

    Private Sub BtnBullseye_Click(sender As Object, e As RoutedEventArgs)
        MessageBox.Show("Bullseye is not yet available.", "Bullseye",
                        MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub

    Private Sub BtnPlayFavorites_Click(sender As Object, e As RoutedEventArgs)
        Dim slot = WinFavoritesSlot.ShowForLoad(Me)
        If slot < 0 Then Return

        Dim favs = LoadFavorites(slot)
        If favs.Length = 0 Then
            MessageBox.Show("No favorites saved in that slot. Pick numbers and click Save My Favorites first.",
                            "Play Favorites", MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        ResetGrid()
        For Each n In favs.Take(20)

            Dim value As Button = Nothing

            If _kenoButtons.TryGetValue(n, value) Then
                _selectedNumbers.Add(n)
                value.Background = BrushUserPick
            End If
        Next
        UpdatePayoutScheduleDisplay()
        UpdatePlayedGrid()
        UpdateStatusBar()
        UpdateWayTicketSummary()
        UpdateWagerPreview()
    End Sub

    Private Sub BtnReplayLastGame_Click(sender As Object, e As RoutedEventArgs)
        If _replayNumbers.Length = 0 Then
            MessageBox.Show("No previous game to replay.", "Replay Last Game",
                            MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        ResetGrid()
        For Each n In _replayNumbers
            Dim value As Button = Nothing
            If _kenoButtons.TryGetValue(n, value) Then
                _selectedNumbers.Add(n)
                _kenoButtons(n).Background = BrushUserPick
            End If
        Next
        UpdatePayoutScheduleDisplay()
        UpdatePlayedGrid()
        UpdateStatusBar()
        UpdateWayTicketSummary()
        UpdateWagerPreview()
    End Sub

    Private Sub BtnQuadrant_Click(sender As Object, e As RoutedEventArgs)
        If TypeOf sender IsNot Button Then Return
        Dim btn = CType(sender, Button)
        Dim quadrant = CInt(btn.Tag)
        ResetGrid()
        For Each n In GetQuadrantNumbers(quadrant)
            _selectedNumbers.Add(n)
            _kenoButtons(n).Background = BrushUserPick
        Next
        UpdatePayoutScheduleDisplay()
        UpdatePlayedGrid()
        UpdateStatusBar()
        UpdateWayTicketSummary()
        UpdateWagerPreview()
    End Sub

    Private Shared Function GetQuadrantNumbers(quadrant As Integer) As Integer()
        Dim rowStart = If(quadrant <= 2, 0, 4)
        Dim colStart = If(quadrant Mod 2 = 1, 0, 5)
        Dim result(19) As Integer
        Dim idx = 0
        For r = 0 To 3
            For c = 0 To 4
                result(idx) = (rowStart + r) * 10 + (colStart + c) + 1
                idx += 1
            Next
        Next
        Return result
    End Function

    ' ── status bar actions

    Private Sub SbiHelp_MouseDown(sender As Object, e As MouseButtonEventArgs)
        WinKenoHelp.ShowWindow(Me)
    End Sub

    Private Sub SbiGameLog_MouseDown(sender As Object, e As MouseButtonEventArgs)
        WinGameLog.ShowWindow(Me)
    End Sub

    Private Sub SbiSummary_MouseDown(sender As Object, e As MouseButtonEventArgs)
        WinAllTimeSummary.ShowHistory(Me)
    End Sub

    Private Sub SbiProgressiveJackpot_MouseDown(sender As Object, e As MouseButtonEventArgs)
        MessageBox.Show($"Current Jackpot: {_jackpotBalance:C0}" & Environment.NewLine & Environment.NewLine &
                        "Win by matching ALL numbers on any 8-spot or greater ticket." & Environment.NewLine &
                        "5% of every wager ≥$5 contributes to the jackpot." & Environment.NewLine &
                        "Jackpot resets to $1,000 when won.",
                        "Progressive Jackpot", MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        If _sessionGamesPlayed = 0 Then Return
        Dim netPL = _bankBalance - _sessionStartBalance
        WinSessionSummary.ShowSummary(Me, netPL, _sessionGamesPlayed, _sessionWins,
                                      _sessionBestPayout, _sessionFreeGamesEarned, _sessionTotalWagered)
    End Sub

End Class