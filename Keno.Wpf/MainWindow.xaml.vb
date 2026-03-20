' Last Edit: 2026-03-20 05:36 AM - BtnFreeGames_Click: one click now stages all available free games (up to 10) instead of one per click; fixes regular-game path running when queue was not staged.

Class MainWindow

    ' ── colour constants matching WinForms FrmMain.KenoSelection ──────────────
    Private Shared ReadOnly BrushDefault As SolidColorBrush = Freeze(Color.FromRgb(192, 255, 255))  ' #C0FFFF

    Private Shared ReadOnly BrushPlayedDefault As SolidColorBrush = Freeze(Color.FromRgb(245, 245, 245))  ' #F5F5F5

    Private Shared ReadOnly BrushUserPick As SolidColorBrush = Freeze(Colors.LightGreen)
    Private Shared ReadOnly BrushDraw As SolidColorBrush = Freeze(Colors.LightSkyBlue)
    Private Shared ReadOnly BrushMatch As SolidColorBrush = Freeze(Colors.Gold)
    Private Shared ReadOnly BrushMatchDraw As SolidColorBrush = Freeze(Colors.RoyalBlue)
    Private Shared ReadOnly BrushPowerball As SolidColorBrush = Freeze(Colors.OrangeRed)

    Private ReadOnly _kenoButtons As New Dictionary(Of Integer, Button)(80)
    Private ReadOnly _selectedNumbers As New SortedSet(Of Integer)()
    Private ReadOnly _playedLabels(19) As Label   ' 20 display slots (0-based)
    Private ReadOnly _gamePlayLabels(19) As Label  ' consecutive-game progress cells (0-based)
    Private ReadOnly _freeGameCells(9) As Label    ' free-game queue slots (0-based, 10 cells)
    Private _freeGamesQueued As Integer = 0        ' number of free games the user has staged to play

    Private _bankBalance As Decimal
    Private _jackpotBalance As Decimal
    Private _lastMatches As Integer = -1
    Private _lastPayout As Decimal
    Private _replayNumbers As Integer() = Array.Empty(Of Integer)()
    Private _wayTicketGroups As List(Of List(Of Integer)) = Nothing
    Private _wayTicketKingNumber As Integer = 0
    Private _isBullseyeActive As Boolean = False
    Private Shared ReadOnly BullseyeNumbers As Integer() = {1, 10, 35, 36, 45, 46, 71, 80}

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
        BuildFreeGamesGrid()
        SbiCpy.Content = cpy
        _bankBalance = GetBankBalance()
        _sessionStartBalance = _bankBalance
        UpdateBankDisplay()
        _jackpotBalance = GetJackpotBalance()
        UpdateJackpotDisplay()
        AllTimeSummaryStore.EnsureAllTimeSummary()
        AllTimeSummaryStore.IncrementSessions()
        DrawStatsStore.EnsureDrawStats()
        RefreshStatsDisplay(DrawStatsStore.LoadStats())
        UpdateFreeGamesButton()
        Dim appSettings = LoadAppSettings()
        NudRepopulateAmount.Value = CDbl(If(appSettings.RepopulateAmount > 0D, appSettings.RepopulateAmount, 10000D))
        ChkFlyoutsEnabled.IsChecked = appSettings.FlyoutsEnabled
        Dim speedRadios = {RbDrawSpeedSlow, RbDrawSpeedMedium, RbDrawSpeedFast, RbDrawSpeedSS}
        speedRadios(Math.Max(0, Math.Min(appSettings.DrawSpeedIndex, speedRadios.Length - 1))).IsChecked = True
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

    ' ── free-games queue grid builder ────────────────────────────────────────
    Private Sub BuildFreeGamesGrid()
        For i = 0 To 9
            Dim lbl As New Label() With {
                .Content = (i + 1).ToString(),
                .Style = CType(FindResource("GamePlayStyle"), Style)
            }
            _freeGameCells(i) = lbl
            FreeGamesPlay.Children.Add(lbl)
        Next
    End Sub

    ' Refreshes all 10 free-game queue cells; queued slots are Gold, empty slots default.
    Private Sub UpdateFreeGamesGrid()
        For i = 0 To 9
            _freeGameCells(i).Content = (i + 1).ToString()
            _freeGameCells(i).Background = If(i < _freeGamesQueued, Brushes.Gold, SystemColors.ControlBrush)
        Next
    End Sub

    ' Marks a single cell (0-based) with a win/loss colour after it has been played.
    Private Sub UpdateFreeGameCellResult(cellIndex As Integer, isWin As Boolean)
        If cellIndex < 0 OrElse cellIndex > 9 Then Return
        _freeGameCells(cellIndex).Background = If(isWin, Brushes.LightGreen, Brushes.LightCoral)
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
            _playedLabels(i).Background = BrushPlayedDefault
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

        If _isBullseyeActive Then
            _isBullseyeActive = False
            BtnBullseye.Background = Brushes.MistyRose
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
        _isBullseyeActive = False
        BtnBullseye.Background = Brushes.MistyRose
        For Each kvp In _kenoButtons
            kvp.Value.Background = BrushDefault
        Next
        _selectedNumbers.Clear()
        _lastMatches = -1
        _lastPayout = 0D
        LblWinnings.Content = "$0"
        LblWagerTotal.Content = "$0"
        ResetGamePlayDisplay()
        _freeGamesQueued = 0
        UpdateFreeGamesGrid()
        UpdatePlayedGrid()
        UpdateStatusBar()
        UpdateQuadrantButtonStates()
    End Sub

    ' ── status bar ───────────────────────────────────────────────────────────
    Private Sub UpdateStatusBar()
        SbiPicks.Content = $"Picks: {_selectedNumbers.Count}"
        SbiMatches.Content = $"Matches: {If(_lastMatches < 0, "—", _lastMatches.ToString())}"
        SbiPayout.Content = $"Payout: {_lastPayout:C2}"
    End Sub

    ' ── hot / cold numbers + streaks ─────────────────────────────────────────
    Private Sub UpdateDrawStats(draw As HashSet(Of Integer), won As Boolean)
        Try
            Dim stats = RecordDraw(draw.ToList(), won)
            RefreshStatsDisplay(stats)
        Catch ex As Exception
            LogError(ex, NameOf(UpdateDrawStats))
        End Try
    End Sub

    Private Sub RefreshStatsDisplay(stats As DrawStatsStore.DrawStats)
        ' ── streaks ──────────────────────────────────────────────────────────
        If stats.CurrentWinStreak > 0 Then
            SbiCurrentStreak.Content = $"Win Streak: {stats.CurrentWinStreak}"
            SbiCurrentStreak.Foreground = Brushes.ForestGreen
        ElseIf stats.CurrentLossStreak > 0 Then
            SbiCurrentStreak.Content = $"Loss Streak: {stats.CurrentLossStreak}"
            SbiCurrentStreak.Foreground = Brushes.Firebrick
        Else
            SbiCurrentStreak.Content = "Streak: —"
            SbiCurrentStreak.Foreground = SystemColors.ControlTextBrush
        End If

        SbiBestStreak.Content = $"Best: {stats.BestWinStreak}W"
        SbiBestStreak.Foreground = SystemColors.ControlTextBrush

        ' ── hot / cold — require 5+ games in the window ───────────────────
        If stats.GamesPlayed < 5 Then Return

        Dim counts As New Dictionary(Of Integer, Integer)()
        For n = 1 To 80
            counts(n) = 0
        Next
        For Each draw In stats.RecentDraws
            For Each num In draw
                If num >= 1 AndAlso num <= 80 Then counts(num) += 1
            Next
        Next

        Dim ordered = counts.OrderByDescending(Function(kv) kv.Value).ThenBy(Function(kv) kv.Key).ToList()
        Dim hot = ordered.Take(5).Select(Function(kv) kv.Key).ToArray()
        Dim cold = counts.OrderBy(Function(kv) kv.Value).ThenBy(Function(kv) kv.Key).Take(5).Select(Function(kv) kv.Key).ToArray()

        Dim hotTbks = {TbkHot1, TbkHot2, TbkHot3, TbkHot4, TbkHot5}
        Dim coldTbks = {TbkCold1, TbkCold2, TbkCold3, TbkCold4, TbkCold5}
        For i = 0 To 4
            hotTbks(i).Text = hot(i).ToString()
            coldTbks(i).Text = cold(i).ToString()
        Next
    End Sub

    Private Sub HotColdNum_Click(sender As Object, e As MouseButtonEventArgs)
        If Not BtnPlay.IsEnabled Then Return
        Dim brd = TryCast(sender, Border)
        If brd Is Nothing Then Return
        Dim tbk = TryCast(brd.Child, TextBlock)
        If tbk Is Nothing Then Return

        Dim number As Integer
        If Not Integer.TryParse(tbk.Text, number) Then Return

        Dim value As Button = Nothing
        If Not _kenoButtons.TryGetValue(number, value) Then Return

        If _selectedNumbers.Remove(number) Then
            value.Background = BrushDefault
        ElseIf _selectedNumbers.Count < 20 Then
            _selectedNumbers.Add(number)
            _kenoButtons(number).Background = BrushUserPick
        Else
            Return
        End If

        UpdatePayoutScheduleDisplay()
        UpdatePlayedGrid()
        UpdateStatusBar()
        UpdateWayTicketSummary()
        UpdateWagerPreview()
    End Sub

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
        Dim halfType = GetHalfType()
        Dim entries = If(_isBullseyeActive, GetBullseyePayoutScheduleEntries(),
                         If(halfType IsNot Nothing, GetAreaPayoutScheduleEntries(halfType),
                            GetPayoutScheduleEntries(pickCount)))
        TbkPayoutHeader.Text = If(_isBullseyeActive, "Payout — Bullseye",
                               If(halfType = "TopBottom", "Payout — Top/Bottom Half",
                               If(halfType = "LeftRight", "Payout — Left/Right Half",
                                  $"Payout — Pick {pickCount}")))

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
        If NudSpecialWager IsNot Nothing Then
            NudSpecialWager.Maximum = Math.Min(5000D, CDbl(_bankBalance))
        End If
        If BtnRepopulateBank IsNot Nothing Then
            BtnRepopulateBank.IsEnabled = _bankBalance <= 0D
        End If
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
        If _freeGamesQueued > 0 Then
            Await PlayQueuedFreeGamesAsync()
            Return
        End If

        If _selectedNumbers.Count = 0 Then Return
        _replayNumbers = _selectedNumbers.ToArray()

        Dim bet = GetSelectedBetAmount()
        If bet <= 0D Then Return

        Dim gamesToPlay = GetConsecutiveGamesCount()
        Dim useMultiplier = ChkMultiplierKeno.IsChecked = True
        Dim useFirstLast = ChkFirstLastPlay.IsChecked = True
        Dim useWayTicket = ChkWayTicket.IsChecked = True
        Dim useBullseye = _isBullseyeActive
        Dim usePowerball = ChkPowerball.IsChecked = True

        Dim totalWager = ComputeTotalWager(bet, gamesToPlay)
        Dim totalPayout = 0D
        Dim lastMatches = 0
        Dim gameResults As New List(Of (Index As Integer, Matched As Integer, Payout As Decimal, Multiplier As Integer, FirstLastBonus As Decimal))()
        Dim freeGameIndices As New HashSet(Of Integer)()

        Dim isSuperSonic = (GetDrawDelayMs() = 0)
        Dim halfType = GetHalfType()
        Dim gameMode = If(useBullseye, "Bullseye",
                          If(halfType = "TopBottom", "Top/Bottom Half",
                          If(halfType = "LeftRight", "Left/Right Half", "Regular")))
        BtnPlay.IsEnabled = False
        BtnCLEAR.IsEnabled = False
        ResetGamePlayDisplay()
        Try
            For i = 1 To gamesToPlay
                Dim result = Await DrawSingleGameAnimated()

                Dim gamePayout As Decimal
                If useWayTicket Then
                    gamePayout = GetWayTicketPayout(result.Draw, bet)
                ElseIf useBullseye Then
                    gamePayout = GetBullseyePayout(result.Matches) * bet
                ElseIf halfType IsNot Nothing Then
                    gamePayout = GetAreaPayout(halfType, result.Matches) * bet
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

                ' ── First/Last flat bonus — not scaled by bet, multiplier, or Powerball ──
                Dim firstLastBonus = 0D
                If useFirstLast Then
                    If _selectedNumbers.Contains(result.FirstDrawn) OrElse _selectedNumbers.Contains(result.LastDrawn) Then
                        firstLastBonus = GetFirstLastBallBonus(_selectedNumbers.Count)
                        gamePayout += firstLastBonus
                    End If
                End If

                totalPayout += gamePayout
                LblWinnings.Content = totalPayout.ToString("C2")
                lastMatches = result.Matches
                gameResults.Add((i, result.Matches, gamePayout, currentMultiplier, firstLastBonus))
                UpdateGamePlayCell(i, gamePayout > 0D,
                                   _selectedNumbers.Contains(result.FirstDrawn),
                                   _selectedNumbers.Contains(result.LastDrawn))
                UpdateDrawStats(result.Draw, gamePayout > 0D)

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

                ' Free game bonus — matching WinForms IsFreeGameBonus / AwardFreeGameBonus
                Dim isFreeGameResult = (gamePayout = 0D) AndAlso IsFreeGameBonus(result.Matches, bet)
                If isFreeGameResult Then
                    freeGameIndices.Add(i)
                    AwardFreeGameBonus(suppressDialog:=isSuperSonic AndAlso gamesToPlay > 1)
                ElseIf i < gamesToPlay AndAlso Not isSuperSonic Then
                    If gamePayout > 0D Then
                        WinPayoutSchedule.ShowForWin(Me, gameMode, _selectedNumbers.Count, result.Matches, gamePayout, bet)
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
        Dim totalFirstLastBonus = gameResults.Sum(Function(r) r.FirstLastBonus)
        Dim bonus = GetConsecutiveBonus()
        totalPayout = (subtotal - totalFirstLastBonus) * bonus + totalFirstLastBonus

        ' Record each game using bonus-adjusted payout so AllTimeSummary reflects actual earnings.
        Dim perGameBet = totalWager / gamesToPlay
        For Each r In gameResults
            Dim adjustedPayout = (r.Payout - r.FirstLastBonus) * bonus + r.FirstLastBonus
            AllTimeSummaryStore.RecordGame(perGameBet, adjustedPayout)
        Next

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
        UpdateFreeGamesButton()

        ' ── game log ──────────────────────────────────────────────────────────
        If gamesToPlay > 1 Then
            AppendBatch(gameMode, bet,
                        gameResults.Select(Function(r) (r.Matched, r.Payout, freeGameIndices.Contains(r.Index), r.Multiplier, r.FirstLastBonus)),
                        bonus)
        Else
            AppendGame(gameMode, bet, lastMatches, totalPayout,
                       If(gameResults.Count > 0, gameResults(0).Multiplier, 1),
                       freeGameIndices.Count > 0, 0D)
        End If

        ' ── Show result dialogs ────────────────────────────────────────────
        Try
            If gamesToPlay > 1 Then
                WinConsecutiveSummary.ShowSummary(Me, gameResults, bet, bonus, subtotal, totalPayout,
                                                  freeGamesEarned:=freeGameIndices.Count,
                                                  isSuperSonic:=isSuperSonic)
            ElseIf totalPayout > 0D Then
                WinPayoutSchedule.ShowForWin(Me, gameMode, _selectedNumbers.Count, lastMatches, totalPayout, bet)
            End If
        Catch ex As Exception
            MessageBox.Show($"Result dialog error: {ex.Message}{Environment.NewLine}{ex.StackTrace}",
                            "Result Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Async Function DrawSingleGameAnimated() As Task(Of (Draw As HashSet(Of Integer), Matches As Integer, FirstDrawn As Integer, LastDrawn As Integer))
        Dim pool As New List(Of Integer)(80)
        For i = 1 To 80 : pool.Add(i) : Next

        Dim drawOrder As New List(Of Integer)(20)
        Dim drawSet As New HashSet(Of Integer)()

        Dim sortedPicks = _selectedNumbers.ToArray()

        For Each kvp In _kenoButtons
            kvp.Value.Background = If(_selectedNumbers.Contains(kvp.Key), BrushUserPick, BrushDefault)
        Next

        For i = 0 To Math.Min(sortedPicks.Length, _playedLabels.Length) - 1
            _playedLabels(i).Background = BrushPlayedDefault
        Next

        Dim delay = GetDrawDelayMs()
        Do While drawOrder.Count < 20
            Dim idx = Random.Shared.Next(pool.Count)
            Dim num = pool(idx)
            pool.RemoveAt(idx)
            drawOrder.Add(num)
            drawSet.Add(num)
            _kenoButtons(num).Background = If(_selectedNumbers.Contains(num), BrushMatch, BrushDraw)

            Dim pickIdx = Array.IndexOf(sortedPicks, num)
            If pickIdx >= 0 AndAlso pickIdx < _playedLabels.Length Then
                _playedLabels(pickIdx).Background = BrushMatch
            End If

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

        Dim sorted = _selectedNumbers.ToArray()
        For i = 0 To Math.Min(sorted.Length, _playedLabels.Length) - 1
            _playedLabels(i).Background = If(draw.Contains(sorted(i)), BrushMatch, BrushPlayedDefault)
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

        If NudSpecialWager IsNot Nothing AndAlso NudSpecialWager.Value.HasValue AndAlso NudSpecialWager.Value.Value >= 0.05 Then
            Return CDec(NudSpecialWager.Value.Value)
        End If

        Return 0D
    End Function

    Private Sub RbWager_Checked(sender As Object, e As RoutedEventArgs)
        If NudSpecialWager IsNot Nothing Then NudSpecialWager.Value = Nothing
        UpdatePayoutScheduleDisplay()
        UpdateWayTicketSummary()
        UpdateWagerPreview()
    End Sub

    Private Sub NudSpecialWager_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double?))
        If NudSpecialWager IsNot Nothing AndAlso NudSpecialWager.Value.GetValueOrDefault() >= 0.05 Then
            For Each rb In GetWagerRadioButtons()
                rb.IsChecked = False
            Next
        End If

        UpdatePayoutScheduleDisplay()
        UpdateWagerPreview()
    End Sub

    ' ── draw speed
    Private Sub RbDrawSpeed_Checked(sender As Object, e As RoutedEventArgs)
        ShowDrawSpeedFlyout()
        If Not IsLoaded Then Return
        Dim speedRadios = {RbDrawSpeedSlow, RbDrawSpeedMedium, RbDrawSpeedFast, RbDrawSpeedSS}
        Dim idx = Array.IndexOf(speedRadios, TryCast(sender, RadioButton))
        If idx < 0 Then Return
        Dim appSettings = LoadAppSettings()
        appSettings.DrawSpeedIndex = idx
        SaveAppSettings(appSettings)
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
        Dim n As Integer = CInt(NudQuickPickCount.Value.GetValueOrDefault(1))

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

    Private Sub ChkMultiplierKeno_Changed()
        If ChkMultiplierKeno.IsChecked <> True Then
            ChkMultiplierKeno.Content = "Multiplier: 1×"
            HideFlyout()
        Else
            ShowMultiplierFlyout()
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
        If ChkFirstLastPlay.IsChecked = True Then
            ShowFirstLastFlyout()
        Else
            HideFlyout()
        End If
        UpdateWagerPreview()
    End Sub

    Private Sub ShowMultiplierFlyout()
        If ChkFlyoutsEnabled.IsChecked <> True Then Return
        FlyoutCheckboxHelp.Header = "Multiplier Keno"
        TbkFlyoutDesc.Text = "Adds $1 to your wager per game. Before each draw a random multiplier is applied to your base payout:"
        TbkFlyoutData.Text = "  ×1  — 45%   (most common)" & vbLf &
                             "  ×2  — 30%" & vbLf &
                             "  ×3  — 13%" & vbLf &
                             "  ×5  —  9%" & vbLf &
                             "  ×10 —  3%   (jackpot tier)"
        FlyoutCheckboxHelp.IsOpen = True
    End Sub

    Private Sub ShowFirstLastFlyout()
        If ChkFlyoutsEnabled.IsChecked <> True Then Return
        FlyoutCheckboxHelp.Header = "First / Last Ball"
        TbkFlyoutDesc.Text = "Adds $1 to your wager per game. If the first or last ball drawn matches any of your picks, you win a flat cash bonus:"
        TbkFlyoutData.Text = " Pick  Bonus" & vbLf &
                             "    1   $75" & vbLf &
                             "    2   $71" & vbLf &
                             "    3   $67" & vbLf &
                             "    4   $63" & vbLf &
                             "    5   $59" & vbLf &
                             "    6   $55" & vbLf &
                             "    7   $51" & vbLf &
                             "    8   $47" & vbLf &
                             "    9   $43" & vbLf &
                             "   10   $39" & vbLf &
                             "   11   $35" & vbLf &
                             "   12   $31" & vbLf &
                             "   13   $27" & vbLf &
                             "   14   $23" & vbLf &
                             "   15   $20" & vbLf &
                             "   16   $17" & vbLf &
                             "   17   $14" & vbLf &
                             "   18   $11" & vbLf &
                             "   19    $8" & vbLf &
                             "   20    $5"
        FlyoutCheckboxHelp.IsOpen = True
    End Sub

    Private Sub HideFlyout()
        FlyoutCheckboxHelp.IsOpen = False
    End Sub

    Private Sub ShowDrawSpeedFlyout()
        If Not IsLoaded Then Return
        If ChkFlyoutsEnabled.IsChecked <> True Then Return
        Dim delay = GetDrawDelayMs()
        Dim selected As String
        Select Case delay
            Case 1000 : selected = "Slow"
            Case 500 : selected = "Medium"
            Case 200 : selected = "Fast"
            Case Else : selected = "SS (Super Sonic)"
        End Select
        FlyoutCheckboxHelp.Header = "Draw Speed"
        TbkFlyoutDesc.Text = If(delay = 0,
            "Controls the pause between each of the 20 balls drawn per game. " &
            "In SS mode all 20 balls draw instantly, and consecutive games run " &
            "back-to-back with no pause between them.",
            $"Controls the pause between each of the 20 balls drawn per game. Currently: {selected}.")
        TbkFlyoutData.Text = " Mode   Delay" & vbLf &
                             " Slow   1.0 s / ball" & vbLf &
                             " Med    0.5 s / ball" & vbLf &
                             " Fast   0.2 s / ball" & vbLf &
                             " SS     instant"
        FlyoutCheckboxHelp.IsOpen = True
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
        If Not BtnPlay.IsEnabled Then Return
        If ChkWayTicket.IsChecked = True Then ChkWayTicket.IsChecked = False

        For Each kvp In _kenoButtons
            kvp.Value.Background = BrushDefault
        Next
        _selectedNumbers.Clear()
        _wayTicketGroups = Nothing
        _wayTicketKingNumber = 0

        For Each n In BullseyeNumbers
            _selectedNumbers.Add(n)
            _kenoButtons(n).Background = BrushUserPick
        Next

        _isBullseyeActive = True
        BtnBullseye.Background = Brushes.Gold
        UpdatePayoutScheduleDisplay()
        UpdatePlayedGrid()
        UpdateStatusBar()
        UpdateWagerPreview()
    End Sub

    Private Sub BtnFreeGames_Click(sender As Object, e As RoutedEventArgs)
        Dim available = GetFreeGames()
        If available <= 0 Then Return

        _freeGamesQueued = Math.Min(available, 10)
        UpdateFreeGamesGrid()
        UpdateFreeGamesButton()
    End Sub

    Private Async Function PlayQueuedFreeGamesAsync() As Task
        If _selectedNumbers.Count = 0 Then Return

        Dim gamesToPlay = Math.Min(_freeGamesQueued, GetFreeGames())
        If gamesToPlay <= 0 Then
            _freeGamesQueued = 0
            UpdateFreeGamesGrid()
            UpdateFreeGamesButton()
            Return
        End If

        Const FreeGameBet As Decimal = 2D
        Dim gameMode = If(_isBullseyeActive, "Bullseye", "Regular")
        Dim totalPayout = 0D

        UseFreeGames(gamesToPlay)

        BtnPlay.IsEnabled = False
        BtnCLEAR.IsEnabled = False
        BtnFreeGames.IsEnabled = False
        LblWinnings.Content = "$0"

        Try
            For i = 0 To gamesToPlay - 1
                Dim result = Await DrawSingleGameAnimated()

                Dim gamePayout As Decimal
                If _isBullseyeActive Then
                    gamePayout = GetBullseyePayout(result.Matches) * FreeGameBet
                Else
                    gamePayout = GetPayout(_selectedNumbers.Count, result.Matches) * FreeGameBet
                End If

                totalPayout += gamePayout
                LblWinnings.Content = totalPayout.ToString("C2")

                UpdateFreeGameCellResult(i, gamePayout > 0D)
                UpdateDrawStats(result.Draw, gamePayout > 0D)

                _bankBalance += gamePayout
                SaveBankBalance(_bankBalance)
                UpdateBankDisplay()

                AllTimeSummaryStore.RecordGame(FreeGameBet, gamePayout)
                _sessionGamesPlayed += 1
                If gamePayout > 0D Then _sessionWins += 1
                If gamePayout > _sessionBestPayout Then _sessionBestPayout = gamePayout
                _sessionTotalWagered += FreeGameBet

                AppendGame($"Free Game ({gameMode})", FreeGameBet, result.Matches, gamePayout, 1, False, 0D)
                UpdateStatusBar()
                UpdatePayoutScheduleDisplay()
            Next
        Catch ex As Exception
            MessageBox.Show($"Free game error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            _freeGamesQueued = 0
            BtnPlay.IsEnabled = True
            BtnCLEAR.IsEnabled = True
            UpdateFreeGamesButton()
            UpdateFreeGamesGrid()
        End Try
    End Function

    Private Function IsFreeGameBonus(matchedCount As Integer, betAmount As Decimal) As Boolean
        If betAmount < 2D Then Return False
        If matchedCount <> 0 Then Return False
        Dim picks = _selectedNumbers.Count
        Return picks >= 5 AndAlso picks <= 9
    End Function

    Private Sub AwardFreeGameBonus(Optional suppressDialog As Boolean = False)
        AddFreeGame()
        _sessionFreeGamesEarned += 1
        AllTimeSummaryStore.RecordFreeGameEarned()
        UpdateFreeGamesButton()
        If Not suppressDialog Then
            WinBonusGame.ShowBonus(Me, _selectedNumbers.Count)
        End If
    End Sub

    Private Sub UpdateFreeGamesButton()
        Dim count = GetFreeGames()
        Dim remaining = count - _freeGamesQueued
        Dim template = TryCast(BtnFreeGames.Tag, String)
        BtnFreeGames.Content = If(template IsNot Nothing, String.Format(template, remaining), $"Free Games Won ({remaining})")
        BtnFreeGames.IsEnabled = remaining > 0 AndAlso _freeGamesQueued < 10 AndAlso BtnPlay.IsEnabled
    End Sub

    Private Sub BtnPlayFavorites_Click()
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
        Dim nums = GetQuadrantNumbers(quadrant)
        Dim isActive = nums.All(Function(n) _selectedNumbers.Contains(n))

        ' Cap at 2 active quadrants — must deselect one before selecting another.
        If Not isActive AndAlso CountActiveQuadrants() >= 2 Then Return

        If ChkWayTicket.IsChecked = True Then ChkWayTicket.IsChecked = False
        If _isBullseyeActive Then
            _isBullseyeActive = False
            BtnBullseye.Background = Brushes.MistyRose
        End If

        If isActive Then
            For Each n In nums
                _selectedNumbers.Remove(n)
                _kenoButtons(n).Background = BrushDefault
            Next
        Else
            For Each n In nums
                _selectedNumbers.Add(n)
                _kenoButtons(n).Background = BrushUserPick
            Next
        End If

        UpdateQuadrantButtonStates()
        UpdatePayoutScheduleDisplay()
        UpdatePlayedGrid()
        UpdateStatusBar()
        UpdateWayTicketSummary()
        UpdateWagerPreview()
    End Sub

    Private Function CountActiveQuadrants() As Integer
        Return Enumerable.Range(1, 4).Count(Function(q) GetQuadrantNumbers(q).All(Function(n) _selectedNumbers.Contains(n)))
    End Function

    ' Returns the area-payout key when exactly 2 quadrants are active, Nothing otherwise.
    ' Q1=top-left  Q2=top-right  Q3=bottom-left  Q4=bottom-right
    ' Same row  → "TopBottom";  Same column or diagonal → "LeftRight".
    Private Function GetHalfType() As String
        If CountActiveQuadrants() <> 2 Then Return Nothing
        Dim active = Enumerable.Range(1, 4).Where(Function(q) GetQuadrantNumbers(q).All(Function(n) _selectedNumbers.Contains(n))).ToList()
        If active.Count <> 2 Then Return Nothing
        Dim a = active(0), b = active(1)   ' always a < b (ascending Range)
        If (a = 1 AndAlso b = 2) OrElse (a = 3 AndAlso b = 4) Then Return "TopBottom"
        Return "LeftRight"   ' same column (1+3, 2+4) or diagonal (1+4, 2+3)
    End Function

    Private Sub UpdateQuadrantButtonStates()
        Dim qBtns = {BtnQuadrant1, BtnQuadrant2, BtnQuadrant3, BtnQuadrant4}
        For q = 1 To 4
            Dim nums = GetQuadrantNumbers(q)
            qBtns(q - 1).Background = If(nums.All(Function(n) _selectedNumbers.Contains(n)),
                                          Brushes.Gold, Brushes.Orange)
        Next

        ' First/Last is incompatible with half-board (2 active quadrants).
        Dim isHalf = CountActiveQuadrants() >= 2
        If isHalf AndAlso ChkFirstLastPlay.IsChecked = True Then
            ChkFirstLastPlay.IsChecked = False
        End If
        ChkFirstLastPlay.IsEnabled = Not isHalf
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

    Private Sub SbiSettings_MouseDown(sender As Object, e As MouseButtonEventArgs)
        FlyoutSettings.IsOpen = Not FlyoutSettings.IsOpen
    End Sub

    Private Sub ChkFlyoutsEnabled_Changed(sender As Object, e As RoutedEventArgs)
        If Not IsLoaded Then Return
        Dim appSettings = LoadAppSettings()
        appSettings.FlyoutsEnabled = ChkFlyoutsEnabled.IsChecked.GetValueOrDefault(False)
        SaveAppSettings(appSettings)
    End Sub

    Private Sub BtnRepopulateBank_Click(sender As Object, e As RoutedEventArgs)
        Dim amount = CDec(NudRepopulateAmount.Value.GetValueOrDefault(10000D))
        _bankBalance = amount
        SaveBankBalance(_bankBalance)
        UpdateBankDisplay()
        Dim appSettings = LoadAppSettings()
        appSettings.RepopulateAmount = amount
        SaveAppSettings(appSettings)
        FlyoutSettings.IsOpen = False
    End Sub

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