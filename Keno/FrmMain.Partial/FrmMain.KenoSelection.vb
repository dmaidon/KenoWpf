' Last Edit: 2026-03-13 - IsFreeGameBonus extended to Pick 9; BtnPlay_Click excludes FirstLast from consecutive bonus.
Imports System.IO
Imports System.Security.Cryptography

Partial Class FrmMain
    Private ReadOnly _selectedNumbers As New SortedSet(Of Integer)()
    Private ReadOnly _kenoLabels As New Dictionary(Of Integer, Label)()
    Private ReadOnly _numSelectBoxes As New List(Of TextBox)()
    Private ReadOnly _selectedQuadrants As New HashSet(Of Integer)()
    Private ReadOnly _quadrantLabels As New Dictionary(Of Integer, Label)()
    Private _quadrantDefaultBackColor As Color
    Private ReadOnly _gamePlayLabels As New List(Of Label)()
    Private _gamePlayDefaultBackColor As Color
    Private _gamePlayDefaultForeColor As Color

    Private Enum GameState
        Idle
        Playing
        Complete
    End Enum

    Private _gameState As GameState = GameState.Idle
    Private _multiplierValue As Integer = 1
    Private _wayTicketGroups As List(Of List(Of Integer)) = Nothing
    Private _wayTicketKingNumber As Integer = 0
    Private _isBullseyeActive As Boolean = False
    Private Shared ReadOnly BullseyeNumbers As Integer() = {1, 10, 35, 36, 45, 46, 71, 80}
    Private ReadOnly _toolTip As New ToolTip()
    Private _lastMatchedCount As Integer
    Private _lastPayout As Decimal
    Private _lastFirstLastBonus As Decimal
    Private _gamesPlayed As Integer
    Private Const UserPickColor As KnownColor = KnownColor.LightGreen
    Private Const DrawPickColor As KnownColor = KnownColor.LightSkyBlue
    Private Const MatchPickColor As KnownColor = KnownColor.Gold
    Private Const MatchDrawColor As KnownColor = KnownColor.RoyalBlue
    Private Const PowerballDrawColor As KnownColor = KnownColor.OrangeRed
    Private Const GameInProgressColor As KnownColor = KnownColor.LightSkyBlue
    Private Const GamePlayedColor As KnownColor = KnownColor.LightGray
    Private _sessionStartBalance As Decimal
    Private _sessionGamesPlayed As Integer
    Private _sessionWins As Integer
    Private _sessionBestPayout As Decimal
    Private _sessionFreeGamesEarned As Integer
    Private _sessionTotalWagered As Decimal
    Private _currentTotalBet As Decimal

    Private Sub FrmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            StsCpy.Text = cpy
            InitializeWagerTags()
            InitializeKenoCaches()
            For Each label In _kenoLabels.Values
                AddHandler label.Click, AddressOf KenoNumber_Click
            Next

            InitializeQuadrantSelection()
            InitializeConsecutiveGameSelection()

            Dim speedIndex = LoadAppSettings().DrawSpeedIndex
            Select Case speedIndex
                Case 1 : RbDrawSpeedMedium.Checked = True
                Case 2 : RbDrawSpeedFast.Checked = True
                Case Else : RbDrawSpeedSlow.Checked = True
            End Select

            EnsureDrawStats()
            EnsureAllTimeSummary()
            EnsureJackpot()
            IncrementSessions()
            _gamesPlayed = LoadStats().GamesPlayed
            _sessionStartBalance = GetBankBalance()
            UpdateFormIcon()
            ApplyWindowSettings()
            UpdateBankDisplay()
            UpdateWinningsDisplay(0D)
            UpdateStatus()
            UpdateStreakDisplay()
            UpdateJackpotDisplay()
            BtnPlay.Enabled = False
            UpdateFreeGamesButton()
            InitializeToolTips()
            InitializeHotColdLabels()
        Catch ex As Exception
            LogError(ex, NameOf(FrmMain_Load))
        End Try
    End Sub

    Private Sub FrmMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            SaveWindowSettings()
            If _sessionGamesPlayed > 0 Then
                ShowSessionSummary()
            End If
        Catch ex As Exception
            LogError(ex, NameOf(FrmMain_FormClosing))
        End Try
    End Sub

    Private Async Sub BtnPlay_Click(sender As Object, e As EventArgs) Handles BtnPlay.Click
        Try
            Dim betAmount = GetSelectedBetAmount()
            Dim hasSelection = _selectedNumbers.Count > 0 OrElse _selectedQuadrants.Count > 0
            Dim gamesToPlay = GetConsecutiveGamesCount()
            Dim totalBet = GetTotalBet(betAmount, gamesToPlay)
            If Not hasSelection OrElse betAmount <= 0D OrElse Not HasSufficientFunds(totalBet) Then
                UpdatePlayState()
                Return
            End If

            Dim modeLabel = GetGameModeLabel()
            _gameState = GameState.Playing
            UpdatePlayState()
            DeductBetFromBank(totalBet)
            _sessionTotalWagered += totalBet
            _currentTotalBet = totalBet
            UpdateWagerTotalDisplay(totalBet)

            If betAmount >= 5D Then
                Dim effectiveGames = If(IsWayTicketActive(), _wayTicketGroups.Count, gamesToPlay)
                AddToJackpot(betAmount * effectiveGames * 0.05D)
                UpdateJackpotDisplay()
            End If

            UpdateWinningsDisplay(0D)
            _lastMatchedCount = 0
            _lastPayout = 0D
            _lastFirstLastBonus = 0D
            UpdateStatus()
            ResetGamePlayLabels()

            If Not ChkMultiplierKeno.Checked Then
                _multiplierValue = 1
            End If

            Dim gameResults As New List(Of (Index As Integer, Matched As Integer, Payout As Decimal, Multiplier As Integer, FirstLastBonus As Decimal))()
            Dim freeGameIndices As New HashSet(Of Integer)()
            Dim totalPayout = 0D
            If IsWayTicketActive() Then
                totalPayout = Await PlayWayTicketAsync(betAmount)
            Else
                For gameIndex = 1 To gamesToPlay
                    If ChkMultiplierKeno.Checked Then
                        _multiplierValue = DrawMultiplierValue()
                        UpdateMultiplierDisplay(_multiplierValue)
                    End If
                    SetGamePlayInProgress(gameIndex)
                    Dim result = Await PlaySingleGameAsync(betAmount)
                    Dim isFreeGame = result.Payout = 0D AndAlso IsFreeGameBonus(result.Matched, betAmount)
                    UpdateGamePlayResult(gameIndex, result.Payout, isFreeGame, result.FirstBallHit, result.LastBallHit)
                    totalPayout += result.Payout
                    UpdateWinningsDisplay(totalPayout)
                    gameResults.Add((gameIndex, result.Matched, result.Payout, _multiplierValue, result.FirstLastBonus))
                    If isFreeGame Then freeGameIndices.Add(gameIndex)

                    If result.Payout > 0D Then
                        ShowPayoutSchedulePopup(result.Payout, result.Matched, betAmount)
                    ElseIf isFreeGame Then
                        AwardFreeGameBonus()
                    ElseIf gameIndex < gamesToPlay Then
                        Await Task.Delay(GetBetweenGameDelayMs())
                    End If
                Next
            End If

            Dim subtotalBeforeBonus = totalPayout
            Dim totalFirstLastBonus = gameResults.Sum(Function(r) r.FirstLastBonus)
            Dim bonus = GetConsecutiveBonus()
            If bonus > 1D Then
                totalPayout = (subtotalBeforeBonus - totalFirstLastBonus) * bonus + totalFirstLastBonus
            End If
            ApplyTotalWinnings(totalPayout)

            If gamesToPlay > 1 AndAlso gameResults.Count > 0 Then
                FrmConsecutiveSummary.ShowSummary(Me, gameResults, betAmount, bonus, subtotalBeforeBonus, totalPayout)
            End If

            If gameResults.Count = 0 Then
                AppendGame(modeLabel, betAmount, -1, totalPayout, _multiplierValue)
            ElseIf gameResults.Count = 1 Then
                AppendGame(modeLabel, betAmount, gameResults(0).Matched, gameResults(0).Payout, _multiplierValue, freeGameIndices.Count > 0, gameResults(0).FirstLastBonus)
            Else
                AppendBatch(modeLabel, betAmount, gameResults.Select(Function(r) (r.Matched, r.Payout, freeGameIndices.Contains(r.Index), r.Multiplier, r.FirstLastBonus)), bonus)
            End If

            DisableKenoGridSelection()

            If ChkMultiplierKeno.Checked Then
                ChkMultiplierKeno.Checked = False
            End If
            If ChkWayTicket.Checked Then
                ChkWayTicket.Checked = False
                LblWayTicketSummary.Text = $"Done - Total: {totalPayout:C2}"
            End If
            If ChkPowerball.Checked Then ChkPowerball.Checked = False
            If ChkFirstLastPlay.Checked Then ChkFirstLastPlay.Checked = False
            _gameState = GameState.Complete
            UpdatePlayState()
        Catch ex As Exception
            LogError(ex, NameOf(BtnPlay_Click))
        End Try
    End Sub

    Private Sub BtnQuickPick_Click(sender As Object, e As EventArgs) Handles BtnQuickPick.Click
        Try
            If _gameState <> GameState.Idle Then Return

            _selectedNumbers.Clear()
            _selectedQuadrants.Clear()
            _isBullseyeActive = False
            UpdateBullseyeButtonVisual()
            ResetKenoGridHighlights()
            UpdateQuadrantSelectionVisuals()

            For Each number In GetRandomPicks(CInt(NudQuickPick.Value))
                _selectedNumbers.Add(number)
                Dim label = FindKenoLabel(number)
                If label IsNot Nothing Then
                    UpdateLabelSelectionVisual(label, True)
                End If
            Next

            UpdateSelectedNumbers()
            UpdatePlayState()
            UpdateStatus()
        Catch ex As Exception
            LogError(ex, NameOf(BtnQuickPick_Click))
        End Try
    End Sub

    Private Sub BtnBullseye_Click(sender As Object, e As EventArgs) Handles BtnBullseye.Click
        Try
            If _gameState <> GameState.Idle Then Return

            _selectedNumbers.Clear()
            _selectedQuadrants.Clear()

            If ChkWayTicket.Checked Then ChkWayTicket.Checked = False

            ResetKenoGridHighlights()
            EnableKenoGridSelection()
            UpdateQuadrantSelectionVisuals()

            For Each n In BullseyeNumbers
                _selectedNumbers.Add(n)
                Dim label = FindKenoLabel(n)
                If label IsNot Nothing Then
                    UpdateLabelSelectionVisual(label, True)
                End If
            Next

            _isBullseyeActive = True
            UpdateBullseyeButtonVisual()
            UpdateSelectedNumbers()
            UpdatePlayState()
            UpdateStatus()
        Catch ex As Exception
            LogError(ex, NameOf(BtnBullseye_Click))
        End Try
    End Sub

    Private Sub UpdateBullseyeButtonVisual()
        BtnBullseye.BackColor = If(_isBullseyeActive, Color.Gold, Color.MistyRose)
    End Sub

    Private Sub ChkMultiplierKeno_CheckedChanged(sender As Object, e As EventArgs) Handles ChkMultiplierKeno.CheckedChanged
        If Not ChkMultiplierKeno.Checked Then
            _multiplierValue = 1
            UpdateMultiplierDisplay(1)
        End If
        UpdatePlayState()
    End Sub

    Private Sub ChkWayTicket_CheckedChanged(sender As Object, e As EventArgs) Handles ChkWayTicket.CheckedChanged
        If ChkWayTicket.Checked Then
            Dim betAmount = GetSelectedBetAmount()
            If _selectedNumbers.Count < 4 Then
                MessageBox.Show("Select at least 4 numbers before enabling Way Ticket.", "Way Ticket", MessageBoxButtons.OK, MessageBoxIcon.Information)
                ChkWayTicket.Checked = False
                Return
            End If
            If betAmount <= 0D Then
                MessageBox.Show("Select a bet amount before enabling Way Ticket.", "Way Ticket", MessageBoxButtons.OK, MessageBoxIcon.Information)
                ChkWayTicket.Checked = False
                Return
            End If
            Dim frm = FrmWayTicket.ShowSetup(Me, _selectedNumbers, betAmount)
            If frm IsNot Nothing Then
                _wayTicketGroups = frm.Groups
                _wayTicketKingNumber = frm.KingNumber
                UpdateWayTicketSummary()
            Else
                ChkWayTicket.Checked = False
            End If
        Else
            _wayTicketGroups = Nothing
            _wayTicketKingNumber = 0
            LblWayTicketSummary.Text = String.Empty
        End If
        UpdatePlayState()
    End Sub

    Private Sub ChkPowerball_CheckedChanged(sender As Object, e As EventArgs) Handles ChkPowerball.CheckedChanged
        If ChkPowerball.Checked Then
            Dim count = _selectedNumbers.Count
            If count < 6 OrElse count > 20 Then
                MessageBox.Show("Powerball Play is only available for 6 to 20 selected numbers.", "Powerball Play", MessageBoxButtons.OK, MessageBoxIcon.Information)
                ChkPowerball.Checked = False
                Return
            End If
        End If
        UpdatePlayState()
    End Sub

    Private Sub ChkFirstLastPlay_CheckedChanged(sender As Object, e As EventArgs) Handles ChkFirstLastPlay.CheckedChanged
        UpdatePlayState()
        UpdatePayoutScheduleDisplay()
    End Sub

    Private Sub BtnReplayLastGame_Click(sender As Object, e As EventArgs) Handles BtnReplayLastGame.Click
        Try
            If _gameState <> GameState.Complete Then Return
            If _selectedNumbers.Count = 0 AndAlso _selectedQuadrants.Count = 0 Then Return
            _gameState = GameState.Idle
            BtnPlay_Click(sender, e)
        Catch ex As Exception
            LogError(ex, NameOf(BtnReplayLastGame_Click))
        End Try
    End Sub

    Private Sub BtnSaveFavorites_Click(sender As Object, e As EventArgs) Handles BtnSaveFavorites.Click
        Try
            If _selectedNumbers.Count = 0 Then
                MessageBox.Show("Select numbers before saving favorites.", "Save Favorites", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim slotIndex = FrmFavoritesSlot.ShowForSave(Me, _selectedNumbers.Count)
            If slotIndex < 0 Then Return

            SaveFavorites(slotIndex, _selectedNumbers)
            UpdatePlayState()
            MessageBox.Show($"{_selectedNumbers.Count} number{If(_selectedNumbers.Count = 1, "", "s")} saved to Slot {slotIndex + 1}.", "Save Favorites", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            LogError(ex, NameOf(BtnSaveFavorites_Click))
        End Try
    End Sub

    Private Sub BtnPlayFavorites_Click(sender As Object, e As EventArgs) Handles BtnPlayFavorites.Click
        Try
            If Not HasFavorites() Then
                MessageBox.Show("No favorites saved yet. Select numbers and click Save My Favorites first.", "Play Favorites", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim slotIndex = FrmFavoritesSlot.ShowForLoad(Me)
            If slotIndex < 0 Then Return

            Dim favorites = LoadFavorites(slotIndex)
            If favorites.Length = 0 Then Return

            ' Reset for new game, keeping bet and consecutive settings
            _gameState = GameState.Idle
            _selectedNumbers.Clear()
            _selectedQuadrants.Clear()
            _isBullseyeActive = False
            UpdateBullseyeButtonVisual()

            If ChkWayTicket.Checked Then ChkWayTicket.Checked = False
            If ChkPowerball.Checked Then ChkPowerball.Checked = False

            ResetKenoGridHighlights()
            EnableKenoGridSelection()
            UpdateQuadrantSelectionVisuals()

            For Each number In favorites.Take(20)
                If number >= 1 AndAlso number <= 80 Then
                    _selectedNumbers.Add(number)
                    Dim label = FindKenoLabel(number)
                    If label IsNot Nothing Then
                        UpdateLabelSelectionVisual(label, True)
                    End If
                End If
            Next

            UpdateSelectedNumbers()
            UpdateWinningsDisplay(0D)
            UpdatePlayState()
            UpdateStatus()
        Catch ex As Exception
            LogError(ex, NameOf(BtnPlayFavorites_Click))
        End Try
    End Sub

    Private Sub BtnClear_Click(sender As Object, e As EventArgs) Handles BtnClear.Click
        Try
            _gameState = GameState.Idle
            _selectedNumbers.Clear()
            _selectedQuadrants.Clear()
            _isBullseyeActive = False
            UpdateBullseyeButtonVisual()

            If ChkWayTicket.Checked Then ChkWayTicket.Checked = False
            If ChkPowerball.Checked Then ChkPowerball.Checked = False

            UpdateSelectedNumbers()
            ResetNumSelectHighlights()
            ResetKenoGridHighlights()
            EnableKenoGridSelection()
            UpdateQuadrantSelectionVisuals()
            ResetConsecutiveSelection()
            ResetBetSelection()
            UpdateWinningsDisplay(0D)
            UpdateWagerTotalDisplay(0D)
            UpdatePlayState()
            UpdatePayoutScheduleDisplay()
        Catch ex As Exception
            LogError(ex, NameOf(BtnClear_Click))
        End Try
    End Sub

    Private Sub ResetNumSelectHighlights()
        For Each control As Control In TlpNumbersPicked.Controls
            Dim textBox = TryCast(control, TextBox)
            If textBox Is Nothing Then
                Continue For
            End If

            textBox.BackColor = SystemColors.Window
        Next
    End Sub

    Private Sub KenoNumber_Click(sender As Object, e As EventArgs)
        Try
            If _gameState <> GameState.Idle Then
                Return
            End If

            If _selectedQuadrants.Count > 0 Then
                Return
            End If

            Dim label = TryCast(sender, Label)
            If label Is Nothing Then
                Return
            End If

            Dim number As Integer
            If Not Integer.TryParse(label.Text, number) Then
                Return
            End If

            If _selectedNumbers.Remove(number) Then
                UpdateLabelSelectionVisual(label, False)
            ElseIf _selectedNumbers.Count < 20 AndAlso _selectedNumbers.Add(number) Then
                UpdateLabelSelectionVisual(label, True)
            Else
                Return
            End If

            _isBullseyeActive = False
            UpdateBullseyeButtonVisual()

            UpdateSelectedNumbers()
            UpdatePlayState()
            UpdateStatus()
        Catch ex As Exception
            LogError(ex, NameOf(KenoNumber_Click))
        End Try
    End Sub

    Private Sub DisableKenoGridSelection()
        For Each label In _kenoLabels.Values
            label.Enabled = False
        Next
    End Sub

    Private Sub EnableKenoGridSelection()
        For Each label In _kenoLabels.Values
            label.Enabled = True
        Next
    End Sub

    Private Sub ResetKenoGridHighlights()
        For Each entry In _kenoLabels
            Dim lbl = entry.Value
            lbl.BackColor = Color.Transparent
            lbl.ForeColor = SystemColors.ControlText
            lbl.Text = entry.Key.ToString()
        Next
        ResetPowerballDisplay()
    End Sub

    Private Sub UpdatePlayState()
        Dim betAmount = GetSelectedBetAmount()
        Dim hasNumbers = _selectedNumbers.Count > 0 OrElse _selectedQuadrants.Count > 0
        Dim wayTicketReady = Not ChkWayTicket.Checked OrElse IsWayTicketActive()
        Dim powerballOk = Not ChkPowerball.Checked OrElse (_selectedNumbers.Count >= 6 AndAlso _selectedNumbers.Count <= 20)
        Dim hasSelection = hasNumbers AndAlso wayTicketReady AndAlso powerballOk
        Dim gamesToPlay = GetConsecutiveGamesCount()
        Dim totalBet = GetTotalBet(betAmount, gamesToPlay)
        BtnPlay.Enabled = hasSelection AndAlso betAmount > 0D AndAlso HasSufficientFunds(totalBet) AndAlso _gameState = GameState.Idle
        BtnReplayLastGame.Enabled = _gameState = GameState.Complete AndAlso hasNumbers AndAlso betAmount > 0D AndAlso HasSufficientFunds(totalBet)
        BtnSaveFavorites.Enabled = _selectedNumbers.Count > 0 AndAlso _gameState = GameState.Idle
        BtnPlayFavorites.Enabled = _gameState = GameState.Idle AndAlso HasFavorites()
        UpdateFreeGamesButton()
        UpdateWagerTotalDisplay(totalBet)
    End Sub

    Private Sub UpdateFreeGamesButton()
        Dim count = GetFreeGames()
        Dim template = TryCast(BtnFreeGames.Tag, String)
        If template IsNot Nothing Then
            BtnFreeGames.Text = String.Format(template, count)
        End If
        BtnFreeGames.Enabled = count > 0 AndAlso _gameState = GameState.Idle
    End Sub

    Private Sub RbWager_CheckedChanged(sender As Object, e As EventArgs) Handles RbWager1.CheckedChanged, RbWager2.CheckedChanged, RbWager3.CheckedChanged, RbWager5.CheckedChanged, RbWager10.CheckedChanged, RbWager15.CheckedChanged, RbWager20.CheckedChanged, RbWager25.CheckedChanged, RbWager30.CheckedChanged, RbWager40.CheckedChanged, RbWager50.CheckedChanged, RbWager100.CheckedChanged, RbWager150.CheckedChanged, RbWager200.CheckedChanged
        NudSpecialWager.Value = 0D
        UpdateWayTicketSummary()
        UpdatePlayState()
        UpdatePayoutScheduleDisplay()
    End Sub

    Private Sub NudSpecialWager_ValueChanged(sender As Object, e As EventArgs) Handles NudSpecialWager.ValueChanged
        If NudSpecialWager.Value > 0D Then
            RbWager1.Checked = False
            RbWager2.Checked = False
            RbWager3.Checked = False
            RbWager5.Checked = False
            RbWager10.Checked = False
            RbWager15.Checked = False
            RbWager20.Checked = False
            RbWager25.Checked = False
            RbWager30.Checked = False
            RbWager40.Checked = False
            RbWager50.Checked = False
            RbWager100.Checked = False
            RbWager150.Checked = False
            RbWager200.Checked = False
        End If
        UpdateWayTicketSummary()
        UpdatePlayState()
        UpdatePayoutScheduleDisplay()
    End Sub

    Private Sub Nud_Enter(sender As Object, e As EventArgs) Handles NudQuickPick.Enter, NudSpecialWager.Enter
        Dim nud = TryCast(sender, NumericUpDown)
        nud?.Select(0, nud.Text.Length)
    End Sub

    Private Sub RbConsecutive_CheckedChanged(sender As Object, e As EventArgs) Handles RbConsecutive2.CheckedChanged, RbConsecutive3.CheckedChanged, RbConsecutive4.CheckedChanged, RbConsecutive5.CheckedChanged, RbConsecutive6.CheckedChanged, RbConsecutive7.CheckedChanged, RbConsecutive8.CheckedChanged, RbConsecutive9.CheckedChanged, RbConsecutive10.CheckedChanged, RbConsecutive11.CheckedChanged, RbConsecutive12.CheckedChanged, RbConsecutive13.CheckedChanged, RbConsecutive14.CheckedChanged, RbConsecutive15.CheckedChanged, RbConsecutive16.CheckedChanged, RbConsecutive17.CheckedChanged, RbConsecutive18.CheckedChanged, RbConsecutive19.CheckedChanged, RbConsecutive20.CheckedChanged
        UpdateConsecutiveGameLabels()
        UpdateBonusMultiplierDisplay()
        UpdatePlayState()
    End Sub

    Private Function GetSelectedBetAmount() As Decimal
        Dim wagerButtons As RadioButton() = {
            RbWager1, RbWager2, RbWager3, RbWager5, RbWager10, RbWager15, RbWager20,
            RbWager25, RbWager30, RbWager40, RbWager50, RbWager100, RbWager150, RbWager200
        }
        Dim checked = wagerButtons.FirstOrDefault(Function(rb) rb.Checked)
        If checked IsNot Nothing Then Return CDec(checked.Tag)
        If NudSpecialWager.Value > 0D Then Return NudSpecialWager.Value

        Return 0D
    End Function

    Private Sub DeductBetFromBank(betAmount As Decimal)
        Dim balance = GetBankBalance()
        balance -= betAmount

        SaveBankBalance(balance)
        UpdateBankDisplay()
    End Sub

    Private Sub ApplyWinnings(betAmount As Decimal, matchedCount As Integer)
        Dim payout = GetGamePayout(matchedCount) * betAmount
        _lastMatchedCount = matchedCount
        _lastPayout = payout
        UpdateWinningsDisplay(payout)
        UpdateStatus()
        If payout <= 0D Then
            Return
        End If

        Dim balance = GetBankBalance()
        balance += payout

        SaveBankBalance(balance)
        UpdateBankDisplay()
    End Sub

    Private Sub UpdateBankDisplay()
        Dim balance = GetBankBalance()
        LblBankAccount.Text = balance.ToString("C2")
        NudSpecialWager.Maximum = Math.Min(balance, 1000D)
        If NudSpecialWager.Value > NudSpecialWager.Maximum Then
            NudSpecialWager.Value = NudSpecialWager.Maximum
        End If
    End Sub

    Private Sub UpdateWinningsDisplay(amount As Decimal)
        LblWinningsValue.Text = amount.ToString("C2")
        If _currentTotalBet > 0D AndAlso amount > _currentTotalBet Then
            LblWinningsValue.BackColor = Color.LimeGreen
            LblWinningsValue.ForeColor = Color.Black
        ElseIf _currentTotalBet > 0D Then
            LblWinningsValue.BackColor = Color.Red
            LblWinningsValue.ForeColor = Color.White
        Else
            LblWinningsValue.BackColor = Color.FromArgb(128, 255, 128)
            LblWinningsValue.ForeColor = SystemColors.ControlText
        End If
    End Sub

    Private Sub UpdateWagerTotalDisplay(amount As Decimal)
        LblWagerTotal.Text = amount.ToString("C2")
    End Sub

    Private Sub UpdateStatus()
        StsPicks.Text = $"Picks: {_selectedNumbers.Count}"
        StsMatches.Text = $"Matches: {_lastMatchedCount}"
        StsPayout.Text = $"Payout: {_lastPayout:C2}"
        StsGames.Text = $"Games: {_gamesPlayed}"

        If HasErrorLogForToday() Then
            StsErrorCheck.Text = "Error! Detected"
            StsErrorCheck.ForeColor = Color.Red
        Else
            StsErrorCheck.Text = "-"
            StsErrorCheck.ForeColor = Color.ForestGreen
        End If
        UpdatePayoutScheduleDisplay()
    End Sub

    Private Sub UpdateDrawStats(picks As IEnumerable(Of Integer), payout As Decimal)
        Try
            Dim won = payout > 0D
            Dim stats = RecordDraw(picks, won)
            _gamesPlayed = stats.GamesPlayed

            _sessionGamesPlayed += 1
            If won Then _sessionWins += 1
            If payout > _sessionBestPayout Then _sessionBestPayout = payout

            UpdateStreakDisplay(stats)

            If stats.GamesPlayed < 5 Then
                Return
            End If

            Dim windowCounts = BuildWindowCounts(stats)
            Dim ordered = windowCounts.OrderByDescending(Function(entry) entry.Value).ThenBy(Function(entry) entry.Key).ToList()
            Dim hot = ordered.Take(5).Select(Function(entry) entry.Key).ToArray()
            Dim cold = ordered.OrderBy(Function(entry) entry.Value).ThenBy(Function(entry) entry.Key).Take(5).Select(Function(entry) entry.Key).ToArray()

            UpdateHotColdLabels(hot, cold)
        Catch ex As Exception
            LogError(ex, NameOf(UpdateDrawStats))
        End Try
    End Sub

    Private Function BuildWindowCounts(stats As DrawStatsStore.DrawStats) As Dictionary(Of Integer, Integer)
        Dim counts As New Dictionary(Of Integer, Integer)()
        For number = 1 To 80
            counts(number) = 0
        Next

        If stats.RecentDraws Is Nothing Then
            Return counts
        End If

        For Each draw In stats.RecentDraws
            For Each pick In draw
                If pick >= 1 AndAlso pick <= 80 Then
                    counts(pick) += 1
                End If
            Next
        Next

        Return counts
    End Function

    Private Sub UpdateHotColdLabels(hot As Integer(), cold As Integer())
        Dim hotLabels As Label() = {LblHot1, LblHot2, LblHot3, LblHot4, LblHot5}
        Dim coldLabels As Label() = {LblCold1, LblCold2, LblCold3, LblCold4, LblCold5}

        For index = 0 To hotLabels.Length - 1
            hotLabels(index).Text = If(index < hot.Length, hot(index).ToString(), String.Empty)
            coldLabels(index).Text = If(index < cold.Length, cold(index).ToString(), String.Empty)
        Next
    End Sub

    Private Function HasSufficientFunds(betAmount As Decimal) As Boolean
        If betAmount <= 0D Then
            Return False
        End If

        Return GetBankBalance() >= betAmount
    End Function

    Private Sub ResetBetSelection()
        RbWager1.Checked = False
        RbWager2.Checked = False
        RbWager3.Checked = False
        RbWager5.Checked = False
        RbWager10.Checked = False
        RbWager15.Checked = False
        RbWager20.Checked = False
        RbWager25.Checked = False
        RbWager30.Checked = False
        RbWager40.Checked = False
        RbWager50.Checked = False
        RbWager100.Checked = False
        RbWager150.Checked = False
        RbWager200.Checked = False
        NudSpecialWager.Value = 0D
    End Sub

    Private Function FindKenoLabel(number As Integer) As Label
        Dim label As Label = Nothing
        If _kenoLabels.TryGetValue(number, label) Then
            Return label
        End If

        Return Nothing
    End Function

    Private Function FindNumSelectTextBox(number As Integer) As TextBox
        For Each textBox In _numSelectBoxes
            If textBox.Text = number.ToString() Then
                Return textBox
            End If
        Next

        Return Nothing
    End Function

    Private Sub UpdateSelectedNumbers()
        Dim selectedNumbers = _selectedNumbers.ToArray()
        For index = 0 To _numSelectBoxes.Count - 1
            If index < selectedNumbers.Length Then
                _numSelectBoxes(index).Text = selectedNumbers(index).ToString()
            Else
                _numSelectBoxes(index).Text = String.Empty
            End If
        Next

    End Sub

    Private Sub UpdateLabelSelectionVisual(label As Label, isSelected As Boolean)
        If isSelected Then
            label.BackColor = Color.FromKnownColor(UserPickColor)
            label.ForeColor = SystemColors.ControlText
        Else
            label.BackColor = Color.Transparent
            label.ForeColor = SystemColors.ControlText
        End If
    End Sub

    Private Sub InitializeWagerTags()
        RbWager1.Tag = 1D
        RbWager2.Tag = 2D
        RbWager3.Tag = 3D
        RbWager5.Tag = 5D
        RbWager10.Tag = 10D
        RbWager15.Tag = 15D
        RbWager20.Tag = 20D
        RbWager25.Tag = 25D
        RbWager30.Tag = 30D
        RbWager40.Tag = 40D
        RbWager50.Tag = 50D
        RbWager100.Tag = 100D
        RbWager150.Tag = 150D
        RbWager200.Tag = 200D
    End Sub

    Private Sub InitializeKenoCaches()
        _kenoLabels.Clear()
        For Each tlp In {TlpKenoGridTop, TlpKenoGridBottom}
            For Each control As Control In tlp.Controls
                Dim label = TryCast(control, Label)
                If label Is Nothing Then Continue For

                Dim number As Integer
                If Integer.TryParse(label.Text, number) Then
                    _kenoLabels(number) = label
                End If
            Next
        Next

        _numSelectBoxes.Clear()
        _numSelectBoxes.AddRange(New TextBox() {
            NumSelect1, NumSelect2, NumSelect3, NumSelect4, NumSelect5,
            NumSelect6, NumSelect7, NumSelect8, NumSelect9, NumSelect10,
            NumSelect11, NumSelect12, NumSelect13, NumSelect14, NumSelect15,
            NumSelect16, NumSelect17, NumSelect18, NumSelect19, NumSelect20
        })
    End Sub

    Private Sub InitializeToolTips()
        _toolTip.AutoPopDelay = 8000
        _toolTip.InitialDelay = 400
        _toolTip.ReshowDelay = 300

        _toolTip.SetToolTip(BtnPlay,
            "Play the selected numbers with the chosen bet amount.")

        _toolTip.SetToolTip(BtnClear,
            "Clear all selections and reset for a new game.")

        _toolTip.SetToolTip(BtnFreeGames,
            "Play a free $2 game using your current selection." & Environment.NewLine &
            "Earned by playing Pick 5, 6, or 7 with a $2+ bet and matching zero numbers.")

        _toolTip.SetToolTip(GbxWager,
            "Select how much to bet per game." & Environment.NewLine &
            "All payouts are multiplied by your bet amount.")

        _toolTip.SetToolTip(GbxConsecutiveGames,
            "Play multiple games in a row with the same numbers and bet." & Environment.NewLine &
            "The full total is deducted upfront. Winnings are credited after the last game.")

        _toolTip.SetToolTip(BtnQuickPick,
            "Randomly select numbers for you. Set the count with the spinner to the left.")

        _toolTip.SetToolTip(NudQuickPick,
            "How many numbers Quick Pick will randomly choose (1?10).")

        _toolTip.SetToolTip(BtnSaveFavorites,
            "Save your currently selected numbers to one of 3 favorites slots.")

        _toolTip.SetToolTip(BtnPlayFavorites,
            "Load numbers from one of your 3 saved favorites slots." & Environment.NewLine &
            "Keeps your current bet and consecutive settings.")

        _toolTip.SetToolTip(BtnReplayLastGame,
            "Replay the last game with the same numbers and bet." & Environment.NewLine &
            "Available after a game completes. Add-ons (Multiplier, Powerball) can be re-selected before replaying.")

        _toolTip.SetToolTip(GbxSpecialPlay,
            "Quick Pick: randomly select numbers.  |  Multiplier Keno: draw a weighted x1–x10 win multiplier ($1 extra)." & Environment.NewLine &
            "Way Ticket: split picks into equal sub-ticket groups.  |  Powerball: draw a 21st ball; x4 on hit ($1 extra, Pick 6–20)." & Environment.NewLine &
            "Bullseye: select the 4 corners + 4 center cells (fixed 8-spot with special payouts).")

        _toolTip.SetToolTip(BtnBullseye,
            "Select the 4 corners (1,10,71,80) and 4 center cells (35,36,45,46) of the grid." & Environment.NewLine &
            "Uses the Bullseye payout schedule. Jackpot: 30,000x for all 8!")

        _toolTip.SetToolTip(ChkFirstLastPlay,
            "If the first or last drawn ball is among your picks, earn a flat bonus award ($1 extra per game)." & Environment.NewLine &
            "Bonus: $75 for Pick 1, decreasing to $5 for Pick 20. Not available in Quadrant mode.")

        _toolTip.SetToolTip(TlpQuadrants,
            "Select one quadrant or two adjacent quadrants to play an area game." & Environment.NewLine &
            "Q1+Q2 = Top half  |  Q3+Q4 = Bottom half" & Environment.NewLine &
            "Q1+Q3 = Left half  |  Q2+Q4 = Right half")

        _toolTip.SetToolTip(TableLayoutPanel2,
            "Draw Speed controls how fast each ball is drawn during a game." & Environment.NewLine &
            "Slow = 1 sec/ball  |  Medium = 0.5 sec/ball  |  Fast = 0.25 sec/ball")

        _toolTip.SetToolTip(LblBonusMultiplier,
            "Bonus multiplier applied to total payout for consecutive games." & Environment.NewLine &
            "5–7 games: 1.1×  |  8–11 games: 1.25×  |  12–16 games: 1.5×  |  17–20 games: 1.75×")
    End Sub

    Private Sub InitializeQuadrantSelection()
        _quadrantLabels.Clear()
        _quadrantLabels(1) = LblQuadrant1
        _quadrantLabels(2) = LblQuadrant2
        _quadrantLabels(3) = LblQuadrant3
        _quadrantLabels(4) = LblQuadrant4

        _quadrantDefaultBackColor = LblQuadrant1.BackColor

        For Each entry In _quadrantLabels
            AddHandler entry.Value.Click, AddressOf QuadrantLabel_Click
        Next
    End Sub

    Private Sub InitializeConsecutiveGameSelection()
        _gamePlayLabels.Clear()
        _gamePlayLabels.AddRange(New Label() {
            LblGamePlay1, LblGamePlay2, LblGamePlay3, LblGamePlay4,
            LblGamePlay5, LblGamePlay6, LblGamePlay7,
            LblGamePlay8, LblGamePlay9, LblGamePlay10,
            LblGamePlay11, LblGamePlay12, LblGamePlay13, LblGamePlay14,
            LblGamePlay15, LblGamePlay16, LblGamePlay17, LblGamePlay18,
            LblGamePlay19, LblGamePlay20
        })

        _gamePlayDefaultBackColor = LblGamePlay1.BackColor
        _gamePlayDefaultForeColor = LblGamePlay1.ForeColor

        UpdateConsecutiveGameLabels()
        UpdateBonusMultiplierDisplay()
    End Sub

    Private Function GetDrawSpeedMs() As Integer
        If RbDrawSpeedFast.Checked Then
            Return 250
        End If

        If RbDrawSpeedMedium.Checked Then
            Return 500
        End If

        Return 1000
    End Function

    Private Function GetConsecutiveGamesCount() As Integer
        If RbConsecutive2.Checked Then
            Return 2
        End If

        If RbConsecutive3.Checked Then
            Return 3
        End If

        If RbConsecutive4.Checked Then
            Return 4
        End If

        If RbConsecutive5.Checked Then
            Return 5
        End If

        If RbConsecutive6.Checked Then
            Return 6
        End If

        If RbConsecutive7.Checked Then
            Return 7
        End If

        If RbConsecutive8.Checked Then
            Return 8
        End If

        If RbConsecutive9.Checked Then
            Return 9
        End If

        If RbConsecutive10.Checked Then Return 10
        If RbConsecutive11.Checked Then Return 11
        If RbConsecutive12.Checked Then Return 12
        If RbConsecutive13.Checked Then Return 13
        If RbConsecutive14.Checked Then Return 14
        If RbConsecutive15.Checked Then Return 15
        If RbConsecutive16.Checked Then Return 16
        If RbConsecutive17.Checked Then Return 17
        If RbConsecutive18.Checked Then Return 18
        If RbConsecutive19.Checked Then Return 19
        If RbConsecutive20.Checked Then Return 20

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

    Private Sub UpdateBonusMultiplierDisplay()
        Dim template = TryCast(LblBonusMultiplier.Tag, String)
        If template Is Nothing Then Return

        Dim count = GetConsecutiveGamesCount()
        Dim bonusText As String

        If count > 16 Then
            bonusText = "1.75x"
        ElseIf count >= 12 Then
            bonusText = "1.5x"
        ElseIf count >= 8 Then
            bonusText = "1.25x"
        ElseIf count >= 5 Then
            bonusText = "1.1x"
        Else
            bonusText = "0x"
        End If

        LblBonusMultiplier.Text = String.Format(template, bonusText)
    End Sub

    Private Sub UpdateConsecutiveGameLabels()
        Dim gamesToPlay = GetConsecutiveGamesCount()
        For index = 0 To _gamePlayLabels.Count - 1
            Dim label = _gamePlayLabels(index)
            label.Visible = index < gamesToPlay AndAlso gamesToPlay > 1
            label.Text = (index + 1).ToString()
            label.BackColor = _gamePlayDefaultBackColor
            label.ForeColor = _gamePlayDefaultForeColor
        Next
    End Sub

    Private Sub ResetGamePlayLabels()
        For index = 0 To _gamePlayLabels.Count - 1
            Dim label = _gamePlayLabels(index)
            label.Text = (index + 1).ToString()
            label.BackColor = _gamePlayDefaultBackColor
            label.ForeColor = _gamePlayDefaultForeColor
        Next
    End Sub

    Private Sub SetGamePlayInProgress(gameIndex As Integer)
        Dim labelIndex = gameIndex - 1
        If labelIndex < 0 OrElse labelIndex >= _gamePlayLabels.Count Then
            Return
        End If

        Dim label = _gamePlayLabels(labelIndex)
        label.BackColor = Color.FromKnownColor(GameInProgressColor)
        label.ForeColor = Color.Black
    End Sub

    Private Sub UpdateGamePlayResult(gameIndex As Integer, payout As Decimal, Optional isFreeGame As Boolean = False, Optional firstBallHit As Boolean = False, Optional lastBallHit As Boolean = False)
        Dim labelIndex = gameIndex - 1
        If labelIndex < 0 OrElse labelIndex >= _gamePlayLabels.Count Then
            Return
        End If

        Dim label = _gamePlayLabels(labelIndex)
        If payout > 0D Then
            label.BackColor = Color.Gold
            label.ForeColor = Color.Black
        ElseIf isFreeGame Then
            label.BackColor = Color.ForestGreen
            label.ForeColor = Color.White
        Else
            label.BackColor = Color.FromKnownColor(GamePlayedColor)
            label.ForeColor = _gamePlayDefaultForeColor
        End If

        If firstBallHit AndAlso lastBallHit Then
            label.Text = ChrW(&H2665) & ChrW(&H2666)
            label.ForeColor = Color.DarkMagenta
        ElseIf firstBallHit Then
            label.Text = ChrW(&H2665)
            label.ForeColor = Color.Crimson
        ElseIf lastBallHit Then
            label.Text = ChrW(&H2666)
            label.ForeColor = Color.RoyalBlue
        End If
    End Sub

    Private Sub ResetConsecutiveSelection()
        RbConsecutive2.Checked = False
        RbConsecutive3.Checked = False
        RbConsecutive4.Checked = False
        RbConsecutive5.Checked = False
        RbConsecutive6.Checked = False
        RbConsecutive7.Checked = False
        RbConsecutive8.Checked = False
        RbConsecutive9.Checked = False
        RbConsecutive10.Checked = False
        RbConsecutive11.Checked = False
        RbConsecutive12.Checked = False
        RbConsecutive13.Checked = False
        RbConsecutive14.Checked = False
        RbConsecutive15.Checked = False
        RbConsecutive16.Checked = False
        RbConsecutive17.Checked = False
        RbConsecutive18.Checked = False
        RbConsecutive19.Checked = False
        RbConsecutive20.Checked = False
        UpdateConsecutiveGameLabels()
        UpdateBonusMultiplierDisplay()
    End Sub

    Private Async Function PlaySingleGameAsync(betAmount As Decimal) As Task(Of (Payout As Decimal, Matched As Integer, FirstLastBonus As Decimal, FirstBallHit As Boolean, LastBallHit As Boolean))
        Dim quadrantMode = _selectedQuadrants.Count > 0
        ResetKenoGridHighlights()
        ResetNumSelectHighlights()
        ResetPowerballDisplay()

        ' Re-apply pick highlights after the reset so selected numbers are visible
        ' before and during each draw. DrawBallsAsync overwrites drawn numbers naturally:
        ' matched picks → RoyalBlue/Gold; unmatched picks stay green; non-pick draws → LightSkyBlue.
        If Not quadrantMode Then
            For Each n In _selectedNumbers
                Dim lbl = FindKenoLabel(n)
                If lbl IsNot Nothing Then UpdateLabelSelectionVisual(lbl, True)
            Next
        End If

        Dim picks = GetRandomPicks(20)
        Dim matchedCount = Await DrawBallsAsync(picks, quadrantMode)
        Dim evalResult = Await EvaluateGameAsync(betAmount, matchedCount, picks, quadrantMode)

        Return (evalResult.Payout, matchedCount, evalResult.FirstLastBonus, evalResult.FirstBallHit, evalResult.LastBallHit)
    End Function

    Private Async Function DrawBallsAsync(picks As List(Of Integer), quadrantMode As Boolean) As Task(Of Integer)
        Dim matchedCount = 0

        For Each pick In picks
            Dim label = FindKenoLabel(pick)
            If label IsNot Nothing Then
                label.BackColor = Color.FromKnownColor(DrawPickColor)
                label.ForeColor = SystemColors.ControlText
            End If

            Dim isMatch = If(quadrantMode, IsPickInQuadrantSelection(pick), _selectedNumbers.Contains(pick))
            If isMatch Then
                If Not quadrantMode Then
                    Dim matchedBox = FindNumSelectTextBox(pick)
                    If matchedBox IsNot Nothing Then
                        matchedBox.BackColor = Color.FromKnownColor(MatchPickColor)
                    End If
                End If

                If label IsNot Nothing Then
                    label.BackColor = Color.FromKnownColor(MatchDrawColor)
                    label.ForeColor = Color.Gold
                    label.Text = GetMatchSymbol()
                End If

                matchedCount += 1
            End If

            Await Task.Delay(GetDrawSpeedMs())
        Next

        Return matchedCount
    End Function

    Private Async Function EvaluateGameAsync(betAmount As Decimal, matchedCount As Integer, picks As List(Of Integer), quadrantMode As Boolean) As Task(Of (Payout As Decimal, FirstLastBonus As Decimal, FirstBallHit As Boolean, LastBallHit As Boolean))
        Dim powerballHit = False
        If ChkPowerball.Checked AndAlso Not quadrantMode Then
            Dim powerballNumber = DrawPowerballNumber(picks)
            powerballHit = _selectedNumbers.Contains(powerballNumber)
            Await ShowPowerballDrawAsync(powerballNumber, powerballHit)
        End If

        Dim payout = CalculatePayout(betAmount, matchedCount, powerballHit)
        Dim firstLastBonus = 0D
        Dim firstBallHit = False
        Dim lastBallHit = False

        If Not quadrantMode AndAlso ChkFirstLastPlay.Checked Then
            Dim firstBall = picks(0)
            Dim lastBall = picks(picks.Count - 1)
            firstBallHit = _selectedNumbers.Contains(firstBall)
            lastBallHit = _selectedNumbers.Contains(lastBall)
            If firstBallHit OrElse lastBallHit Then
                ' Flat dollar bonus — not scaled by bet, multiplier, or Powerball
                firstLastBonus = GetFirstLastBallBonus(_selectedNumbers.Count)
                payout += firstLastBonus
            End If
        End If

        _lastFirstLastBonus = firstLastBonus
        _lastMatchedCount = matchedCount
        UpdateWinningsDisplay(payout)
        UpdateStatus()
        UpdateDrawStats(picks, payout)

        If Not quadrantMode AndAlso _selectedNumbers.Count >= 8 AndAlso matchedCount = _selectedNumbers.Count Then
            Dim jackpotAmount = GetJackpotBalance()
            If jackpotAmount > 0D Then
                payout += jackpotAmount
                UpdateWinningsDisplay(payout)
                ResetJackpot()
                RecordJackpotWon()
                UpdateJackpotDisplay()
                MessageBox.Show(
                    $"JACKPOT WIN!{Environment.NewLine}You matched all {_selectedNumbers.Count} numbers!{Environment.NewLine}Jackpot: {jackpotAmount:C2}",
                    "Progressive Jackpot!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation)
            End If
        End If

        AllTimeSummaryStore.RecordGame(betAmount, payout)
        Return (payout, firstLastBonus, firstBallHit, lastBallHit)
    End Function

    Private Function CalculatePayout(betAmount As Decimal, matchedCount As Integer, Optional powerballHit As Boolean = False) As Decimal
        Dim powerball = If(ChkPowerball.Checked AndAlso powerballHit, 4, 1)
        Return GetGamePayout(matchedCount) * betAmount * _multiplierValue * powerball
    End Function

    Private Function DrawPowerballNumber(regularPicks As List(Of Integer)) As Integer
        Dim regularSet As New HashSet(Of Integer)(regularPicks)
        Dim remaining As New List(Of Integer)(60)
        For n = 1 To 80
            If Not regularSet.Contains(n) Then
                remaining.Add(n)
            End If
        Next
        Return remaining(RandomNumberGenerator.GetInt32(remaining.Count))
    End Function

    Private Async Function ShowPowerballDrawAsync(powerballNumber As Integer, isHit As Boolean) As Task
        Dim label = FindKenoLabel(powerballNumber)
        If label IsNot Nothing Then
            label.BackColor = Color.FromKnownColor(PowerballDrawColor)
            label.ForeColor = Color.White
        End If

        LblPowerballValue.Text = powerballNumber.ToString()
        LblPowerballValue.BackColor = If(isHit, Color.DarkOrange, Color.FromKnownColor(PowerballDrawColor))
        LblPowerballValue.ForeColor = Color.White

        Await Task.Delay(1500)
    End Function

    Private Sub ResetPowerballDisplay()
        LblPowerballValue.Text = "—"
        LblPowerballValue.BackColor = Color.Transparent
        LblPowerballValue.ForeColor = SystemColors.ControlText
    End Sub

    Private Sub ApplyTotalWinnings(totalPayout As Decimal)
        _lastPayout = totalPayout
        UpdateWinningsDisplay(totalPayout)
        UpdateStatus()

        If totalPayout <= 0D Then
            Return
        End If

        Dim balance = GetBankBalance()
        balance += totalPayout

        SaveBankBalance(balance)
        UpdateBankDisplay()
    End Sub

    Private Sub QuadrantLabel_Click(sender As Object, e As EventArgs)
        If _gameState <> GameState.Idle Then
            Return
        End If

        Dim label = TryCast(sender, Label)
        If label Is Nothing Then
            Return
        End If

        Dim quadrant As Integer
        If Not Integer.TryParse(label.Text, quadrant) Then
            Return
        End If

        If _selectedQuadrants.Remove(quadrant) Then
            UpdateQuadrantSelectionVisuals()
            UpdatePlayState()
            UpdateStatus()
            Return
        Else
            If _selectedQuadrants.Count >= 2 Then
                Return
            End If

            _selectedQuadrants.Add(quadrant)
            If Not IsValidQuadrantSelection() Then
                _selectedQuadrants.Remove(quadrant)
                Return
            End If
        End If

        If _selectedQuadrants.Count > 0 Then
            _selectedNumbers.Clear()
            UpdateSelectedNumbers()
            ResetKenoGridHighlights()
        End If

        UpdateQuadrantSelectionVisuals()
        UpdatePlayState()
        UpdateStatus()
    End Sub

    Private Function IsValidQuadrantSelection() As Boolean
        If _selectedQuadrants.Count <= 1 Then
            Return True
        End If

        Dim selection = _selectedQuadrants.OrderBy(Function(value) value).ToArray()
        Dim first = selection(0)
        Dim second = selection(1)

        Dim isTopBottom = (first = 1 AndAlso second = 2) OrElse (first = 3 AndAlso second = 4)
        Dim isLeftRight = (first = 1 AndAlso second = 3) OrElse (first = 2 AndAlso second = 4)

        Return isTopBottom OrElse isLeftRight
    End Function

    Private Sub UpdateQuadrantSelectionVisuals()
        For Each entry In _quadrantLabels
            Dim label = entry.Value
            If _selectedQuadrants.Contains(entry.Key) Then
                label.BackColor = Color.Gold
            Else
                label.BackColor = _quadrantDefaultBackColor
            End If
        Next
    End Sub

    Private Function GetGameModeLabel() As String
        If IsWayTicketActive() Then Return "Way Ticket"
        If _selectedQuadrants.Count > 0 Then
            Dim areaType = GetQuadrantSelectionType()
            If areaType = "TopBottom" Then Return "Top/Bottom"
            If areaType = "LeftRight" Then Return "Left/Right"
            Return "Quadrants"
        End If
        If _isBullseyeActive Then Return "Bullseye"
        Return $"Regular {_selectedNumbers.Count}-spot"
    End Function

    Private Function GetGamePayout(matchedCount As Integer) As Decimal
        If _selectedQuadrants.Count > 0 Then
            Dim areaType = GetQuadrantSelectionType()
            Return GetAreaPayout(areaType, matchedCount)
        End If

        If _isBullseyeActive Then
            Return GetBullseyePayout(matchedCount)
        End If

        Return GetPayout(_selectedNumbers.Count, matchedCount)
    End Function

    Private Function GetQuadrantSelectionType() As String
        If _selectedQuadrants.Count = 1 Then
            Return "Quadrants"
        End If

        Dim selection = _selectedQuadrants.OrderBy(Function(value) value).ToArray()
        Dim first = selection(0)
        Dim second = selection(1)

        If (first = 1 AndAlso second = 2) OrElse (first = 3 AndAlso second = 4) Then
            Return "TopBottom"
        End If

        Return "LeftRight"
    End Function

    Private Function IsPickInQuadrantSelection(number As Integer) As Boolean
        Dim quadrant = GetQuadrant(number)
        Return _selectedQuadrants.Contains(quadrant)
    End Function

    Private Function GetQuadrant(number As Integer) As Integer
        Dim index = number - 1
        Dim row = index \ 10
        Dim col = index Mod 10

        Dim isTop = row < 4
        Dim isLeft = col < 5

        If isTop AndAlso isLeft Then
            Return 1
        End If

        If isTop AndAlso Not isLeft Then
            Return 2
        End If

        If Not isTop AndAlso isLeft Then
            Return 3
        End If

        Return 4
    End Function

    Private Sub UpdateFormIcon()
        Dim iconPath = Path.Combine(Application.StartupPath, "Resources", "keno_2.ico")
        If File.Exists(iconPath) Then
            Icon = New Icon(iconPath)
        End If
    End Sub

    Private Sub ApplyWindowSettings()
        Dim settings = LoadAppSettings()
        If Not settings.HasLocation Then
            Return
        End If

        Dim pt = New Point(settings.LocationX, settings.LocationY)
        If Not IsLocationVisible(pt) Then
            Return
        End If

        StartPosition = FormStartPosition.Manual
        Location = pt
    End Sub

    Private Sub SaveWindowSettings()
        Dim settings = LoadAppSettings()
        settings.HasLocation = True
        settings.LocationX = Location.X
        settings.LocationY = Location.Y
        settings.WindowState = WindowState.ToString()
        settings.DrawSpeedIndex = If(RbDrawSpeedFast.Checked, 2, If(RbDrawSpeedMedium.Checked, 1, 0))

        SaveAppSettings(settings)
    End Sub

    Private Function IsLocationVisible(location As Point) As Boolean
        For Each scr As Screen In Screen.AllScreens
            If scr.WorkingArea.Contains(location) Then
                Return True
            End If
        Next

        Return False
    End Function

    Private Function IsFreeGameBonus(matchedCount As Integer, betAmount As Decimal) As Boolean
        If betAmount < 2D Then Return False
        If _selectedQuadrants.Count > 0 Then Return False
        If matchedCount <> 0 Then Return False
        Dim picks = _selectedNumbers.Count
        Return picks >= 5 AndAlso picks <= 9
    End Function

    Private Sub AwardFreeGameBonus()
        AddFreeGame()
        _sessionFreeGamesEarned += 1
        AllTimeSummaryStore.RecordFreeGameEarned()
        UpdateFreeGamesButton()
        FrmBonusGame.ShowBonus(Me, _selectedNumbers.Count)
    End Sub

    Private Async Sub BtnFreeGames_Click(sender As Object, e As EventArgs) Handles BtnFreeGames.Click
        Try
            Dim hasSelection = _selectedNumbers.Count > 0 OrElse _selectedQuadrants.Count > 0
            If Not hasSelection OrElse GetFreeGames() <= 0 Then Return

            UseFreeGame()
            _gameState = GameState.Playing
            _multiplierValue = 1
            UpdatePlayState()
            _currentTotalBet = 2D
            UpdateWinningsDisplay(0D)
            _lastMatchedCount = 0
            _lastPayout = 0D
            UpdateStatus()

            Const FreeGameBet As Decimal = 2D
            Dim result = Await PlaySingleGameAsync(FreeGameBet)

            If result.Payout > 0D Then
                ShowPayoutSchedulePopup(result.Payout, result.Matched, FreeGameBet)
            End If

            ApplyTotalWinnings(result.Payout)
            AppendGame($"Free Game ({GetGameModeLabel()})", FreeGameBet, result.Matched, result.Payout, firstLastBonus:=result.FirstLastBonus)
            DisableKenoGridSelection()
            _gameState = GameState.Complete
            UpdatePlayState()
        Catch ex As Exception
            LogError(ex, NameOf(BtnFreeGames_Click))
        End Try
    End Sub

    Private Sub ShowPayoutSchedulePopup(payout As Decimal, matchedCount As Integer, betAmount As Decimal)
        Dim gameType As String
        Dim pickedCount As Integer = 0

        If _selectedQuadrants.Count > 0 Then
            gameType = GetQuadrantSelectionType()
        ElseIf _isBullseyeActive Then
            gameType = "Bullseye"
        Else
            gameType = "Regular"
            pickedCount = _selectedNumbers.Count
        End If

        FrmPayoutSchedule.ShowForWin(Me, gameType, pickedCount, matchedCount, payout, betAmount)
    End Sub

    Private Function GetMultiplierCost() As Decimal
        Return If(ChkMultiplierKeno.Checked, 1D, 0D)
    End Function

    Private Function GetPowerballCost() As Decimal
        Return If(ChkPowerball.Checked, 1D, 0D)
    End Function

    Private Function GetFirstLastPlayCost() As Decimal
        Return If(ChkFirstLastPlay.Checked, 1D, 0D)
    End Function

    Private Function GetTotalBet(betAmount As Decimal, gamesToPlay As Integer) As Decimal
        Dim costPerGame = betAmount + GetMultiplierCost() + GetPowerballCost() + GetFirstLastPlayCost()
        If IsWayTicketActive() Then
            Return costPerGame * _wayTicketGroups.Count
        End If
        Return costPerGame * gamesToPlay
    End Function

    Private Function DrawMultiplierValue() As Integer
        ' Weighted draw: 1×=45%, 2×=30%, 3×=13%, 5×=9%, 10×=3%
        Dim roll = RandomNumberGenerator.GetInt32(100)

        If roll < 45 Then Return 1
        If roll < 75 Then Return 2
        If roll < 88 Then Return 3
        If roll < 97 Then Return 5

        Return 10
    End Function

    Private Sub UpdateMultiplierDisplay(multiplier As Integer)
        Dim template = TryCast(ChkMultiplierKeno.Tag, String)
        If template Is Nothing Then Return
        ChkMultiplierKeno.Text = String.Format(template, $"{multiplier}x")
    End Sub

    Private Function IsWayTicketActive() As Boolean
        Return ChkWayTicket.Checked AndAlso _wayTicketGroups IsNot Nothing AndAlso _wayTicketGroups.Count >= 2
    End Function

    Private Sub UpdateWayTicketSummary()
        If Not IsWayTicketActive() Then Return
        Dim k = _wayTicketGroups.Count
        Dim m = _wayTicketGroups(0).Count
        Dim bet = GetSelectedBetAmount()
        Dim kingMarker = If(_wayTicketKingNumber > 0, " ♛", "")
        If bet > 0D Then
            LblWayTicketSummary.Text = $"{k} sub-tickets ({m}-spot{kingMarker}) - {(k * bet):C2} total"
        Else
            LblWayTicketSummary.Text = $"{k} sub-tickets ({m}-spot{kingMarker})"
        End If
    End Sub

    Private Async Function PlayWayTicketAsync(betAmount As Decimal) As Task(Of Decimal)
        Dim totalPayout = 0D
        Dim originalNumbers = _selectedNumbers.ToList()
        Dim groupCount = _wayTicketGroups.Count

        For groupIndex = 0 To groupCount - 1
            Dim group = _wayTicketGroups(groupIndex)

            ' Swap in this group's numbers so PlaySingleGameAsync sees them
            _selectedNumbers.Clear()
            For Each n In group
                _selectedNumbers.Add(n)
            Next
            UpdateSelectedNumbers()
            LblWayTicketSummary.Text = $"Sub-ticket {groupIndex + 1} of {groupCount}..."

            Dim result = Await PlaySingleGameAsync(betAmount)
            totalPayout += result.Payout

            If result.Payout > 0D Then
                ShowPayoutSchedulePopup(result.Payout, result.Matched, betAmount)
            ElseIf IsFreeGameBonus(result.Matched, betAmount) Then
                AwardFreeGameBonus()
            ElseIf groupIndex < groupCount - 1 Then
                Await Task.Delay(GetBetweenGameDelayMs())
            End If
        Next

        ' Restore the original selection and grid highlights
        _selectedNumbers.Clear()
        For Each n In originalNumbers
            _selectedNumbers.Add(n)
        Next
        UpdateSelectedNumbers()
        ResetKenoGridHighlights()
        For Each n In originalNumbers
            Dim lbl = FindKenoLabel(n)
            If lbl IsNot Nothing Then
                UpdateLabelSelectionVisual(lbl, True)
            End If
        Next

        Return totalPayout
    End Function

    Private Function GetBetweenGameDelayMs() As Integer
        If RbDrawSpeedFast.Checked Then
            Return 2000
        End If

        If RbDrawSpeedMedium.Checked Then
            Return 3500
        End If

        Return 5000
    End Function

    Private Function GetRandomPicks(count As Integer) As List(Of Integer)
        Dim numbers As New List(Of Integer)(80)
        For number = 1 To 80
            numbers.Add(number)
        Next

        For index = numbers.Count - 1 To 1 Step -1
            Dim swapIndex = RandomNumberGenerator.GetInt32(index + 1)
            Dim temp = numbers(index)
            numbers(index) = numbers(swapIndex)
            numbers(swapIndex) = temp
        Next

        Dim picks As New List(Of Integer)(count)
        For index = 0 To Math.Min(count, numbers.Count) - 1
            picks.Add(numbers(index))
        Next

        Return picks
    End Function

    Private Sub UpdateJackpotDisplay()
        Dim jackpot = GetJackpotBalance()
        Dim template = StsProgressiveJackpot.Tag?.ToString()
        If Not String.IsNullOrEmpty(template) Then
            StsProgressiveJackpot.Text = template.Replace("(0)", jackpot.ToString("N2"))
        Else
            StsProgressiveJackpot.Text = $"Progressive Jackpot: {jackpot:C2}"
        End If
    End Sub

    Private Sub UpdateStreakDisplay(Optional stats As DrawStatsStore.DrawStats = Nothing)
        Try
            If stats Is Nothing Then
                stats = LoadStats()
            End If

            If stats.CurrentWinStreak > 0 Then
                StsCurrentStreak.Text = $"Win Streak: {stats.CurrentWinStreak}"
                StsCurrentStreak.ForeColor = Color.ForestGreen
            ElseIf stats.CurrentLossStreak > 0 Then
                StsCurrentStreak.Text = $"Loss Streak: {stats.CurrentLossStreak}"
                StsCurrentStreak.ForeColor = Color.Firebrick
            Else
                StsCurrentStreak.Text = "Streak: —"
                StsCurrentStreak.ForeColor = SystemColors.ControlText
            End If

            StsBestStreak.Text = $"Best: {stats.BestWinStreak}W"
            StsBestStreak.ForeColor = SystemColors.ControlText
        Catch ex As Exception
            LogError(ex, NameOf(UpdateStreakDisplay))
        End Try
    End Sub

    ' -------------------------------------------------------------------------
    ' Hot/cold click-to-select (#14)
    ' -------------------------------------------------------------------------
    Private Sub InitializeHotColdLabels()
        TableLayoutPanel1.Enabled = True

        Dim hotLabels As Label() = {LblHot1, LblHot2, LblHot3, LblHot4, LblHot5}
        Dim coldLabels As Label() = {LblCold1, LblCold2, LblCold3, LblCold4, LblCold5}

        For Each lbl In hotLabels.Concat(coldLabels)
            lbl.Cursor = Cursors.Hand
            AddHandler lbl.Click, AddressOf HotColdLabel_Click
        Next

        _toolTip.SetToolTip(TableLayoutPanel1,
            "Click any Hot or Cold number to toggle its selection on the grid.")
    End Sub

    Private Sub HotColdLabel_Click(sender As Object, e As EventArgs)
        If _gameState <> GameState.Idle Then Return
        Dim lbl = TryCast(sender, Label)
        If lbl Is Nothing OrElse String.IsNullOrEmpty(lbl.Text) Then Return

        Dim number As Integer
        If Not Integer.TryParse(lbl.Text, number) Then Return

        Dim kenoLabel = FindKenoLabel(number)
        If kenoLabel Is Nothing Then Return

        If _selectedNumbers.Remove(number) Then
            UpdateLabelSelectionVisual(kenoLabel, False)
        ElseIf _selectedNumbers.Count < 20 Then
            _selectedNumbers.Add(number)
            UpdateLabelSelectionVisual(kenoLabel, True)
        Else
            Return
        End If

        _isBullseyeActive = False
        UpdateBullseyeButtonVisual()
        UpdateSelectedNumbers()
        UpdatePlayState()
        UpdateStatus()
    End Sub

    Private Sub StsSummary_Click(sender As Object, e As EventArgs) Handles StsSummary.Click
        FrmAllTimeSummary.ShowHistory(Me)
    End Sub

    Private Sub ShowSessionSummary()
        Dim netPL = GetBankBalance() - _sessionStartBalance
        FrmSessionSummary.ShowSummary(Me, netPL, _sessionGamesPlayed, _sessionWins, _sessionBestPayout, _sessionFreeGamesEarned, _sessionTotalWagered)
    End Sub

    Private Function GetMatchSymbol() As String
        If _isBullseyeActive Then Return ChrW(&H25CE)     ' ◎  bullseye
        If IsWayTicketActive() Then Return ChrW(&H2666)   ' ♦  diamond
        Return ChrW(&H2713)                               ' ✓  checkmark
    End Function

End Class