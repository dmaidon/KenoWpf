' Last Edit: 2026-03-24 05:29 AM - RtbPayoutSchedule now shows 1ST/LST bonus row when First/Last play is enabled.
Imports System.Threading

Class MainWindow

    Private ReadOnly Vm As New MainViewModel()
    Private Shared ReadOnly DrawDelays As Integer() = {1000, 500, 200, 0}

    ' ── colour constants matching WinForms FrmMain.KenoSelection ──────────────
    Private Shared ReadOnly BrushDefault As SolidColorBrush = Freeze(Color.FromRgb(192, 255, 255))  ' #C0FFFF

    Private Shared ReadOnly BrushPlayedDefault As SolidColorBrush = Freeze(Color.FromRgb(245, 245, 245))  ' #F5F5F5

    Private Shared ReadOnly BrushUserPick As SolidColorBrush = Freeze(Colors.LightGreen)
    Private Shared ReadOnly BrushDraw As SolidColorBrush = Freeze(Colors.LightSkyBlue)
    Private Shared ReadOnly BrushMatch As SolidColorBrush = Freeze(Colors.Gold)
    Private Shared ReadOnly BrushMatchDraw As SolidColorBrush = Freeze(Colors.RoyalBlue)
    Private Shared ReadOnly BrushPowerball As SolidColorBrush = Freeze(Colors.OrangeRed)

    Private ReadOnly _kenoButtons As New Dictionary(Of Integer, Button)(80)
    Private ReadOnly Sel As New PickSelection()
    Private ReadOnly _playedLabels(19) As Label   ' 20 display slots (0-based)
    Private ReadOnly _gamePlayLabels(19) As Label  ' consecutive-game progress cells (0-based)
    Private ReadOnly _freeGameCells(9) As Label    ' free-game queue slots (0-based, 10 cells)
    Private _freeGamesQueued As Integer = 0        ' number of free games the user has staged to play

    Private _bankBalance As Decimal
    Private _jackpotBalance As Decimal
    Private _lastMatches As Integer = -1
    Private _lastPayout As Decimal
    Private _replayNumbers As Integer() = Array.Empty(Of Integer)()
    Private _lastFirstLastBonus As Decimal = 0D

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
        DataContext = Vm
        AddHandler Vm.PropertyChanged, AddressOf Vm_PropertyChanged
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
        Vm.FlyoutsEnabled = appSettings.FlyoutsEnabled
        Vm.DrawDelayMs = DrawDelays(Math.Max(0, Math.Min(appSettings.DrawSpeedIndex, DrawDelays.Length - 1)))
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
        Dim sorted = Sel.Numbers.ToArray()
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

        Sel.Toggle(num)
        If Vm.UseWayTicket Then Vm.UseWayTicket = False
        OnSelectionChanged()
    End Sub

    ' Repaints the grid and refreshes every display that depends on the current picks.
    Private Sub OnSelectionChanged()
        For Each kvp In _kenoButtons
            kvp.Value.Background = If(Sel.Contains(kvp.Key), BrushUserPick, BrushDefault)
        Next
        BtnBullseye.Background = If(Sel.IsBullseye, Brushes.Gold, Brushes.Coral)
        UpdateQuadrantButtonStates()
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

    Private Sub ResetGrid(Optional preserveFreeGames As Boolean = False)
        If Vm.UseWayTicket Then Vm.UseWayTicket = False
        Sel.Clear()
        _lastMatches = -1
        _lastPayout = 0D
        _lastFirstLastBonus = 0D
        Vm.WinningsText = "$0"
        Vm.WagerTotalText = "$0"
        ResetGamePlayDisplay()
        If Not preserveFreeGames Then
            _freeGamesQueued = 0
            UpdateFreeGamesGrid()
            UpdateFreeGamesButton()
        End If
        OnSelectionChanged()
    End Sub

    ' ── status bar ───────────────────────────────────────────────────────────
    Private Sub UpdateStatusBar()
        Vm.PicksText = $"Picks: {Sel.Numbers.Count}"
        Vm.MatchesText = $"Matches: {If(_lastMatches < 0, "—", _lastMatches.ToString())}"
        Vm.PayoutText = $"Payout: {_lastPayout:C2}"
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
            Vm.StreakText = $"Win Streak: {stats.CurrentWinStreak}"
            Vm.StreakBrush = Brushes.ForestGreen
        ElseIf stats.CurrentLossStreak > 0 Then
            Vm.StreakText = $"Loss Streak: {stats.CurrentLossStreak}"
            Vm.StreakBrush = Brushes.Firebrick
        Else
            Vm.StreakText = "Streak: —"
            Vm.StreakBrush = SystemColors.ControlTextBrush
        End If

        Vm.BestStreakText = $"Best: {stats.BestWinStreak}W"

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
        If Vm.IsPlaying Then Return
        Dim brd = TryCast(sender, Border)
        If brd Is Nothing Then Return
        Dim tbk = TryCast(brd.Child, TextBlock)
        If tbk Is Nothing Then Return

        Dim number As Integer
        If Not Integer.TryParse(tbk.Text, number) Then Return

        Dim value As Button = Nothing
        If Not _kenoButtons.TryGetValue(number, value) Then Return

        If Vm.UseWayTicket Then Vm.UseWayTicket = False
        If Not Sel.Toggle(number) Then Return
        OnSelectionChanged()
    End Sub

    Private Const HitsColWidth As Integer = 8

    Private Const WinColWidth As Integer = 19

    Private Sub UpdatePayoutScheduleDisplay()
        Dim pickCount = Sel.Numbers.Count

        If pickCount = 0 Then
            RtbPayoutSchedule.Document.Blocks.Clear()
            TbkPayoutHeader.Text = "Payout Schedule"
            Return
        End If

        Dim betAmount = GetSelectedBetAmount()
        If betAmount <= 0D Then betAmount = 1D
        Dim halfType = GetHalfType()
        Dim entries = If(Sel.IsBullseye, GetBullseyePayoutScheduleEntries(),
                         If(halfType IsNot Nothing, GetAreaPayoutScheduleEntries(halfType),
                            GetPayoutScheduleEntries(pickCount)))
        TbkPayoutHeader.Text = If(Sel.IsBullseye, "Payout — Bullseye",
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

        ' First/Last Ball bonus row
        If Vm.UseFirstLast AndAlso Not Sel.IsBullseye AndAlso halfType Is Nothing AndAlso pickCount > 0 Then
            Dim flBonus = GetFirstLastBallBonus(pickCount)
            If flBonus > 0D Then
                Dim isFlWin = _lastMatches >= 0 AndAlso _lastFirstLastBonus > 0D
                doc.Blocks.Add(MakeScheduleLine(
                    New String("-"c, HitsColWidth + WinColWidth),
                    Brushes.Gray, bold:=False))
                doc.Blocks.Add(MakeScheduleLine(
                    "1ST/LST".PadRight(HitsColWidth) & ("+" & flBonus.ToString("C2")).PadLeft(WinColWidth),
                    If(isFlWin, Brushes.Black, Brushes.Teal), bold:=isFlWin,
                    If(isFlWin, Brushes.Gold, Nothing)))
            End If
        End If

        doc.Blocks.Add(MakeScheduleLine(
            New String("-"c, HitsColWidth + WinColWidth),
            Brushes.Gray, bold:=False))

        Dim hitText = If(_lastMatches < 0, "-", _lastMatches.ToString())
        doc.Blocks.Add(MakeScheduleLine(
            $"MARKED:{pickCount}  HIT:{hitText}",
            Brushes.Black, bold:=True))

        RtbPayoutSchedule.Document = doc
    End Sub

    Private Shared Function MakeScheduleLine(text As String, fg As Brush, bold As Boolean, Optional bg As Brush = Nothing) As Paragraph
        Dim run As New Run(text) With {
            .Foreground = fg,
            .FontWeight = If(bold, FontWeights.Bold, FontWeights.Normal)
        }
        Dim para As New Paragraph(run) With {
            .Margin = New Thickness(0),
            .Padding = New Thickness(0),
            .LineHeight = 14
        }
        If bg IsNot Nothing Then para.Background = bg
        Return para
    End Function

    ' ── bank display ─────────────────────────────────────────────────────────
    Private Sub UpdateBankDisplay()
        Vm.BankText = _bankBalance.ToString("C2")
        If NudSpecialWager IsNot Nothing Then
            NudSpecialWager.Maximum = Math.Min(5000D, CDbl(_bankBalance))
        End If
        Vm.CanRepopulate = _bankBalance <= 0D
    End Sub

    Private Sub UpdateJackpotDisplay()
        Vm.JackpotText = $"Progressive Jackpot: {_jackpotBalance:C0}"
    End Sub

    Private Sub UpdateWagerPreview()
        Dim bet = GetSelectedBetAmount()
        Dim games = GetConsecutiveGamesCount()

        If bet <= 0D OrElse Sel.Numbers.Count = 0 Then
            Vm.WagerTotalText = "$0"
            Return
        End If

        Vm.WagerTotalText = ComputeTotalWager(bet, games).ToString("C2")
    End Sub

    ' ── play / clear ─────────────────────────────────────────────────────────
    Private _playCts As CancellationTokenSource
    Private _closeRequested As Boolean

    Private Async Sub BtnPlay_Click(sender As Object, e As RoutedEventArgs)
        Try
            Await PlayRoundAsync()
        Catch ex As Exception
            LogError(ex, NameOf(BtnPlay_Click))
            MessageBox.Show($"Play error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            Vm.IsPlaying = False
        Finally
            If _closeRequested Then Close()
        End Try
    End Sub

    Private Async Function PlayRoundAsync() As Task
        If _freeGamesQueued > 0 Then
            Await PlayQueuedFreeGamesAsync()
            Return
        End If

        If Sel.Numbers.Count = 0 Then Return
        _replayNumbers = Sel.Numbers.ToArray()

        Dim bet = GetSelectedBetAmount()
        If bet <= 0D Then Return

        Dim gamesToPlay = GetConsecutiveGamesCount()
        Dim useMultiplier = Vm.UseMultiplier
        Dim useFirstLast = Vm.UseFirstLast
        Dim useWayTicket = Vm.UseWayTicket
        Dim useBullseye = Sel.IsBullseye
        Dim usePowerball = Vm.UsePowerball

        Dim totalWager = ComputeTotalWager(bet, gamesToPlay)
        Dim totalPayout = 0D
        Dim lastMatches = 0
        Dim options = BuildGameOptions(bet)
        Dim gameResults As New List(Of (Index As Integer, Matched As Integer, Payout As Decimal, Multiplier As Integer, FirstLastBonus As Decimal))()
        Dim freeGameIndices As New HashSet(Of Integer)()

        Dim isSuperSonic = (GetDrawDelayMs() = 0)
        Dim halfType = GetHalfType()
        Dim gameMode = If(useBullseye, "Bullseye",
                          If(halfType = "TopBottom", "Top/Bottom Half",
                          If(halfType = "LeftRight", "Left/Right Half", "Regular")))
        ' Debit the full wager up front so a crash or close mid-play cannot leave the bet uncharged.
        _bankBalance -= totalWager
        SaveBankBalance(_bankBalance)
        UpdateBankDisplay()

        Dim perGameWager = totalWager / gamesToPlay
        Dim cancelled = False
        _playCts = New CancellationTokenSource()
        Dim token = _playCts.Token
        Vm.IsPlaying = True
        ResetGamePlayDisplay()
        Try
            For i = 1 To gamesToPlay
                Dim result = Await DrawSingleGameAnimated(token)

                Dim score = GameEngine.ScoreGame(options, result.DrawOrder, Random.Shared)
                Dim gamePayout = score.Payout
                Dim currentMultiplier = score.Multiplier
                Dim firstLastBonus = score.FirstLastBonus
                If useMultiplier Then ChkMultiplierKeno.Content = $"Multiplier: {currentMultiplier}×"
                If usePowerball Then LblPowerballValue.Content = score.PowerballNumber.ToString()

                totalPayout += gamePayout
                Vm.WinningsText = totalPayout.ToString("C2")
                lastMatches = result.Matches
                gameResults.Add((i, result.Matches, gamePayout, currentMultiplier, firstLastBonus))
                UpdateGamePlayCell(i, gamePayout > 0D,
                                   Sel.Numbers.Contains(result.DrawOrder(0)),
                                   Sel.Numbers.Contains(result.DrawOrder(19)))
                UpdateDrawStats(result.Draw, gamePayout > 0D)

                ' ── progressive jackpot contribution ───────────────────────────
                If bet >= 5D Then
                    AddToJackpot(bet * 0.05D)
                    _jackpotBalance = GetJackpotBalance()
                    UpdateJackpotDisplay()
                End If

                ' ── progressive jackpot win check ──────────────────────────────
                If Sel.Numbers.Count >= 8 AndAlso result.Matches = Sel.Numbers.Count Then
                    _bankBalance += _jackpotBalance
                    SaveBankBalance(_bankBalance)
                    UpdateBankDisplay()
                    MessageBox.Show($"JACKPOT! You matched all {Sel.Numbers.Count} numbers and won {_jackpotBalance:C0}!",
                                    "Progressive Jackpot Winner!", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                    ResetJackpot()
                    AllTimeSummaryStore.RecordJackpotWon()
                    _jackpotBalance = GetJackpotBalance()
                    UpdateJackpotDisplay()
                End If

                ' Free game bonus — matching WinForms IsFreeGameBonus / AwardFreeGameBonus
                Dim isFreeGameResult = (gamePayout = 0D) AndAlso GameEngine.IsFreeGameBonus(result.Matches, bet, Sel.Numbers.Count)
                If isFreeGameResult Then
                    freeGameIndices.Add(i)
                    AwardFreeGameBonus(suppressDialog:=isSuperSonic AndAlso gamesToPlay > 1)
                ElseIf i < gamesToPlay AndAlso Not isSuperSonic Then
                    If gamePayout > 0D Then
                        WinPayoutSchedule.ShowForWin(Me, gameMode, Sel.Numbers.Count, result.Matches, gamePayout, bet)
                    Else
                        Await Task.Delay(3000, token)
                    End If
                End If
            Next
        Catch ex As OperationCanceledException
            cancelled = True
        Finally
            Vm.IsPlaying = False
            _playCts.Dispose()
            _playCts = Nothing
        End Try

        ' A cancelled run settles only the completed games and refunds the rest of the wager.
        Dim gamesPlayed = gameResults.Count
        If gamesPlayed < gamesToPlay Then
            Dim refund = totalWager - perGameWager * gamesPlayed
            _bankBalance += refund
            totalWager -= refund
            SaveBankBalance(_bankBalance)
            UpdateBankDisplay()
            If gamesPlayed = 0 Then
                ResetGamePlayDisplay()
                Return
            End If
        End If

        Dim subtotal = totalPayout
        Dim totalFirstLastBonus = gameResults.Sum(Function(r) r.FirstLastBonus)
        _lastFirstLastBonus = totalFirstLastBonus
        Dim bonus = If(cancelled, 1D, GetConsecutiveBonus())
        totalPayout = GameEngine.ApplyConsecutiveBonus(subtotal, totalFirstLastBonus, bonus)

        ' Record each game using bonus-adjusted payout so AllTimeSummary reflects actual earnings.
        Dim perGameBet = perGameWager
        AllTimeSummaryStore.RecordGames(gameResults.Select(Function(r) (perGameBet, GameEngine.ApplyConsecutiveBonus(r.Payout, r.FirstLastBonus, bonus))))

        _bankBalance += totalPayout   ' wager was already debited before the first draw
        SaveBankBalance(_bankBalance)
        UpdateBankDisplay()

        ' Update session stats
        _sessionGamesPlayed += gamesPlayed
        If totalPayout > 0D Then _sessionWins += 1
        If totalPayout > _sessionBestPayout Then _sessionBestPayout = totalPayout
        _sessionTotalWagered += totalWager

        _lastMatches = lastMatches
        _lastPayout = totalPayout
        Vm.WinningsText = totalPayout.ToString("C2")
        Vm.WagerTotalText = totalWager.ToString("C2")
        UpdatePayoutScheduleDisplay()
        UpdateStatusBar()
        UpdateFreeGamesButton()

        ' ── game log ──────────────────────────────────────────────────────────
        If gamesPlayed > 1 Then
            AppendBatch(gameMode, bet,
                        gameResults.Select(Function(r) (r.Matched, r.Payout, freeGameIndices.Contains(r.Index), r.Multiplier, r.FirstLastBonus)),
                        bonus)
        Else
            AppendGame(gameMode, bet, lastMatches, totalPayout,
                       If(gameResults.Count > 0, gameResults(0).Multiplier, 1),
                       freeGameIndices.Count > 0, 0D)
        End If

        ' ── Show result dialogs ────────────────────────────────────────────
        If cancelled Then Return
        Try
            If gamesToPlay > 1 Then
                WinConsecutiveSummary.ShowSummary(Me, gameResults, bet, bonus, subtotal, totalPayout,
                                                  freeGamesEarned:=freeGameIndices.Count,
                                                  isSuperSonic:=isSuperSonic)
            ElseIf totalPayout > 0D Then
                WinPayoutSchedule.ShowForWin(Me, gameMode, Sel.Numbers.Count, lastMatches, totalPayout, bet)
            End If
        Catch ex As Exception
            LogError(ex, "Result dialog")
            MessageBox.Show($"Result dialog error: {ex.Message}",
                            "Result Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Function

    Private Async Function DrawSingleGameAnimated(token As CancellationToken) As Task(Of (Draw As HashSet(Of Integer), DrawOrder As List(Of Integer), Matches As Integer))
        Dim drawOrder = GameEngine.DrawNumbers(Random.Shared)
        Dim drawSet As New HashSet(Of Integer)()
        Dim sortedPicks = Sel.Numbers.ToArray()

        For Each kvp In _kenoButtons
            kvp.Value.Background = If(Sel.Numbers.Contains(kvp.Key), BrushUserPick, BrushDefault)
        Next

        For i = 0 To Math.Min(sortedPicks.Length, _playedLabels.Length) - 1
            _playedLabels(i).Background = BrushPlayedDefault
        Next

        Dim delay = GetDrawDelayMs()
        For Each num In drawOrder
            drawSet.Add(num)
            _kenoButtons(num).Background = If(Sel.Numbers.Contains(num), BrushMatch, BrushDraw)

            Dim pickIdx = Array.IndexOf(sortedPicks, num)
            If pickIdx >= 0 AndAlso pickIdx < _playedLabels.Length Then
                _playedLabels(pickIdx).Background = BrushMatch
            End If

            If delay > 0 Then Await Task.Delay(delay, token)
        Next

        Return (drawSet, drawOrder, GameEngine.CountMatches(sortedPicks, drawSet))
    End Function

    Private Function BuildGameOptions(bet As Decimal) As GameOptions
        Dim halfType = GetHalfType()
        Return New GameOptions With {
            .Mode = If(Sel.IsBullseye, TicketMode.Bullseye,
                    If(halfType = "TopBottom", TicketMode.TopBottomHalf,
                    If(halfType = "LeftRight", TicketMode.LeftRightHalf, TicketMode.Regular))),
            .Picks = Sel.Numbers.ToArray(),
            .Bet = bet,
            .UseMultiplier = Vm.UseMultiplier,
            .UseFirstLast = Vm.UseFirstLast,
            .UsePowerball = Vm.UsePowerball,
            .UseWayTicket = Vm.UseWayTicket,
            .WayGroups = Sel.WayGroups
        }
    End Function

    Private Function ComputeTotalWager(bet As Decimal, gamesToPlay As Integer) As Decimal
        Return GameEngine.ComputeTotalWager(bet, gamesToPlay, Vm.UseMultiplier,
                                            Vm.UseFirstLast, Vm.UseWayTicket,
                                            If(Sel.WayGroups Is Nothing, 0, Sel.WayGroups.Count), Sel.Numbers.Count)
    End Function

    Private Sub ApplyDrawToGrid(draw As HashSet(Of Integer))
        For Each kvp In _kenoButtons
            Dim num = kvp.Key
            Dim inPick = Sel.Numbers.Contains(num)
            Dim inDraw = draw.Contains(num)
            kvp.Value.Background = If(inPick AndAlso inDraw, BrushMatch,
                                    If(inPick, BrushUserPick,
                                    If(inDraw, BrushDraw,
                                       BrushDefault)))
        Next

        Dim sorted = Sel.Numbers.ToArray()
        For i = 0 To Math.Min(sorted.Length, _playedLabels.Length) - 1
            _playedLabels(i).Background = If(draw.Contains(sorted(i)), BrushMatch, BrushPlayedDefault)
        Next
    End Sub

    Private Sub BtnClear_Click(sender As Object, e As RoutedEventArgs)
        ResetGrid()
    End Sub

    ' ── consecutive games ──────────────────────────────────────────────────
    Private Function GetConsecutiveGamesCount() As Integer
        Return Vm.ConsecutiveGames
    End Function

    Private Function GetConsecutiveBonus() As Decimal
        Return GameEngine.GetConsecutiveBonus(GetConsecutiveGamesCount())
    End Function

    Private Sub UpdateBonusDisplay()
        Dim bonus = GetConsecutiveBonus()
        TbkBonusMultiplier.Text = $"Bonus: {bonus}×"
    End Sub

    ' ── wager ───────────────────────────────────────────────────────────────
    Private Function GetSelectedBetAmount() As Decimal
        If Vm.BetPreset > 0 Then Return Vm.BetPreset
        If Vm.SpecialWager.GetValueOrDefault() >= 0.05 Then Return Math.Round(CDec(Vm.SpecialWager.Value), 2)
        Return 0D
    End Function

    ' ── draw speed
    Private Function GetDrawDelayMs() As Integer
        Return Vm.DrawDelayMs
    End Function

    ' ── save favorites ───────────────────────────────────────────────────────────────────────────
    Private Sub BtnSaveFavorites_Click(sender As Object, e As RoutedEventArgs)
        If Sel.Numbers.Count = 0 Then
            MessageBox.Show("No numbers selected to save.", "Save Favorites",
                            MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If

        Dim slot = WinFavoritesSlot.ShowForSave(Me, Sel.Numbers.Count)
        If slot < 0 Then Return

        SaveFavorites(slot, Sel.Numbers)
        MessageBox.Show($"Saved {Sel.Numbers.Count} numbers to Slot {slot + 1}.", "Save Favorites",
                        MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub

    ' ── special play ─────────────────────────────────────────────────────────────────────────────
    Private Sub BtnQuickPick_Click(sender As Object, e As RoutedEventArgs)
        Dim n As Integer = CInt(NudQuickPickCount.Value.GetValueOrDefault(1))

        ResetGrid(preserveFreeGames:=True)
        Sel.QuickPick(n, Random.Shared)
        OnSelectionChanged()
    End Sub

    Private Sub OnMultiplierChanged()
        If Not Vm.UseMultiplier Then
            ChkMultiplierKeno.Content = "Multiplier: 1×"
            HideFlyout()
        Else
            ShowMultiplierFlyout()
        End If
        UpdateWagerPreview()
    End Sub

    ' ── option changes (raised by the view model) ──────────────────────────────
    Private Sub Vm_PropertyChanged(sender As Object, e As ComponentModel.PropertyChangedEventArgs)
        Select Case e.PropertyName
            Case NameOf(MainViewModel.BetPreset), NameOf(MainViewModel.SpecialWager)
                UpdatePayoutScheduleDisplay()
                UpdateWayTicketSummary()
                UpdateWagerPreview()
            Case NameOf(MainViewModel.ConsecutiveGames)
                UpdateBonusDisplay()
                UpdateWagerPreview()
            Case NameOf(MainViewModel.DrawDelayMs)
                ShowDrawSpeedFlyout()
                If IsLoaded Then
                    Dim appSettings = LoadAppSettings()
                    appSettings.DrawSpeedIndex = Math.Max(0, Array.IndexOf(DrawDelays, Vm.DrawDelayMs))
                    SaveAppSettings(appSettings)
                End If
            Case NameOf(MainViewModel.UseMultiplier)
                OnMultiplierChanged()
            Case NameOf(MainViewModel.UseFirstLast)
                If Vm.UseFirstLast Then ShowFirstLastFlyout() Else HideFlyout()
                UpdateWagerPreview()
            Case NameOf(MainViewModel.UseWayTicket)
                OnWayTicketChanged()
            Case NameOf(MainViewModel.IsPlaying)
                UpdateFreeGamesButton()
            Case NameOf(MainViewModel.UsePowerball)
                LblPowerballValue.Content = If(Vm.UsePowerball, "Active", "—")
            Case NameOf(MainViewModel.FlyoutsEnabled)
                If IsLoaded Then
                    Dim appSettings = LoadAppSettings()
                    appSettings.FlyoutsEnabled = Vm.FlyoutsEnabled
                    SaveAppSettings(appSettings)
                End If
        End Select
    End Sub

    Private Sub ShowMultiplierFlyout()
        If Not Vm.FlyoutsEnabled Then Return
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
        If Not Vm.FlyoutsEnabled Then Return
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
        If Not Vm.FlyoutsEnabled Then Return
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

    Private Sub OnWayTicketChanged()
        If Vm.UseWayTicket Then
            If Sel.Numbers.Count = 0 Then
                MessageBox.Show("Select your numbers first, then enable Way Ticket.",
                                "Way Ticket", MessageBoxButton.OK, MessageBoxImage.Information)
                Vm.UseWayTicket = False
                Return
            End If
            Dim result = WinWayTicket.ShowAssignment(Me, Sel.Numbers.ToList(), GetSelectedBetAmount())
            If result IsNot Nothing Then
                Sel.WayGroups = result.Groups
                Sel.WayKingNumber = result.KingNumber
            Else
                Vm.UseWayTicket = False
                Sel.ClearWayTicket()
            End If
        Else
            Sel.ClearWayTicket()
        End If
        UpdateWayTicketSummary()
        UpdateWagerPreview()
    End Sub

    Private Sub UpdateWayTicketSummary()
        If Not Vm.UseWayTicket OrElse Sel.WayGroups Is Nothing OrElse Sel.WayGroups.Count = 0 Then
            LblWayTicketSummary.Content = "—"
            Return
        End If
        Dim subTickets = Sel.WayGroups.Count
        Dim total = subTickets * GetSelectedBetAmount()
        LblWayTicketSummary.Content = $"{subTickets} sub-tickets @ {total:C2}"
    End Sub

    Private Sub BtnBullseye_Click(sender As Object, e As RoutedEventArgs)
        If Vm.IsPlaying Then Return
        If Vm.UseWayTicket Then Vm.UseWayTicket = False
        Sel.ActivateBullseye()
        OnSelectionChanged()
    End Sub

    Private Sub BtnFreeGames_Click(sender As Object, e As RoutedEventArgs)
        Dim available = GetFreeGames()
        If available <= 0 OrElse _freeGamesQueued >= available OrElse _freeGamesQueued >= 10 Then Return

        _freeGamesQueued += 1
        UpdateFreeGamesGrid()
        UpdateFreeGamesButton()
    End Sub

    Private Async Function PlayQueuedFreeGamesAsync() As Task
        If Sel.Numbers.Count = 0 Then Return

        ' Trust the staged count — BtnFreeGames_Click already validated GetFreeGames() >= _freeGamesQueued
        ' at each click, so re-reading the disk here can only return a stale/lower value and play
        ' fewer games than the user staged.
        Dim gamesToPlay = _freeGamesQueued
        If gamesToPlay <= 0 Then
            _freeGamesQueued = 0
            UpdateFreeGamesGrid()
            UpdateFreeGamesButton()
            Return
        End If

        Const FreeGameBet As Decimal = 2D
        Dim gameMode = If(Sel.IsBullseye, "Bullseye", "Regular")
        Dim totalPayout = 0D

        UseFreeGames(gamesToPlay)

        _playCts = New CancellationTokenSource()
        Dim token = _playCts.Token
        Vm.IsPlaying = True
        Vm.WinningsText = "$0"
        Vm.WagerTotalText = "$0"

        Dim freeGameResults As New List(Of (Matched As Integer, Payout As Decimal))()

        Try
            For i = 0 To gamesToPlay - 1
                Dim result = Await DrawSingleGameAnimated(token)

                Dim gamePayout = GameEngine.ScoreGame(New GameOptions With {
                    .Mode = If(Sel.IsBullseye, TicketMode.Bullseye, TicketMode.Regular),
                    .Picks = Sel.Numbers.ToArray(),
                    .Bet = FreeGameBet}, result.DrawOrder, Random.Shared).Payout

                totalPayout += gamePayout
                Vm.WinningsText = totalPayout.ToString("C2")

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

                freeGameResults.Add((result.Matches, gamePayout))
                UpdateStatusBar()
                UpdatePayoutScheduleDisplay()
            Next
        Catch ex As OperationCanceledException
            ' unplayed free games are returned below
        Catch ex As Exception
            LogError(ex, NameOf(PlayQueuedFreeGamesAsync))
            MessageBox.Show($"Free game error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        Finally
            For k = freeGameResults.Count + 1 To gamesToPlay
                AddFreeGame()   ' give back free games that were not played
            Next
            _playCts.Dispose()
            _playCts = Nothing
            If freeGameResults.Count > 0 Then
                AppendFreeGameBatch(gameMode, FreeGameBet, freeGameResults)
            End If
            _freeGamesQueued = 0
            _lastFirstLastBonus = 0D
            Vm.IsPlaying = False
            UpdateFreeGamesButton()
            UpdateFreeGamesGrid()
            Vm.WagerTotalText = (freeGameResults.Count * FreeGameBet).ToString("C2")
        End Try
    End Function

    Private Sub AwardFreeGameBonus(Optional suppressDialog As Boolean = False)
        AddFreeGame()
        _sessionFreeGamesEarned += 1
        AllTimeSummaryStore.RecordFreeGameEarned()
        UpdateFreeGamesButton()
        If Not suppressDialog Then
            WinBonusGame.ShowBonus(Me, Sel.Numbers.Count)
        End If
    End Sub

    Private Sub UpdateFreeGamesButton()
        Dim count = GetFreeGames()
        Dim remaining = count - _freeGamesQueued
        Vm.FreeGamesText = $"Free Games Won ({remaining})"
        Vm.CanStageFreeGame = remaining > 0 AndAlso _freeGamesQueued < 10 AndAlso Not Vm.IsPlaying
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
        ResetGrid(preserveFreeGames:=True)
        Sel.Replace(favs)
        OnSelectionChanged()
    End Sub

    Private Sub BtnReplayLastGame_Click(sender As Object, e As RoutedEventArgs)
        If _replayNumbers.Length = 0 Then
            MessageBox.Show("No previous game to replay.", "Replay Last Game",
                            MessageBoxButton.OK, MessageBoxImage.Information)
            Return
        End If
        ResetGrid(preserveFreeGames:=True)
        Sel.Replace(_replayNumbers)
        OnSelectionChanged()
    End Sub

    Private Sub BtnQuadrant_Click(sender As Object, e As RoutedEventArgs)
        If TypeOf sender IsNot Button Then Return
        Dim btn = CType(sender, Button)
        Dim quadrant = CInt(btn.Tag)
        If Not Sel.ToggleQuadrant(quadrant) Then Return   ' capped at 2 active quadrants
        If Vm.UseWayTicket Then Vm.UseWayTicket = False
        OnSelectionChanged()
    End Sub

    Private Function GetHalfType() As String
        Return Sel.GetHalfType()
    End Function

    Private Sub UpdateQuadrantButtonStates()
        Dim qBtns = {BtnQuadrant1, BtnQuadrant2, BtnQuadrant3, BtnQuadrant4}
        For q = 1 To 4
            qBtns(q - 1).Background = If(Sel.IsQuadrantActive(q), Brushes.Gold, Brushes.Orange)
        Next

        ' First/Last is incompatible with half-board (2 active quadrants).
        Dim isHalf = Sel.CountActiveQuadrants() >= 2
        If isHalf AndAlso Vm.UseFirstLast Then
            Vm.UseFirstLast = False
        End If
        ChkFirstLastPlay.IsEnabled = Not isHalf
    End Sub


    ' ── status bar actions

    Private Sub SbiSettings_MouseDown(sender As Object, e As MouseButtonEventArgs)
        FlyoutSettings.IsOpen = Not FlyoutSettings.IsOpen
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
                        "Jackpot resets to $25,000 when won.",
                        "Progressive Jackpot", MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        If Vm.IsPlaying Then
            ' Stop the running game first; it settles what was played, then re-closes the window.
            e.Cancel = True
            _closeRequested = True
            _playCts?.Cancel()
            Return
        End If
        If _sessionGamesPlayed = 0 Then Return
        Dim netPL = _bankBalance - _sessionStartBalance
        WinSessionSummary.ShowSummary(Me, netPL, _sessionGamesPlayed, _sessionWins,
                                      _sessionBestPayout, _sessionFreeGamesEarned, _sessionTotalWagered)
    End Sub

End Class