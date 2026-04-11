// Last Edit: Apr 11, 2026 10:04 - Internal board resets no longer clear wager/games/side-bet options.
using Keno.Core;

namespace Keno.Android;

public partial class MainPage : ContentPage
{
    // ── Color palette (mirrors WPF KenoDefaultBrush / selection colours) ──────
    private static readonly Color CellDefault = Color.FromArgb("#C0FFFF");

    private static readonly Color CellSelected = Color.FromArgb("#00BB00");
    private static readonly Color CellDrawn = Color.FromArgb("#FF8C00");
    private static readonly Color CellMatched = Colors.Gold;
    private static readonly Color WagerDefault = Color.FromArgb("#4A90D9");
    private static readonly Color WagerActive = Color.FromArgb("#FF8C00");
    private static readonly Color PicksCellEmpty = Color.FromArgb("#F0F0F0");
    private static readonly Color DrawnCellEmpty = Color.FromArgb("#E0E0E0");
    private static readonly Color CellDrawnBoard = Color.FromArgb("#FFD090"); // drawn-not-matched on main board

    // ── Side-bet / favorites palette ──────────────────────────────────────────
    private static readonly Color SideBetOff = Color.FromArgb("#607D8B");

    private static readonly Color SideBetMultiplierOn = Color.FromArgb("#9C27B0");
    private static readonly Color SideBetPowerballOn = Color.FromArgb("#E53935");
    private static readonly Color SideBetFirstLastOn = Color.FromArgb("#EF6C00");
    private static readonly Color FavEmpty = Color.FromArgb("#B0BEC5");
    private static readonly Color FavOccupied = Color.FromArgb("#1976D2");

    // ── Speed pill palette ────────────────────────────────────────────────────
    private static readonly Color SpeedOff = Color.FromArgb("#90A4AE");

    private static readonly Color SpeedOn = Color.FromArgb("#2196A6");

    // ── Wager presets
    private static readonly decimal[] WagerPresets =
        [1m, 2m, 3m, 5m, 10m, 15m, 20m, 25m, 30m, 40m, 50m, 100m, 150m, 200m];

    // ── Persistence preference keys ────────────────────────────────────────
    private const string PrefBank = "bank_balance";

    private const string PrefWager = "setting_wager";
    private const string PrefMultiplier = "setting_multiplier";
    private const string PrefPowerball = "setting_powerball";
    private const string PrefFirstLast = "setting_firstlast";
    private const string PrefAutoPick = "setting_autopick";
    private const string PrefSpeed = "setting_speed";
    private const string PrefGames = "setting_games";

    // ── UI element references ─────────────────────────────────────────────────
    private readonly Label[] _numberLabels = new Label[81]; // 1-indexed, cells 1–80

    private readonly Label[] _picksLabels = new Label[21]; // 1-indexed, slots 1–20
    private readonly Label[] _drawnLabels = new Label[21]; // 1-indexed, slots 1–20
    private readonly HashSet<int> _selectedNumbers = [];
    private decimal _currentWager = 1m;
    private Button? _activeWagerBtn;
    private Button? _customWagerBtn;
    private decimal _bankBalance = 10_000m;
    private int _currentWinStreak = 0;
    private int _currentLossStreak = 0;
    private int[] _lastPickedNumbers = [];
    private readonly List<GameRecord> _sessionHistory = [];

    // ── Quick Pick ────────────────────────────────────────────────────────────
    private int _autoPickCount = 5;

    // ── Consecutive games ─────────────────────────────────────────────────────
    private int _consecutiveGames = 1;

    // ── Play speed ────────────────────────────────────────────────────────────
    private int _drawDelayMs = 200; // Fast by default

    private Button? _activeSpeedBtn;

    // ── Side bets ─────────────────────────────────────────────────────────────
    private bool _useMultiplier;

    private bool _usePowerball;
    private bool _useFirstLast;
    private int _lastMultiplierValue = 1;
    private int _lastPowerballNumber;
    private bool _isMenuBusy;

    public MainPage()
    {
        InitializeComponent();
        BuildNumberGrid();
        BuildPicksGrid();
        BuildDrawnGrid();
        BuildWagerButtons();
        LoadSettings();         // restores bank, wager, speed, side bets, games from Preferences
        UpdateBonusDisplay();
        UpdateStatus();
        UpdateFavoriteButtons();
        UpdateSideBetInfo();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Enables or disables all interactive controls while a game is running.</summary>
    private void SetPlayUiEnabled(bool enabled)
    {
        foreach (Button btn in new[] { BtnPlay, BtnClear, BtnAutoPick, BtnAutoPickMinus, BtnAutoPickPlus,
            BtnGamesMinus, BtnGamesPlus,
            BtnSideBetMultiplier, BtnSideBetPowerball, BtnSideBetFirstLast,
            BtnFav0, BtnFav1, BtnFav2,
            BtnSpeedSlow, BtnSpeedMed, BtnSpeedFast, BtnSpeedSS })
        {
            btn.IsEnabled = enabled;
        }
        BtnReplay.IsEnabled = enabled && _lastPickedNumbers.Length > 0;
    }

    /// <summary>Consecutive-games bonus multiplier (mirrors WPF GetConsecutiveBonus).</summary>
    private decimal GetConsecutiveBonus() => _consecutiveGames switch
    {
        >= 17 => 1.75m,
        >= 12 => 1.50m,
        >= 8 => 1.25m,
        >= 5 => 1.10m,
        _ => 1.00m
    };

    /// <summary>Updates the bonus label next to the games stepper.</summary>
    private void UpdateBonusDisplay()
    {
        decimal bonus = GetConsecutiveBonus();
        LblBonusDisplay.Text = bonus > 1m ? $"Bonus ×{bonus:0.##}" : string.Empty;
    }

    /// <summary>
    /// Clamps _consecutiveGames so that EffectiveWager × games ≤ bank balance.
    /// If the balance cannot afford even one game the count stays at 1.
    /// </summary>
    private void ClampGamesToBalance()
    {
        if (EffectiveWager <= 0m || _bankBalance <= 0m) return;
        int max = Math.Max(1, (int)Math.Floor(_bankBalance / EffectiveWager));
        if (_consecutiveGames > max)
        {
            _consecutiveGames = max;
            LblGamesCount.Text = _consecutiveGames.ToString();
            UpdateBonusDisplay();
        }
    }

    /// <summary>Restores all persisted state (bank, wager, side bets, speed, quick-pick, games) from Preferences.</summary>
    private void LoadSettings()
    {
        // Bank balance
        _bankBalance = Math.Max(0m, (decimal)Preferences.Default.Get(PrefBank, 10_000.0));

        // Wager — match a preset button or highlight the CUSTOM button
        _currentWager = Math.Clamp((decimal)Preferences.Default.Get(PrefWager, 1.0), 0.01m, 9_999.99m);
        bool presetFound = false;
        foreach (IView child in WagerButtonsLayout)
        {
            if (child is not Button btn) continue;
            btn.BackgroundColor = WagerDefault;
            if (!presetFound && btn.CommandParameter is decimal amount && amount == _currentWager)
            {
                _activeWagerBtn = btn;
                btn.BackgroundColor = WagerActive;
                presetFound = true;
            }
        }
        if (!presetFound && _customWagerBtn is not null)
        {
            _customWagerBtn.Text = $"${_currentWager:0.##}";
            _customWagerBtn.BackgroundColor = WagerActive;
            _activeWagerBtn = _customWagerBtn;
        }

        // Side bets
        _useMultiplier = Preferences.Default.Get(PrefMultiplier, false);
        _usePowerball = Preferences.Default.Get(PrefPowerball, false);
        _useFirstLast = Preferences.Default.Get(PrefFirstLast, false);
        BtnSideBetMultiplier.BackgroundColor = _useMultiplier ? SideBetMultiplierOn : SideBetOff;
        BtnSideBetPowerball.BackgroundColor = _usePowerball ? SideBetPowerballOn : SideBetOff;
        BtnSideBetFirstLast.BackgroundColor = _useFirstLast ? SideBetFirstLastOn : SideBetOff;

        // Quick Pick count
        _autoPickCount = Math.Clamp(Preferences.Default.Get(PrefAutoPick, 5), 1, 20);
        LblAutoPickCount.Text = _autoPickCount.ToString();

        // Draw speed — map stored ms value to the correct speed button
        _drawDelayMs = Preferences.Default.Get(PrefSpeed, 200);
        _activeSpeedBtn = _drawDelayMs switch
        {
            1000 => BtnSpeedSlow,
            500 => BtnSpeedMed,
            0 => BtnSpeedSS,
            _ => BtnSpeedFast
        };
        _activeSpeedBtn.BackgroundColor = SpeedOn;

        // Consecutive games
        _consecutiveGames = Math.Clamp(Preferences.Default.Get(PrefGames, 1), 1, 20);
        LblGamesCount.Text = _consecutiveGames.ToString();
    }

    // ── Board construction

    /// <summary>Creates the 8×10 number board (cells 1–80) and injects it into NumberGridContainer.</summary>
    private void BuildNumberGrid()
    {
        var grid = new Grid
        {
            BackgroundColor = Color.FromArgb("#888888"), // 1dp gaps show as borders
            RowSpacing = 1,
            ColumnSpacing = 1,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        for (int r = 0; r < 8; r++)
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        for (int c = 0; c < 10; c++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        for (int n = 1; n <= 80; n++)
        {
            int row = (n - 1) / 10;
            int col = (n - 1) % 10;

            var lbl = new Label
            {
                Text = n.ToString(),
                BackgroundColor = CellDefault,
                TextColor = Colors.Black,
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            var tap = new TapGestureRecognizer { CommandParameter = n };
            tap.Tapped += NumberLabel_Tapped;
            lbl.GestureRecognizers.Add(tap);

            _numberLabels[n] = lbl;
            grid.Add(lbl, col, row);
        }

        NumberGridContainer.Content = grid;
    }

    /// <summary>Creates the 2×10 picks display (selected numbers, slots 1–20) and injects it into PicksGridContainer.</summary>
    private void BuildPicksGrid()
    {
        var grid = new Grid
        {
            BackgroundColor = Color.FromArgb("#888888"),
            RowSpacing = 1,
            ColumnSpacing = 1,
            HeightRequest = 57,
            HorizontalOptions = LayoutOptions.Fill
        };

        grid.RowDefinitions.Add(new RowDefinition(new GridLength(27)));
        grid.RowDefinitions.Add(new RowDefinition(new GridLength(27)));
        for (int c = 0; c < 10; c++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        for (int i = 1; i <= 20; i++)
        {
            var lbl = new Label
            {
                BackgroundColor = PicksCellEmpty,
                TextColor = Colors.Black,
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            _picksLabels[i] = lbl;
            grid.Add(lbl, (i - 1) % 10, (i - 1) / 10);
        }

        PicksGridContainer.Content = grid;
    }

    /// <summary>Creates the 2×10 drawn-numbers display (balls 1–20 in draw order) and injects it into DrawnGridContainer.</summary>
    private void BuildDrawnGrid()
    {
        var grid = new Grid
        {
            BackgroundColor = Color.FromArgb("#888888"),
            RowSpacing = 1,
            ColumnSpacing = 1,
            HeightRequest = 57,
            HorizontalOptions = LayoutOptions.Fill
        };

        grid.RowDefinitions.Add(new RowDefinition(new GridLength(27)));
        grid.RowDefinitions.Add(new RowDefinition(new GridLength(27)));
        for (int c = 0; c < 10; c++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        for (int i = 1; i <= 20; i++)
        {
            var lbl = new Label
            {
                BackgroundColor = DrawnCellEmpty,
                TextColor = Colors.Black,
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            _drawnLabels[i] = lbl;
            grid.Add(lbl, (i - 1) % 10, (i - 1) / 10);
        }

        DrawnGridContainer.Content = grid;
    }

    /// <summary>Creates preset wager Buttons and adds them to the horizontal wager selector strip.</summary>
    private void BuildWagerButtons()
    {
        foreach (decimal amount in WagerPresets)
        {
            var btn = new Button
            {
                Text = $"${amount:0.##}",
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = WagerDefault,
                TextColor = Colors.White,
                CornerRadius = 4,
                Padding = new Thickness(8, 0),
                HeightRequest = 28,
                MinimumHeightRequest = 28,
                CommandParameter = amount
            };
            btn.Clicked += WagerButton_Clicked;
            WagerButtonsLayout.Add(btn);
        }

        // CUSTOM wager button — always at the end of the strip
        _customWagerBtn = new Button
        {
            Text = "CUSTOM",
            FontSize = 10,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = WagerDefault,
            TextColor = Colors.White,
            CornerRadius = 4,
            Padding = new Thickness(8, 0),
            HeightRequest = 28,
            MinimumHeightRequest = 28
        };
        _customWagerBtn.Clicked += BtnCustomWager_Clicked;
        WagerButtonsLayout.Add(_customWagerBtn);

        // Highlight the default $1 wager; LoadSettings() will override this if a different wager was persisted
        foreach (IView child in WagerButtonsLayout)
        {
            if (child is Button first && first.CommandParameter is decimal)
            {
                _activeWagerBtn = first;
                first.BackgroundColor = WagerActive;
                break;
            }
        }
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void NumberLabel_Tapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not int number) return;

        var label = _numberLabels[number];

        if (_selectedNumbers.Remove(number))
        {
            label.BackgroundColor = CellDefault;
        }
        else if (_selectedNumbers.Count < 20)
        {
            _selectedNumbers.Add(number);
            label.BackgroundColor = CellSelected;
        }

        UpdatePicksDisplay();
        UpdateStatus();
    }

    private void WagerButton_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is not decimal amount) return;

        _currentWager = amount;

        _activeWagerBtn?.BackgroundColor = WagerDefault;

        _activeWagerBtn = btn;
        btn.BackgroundColor = WagerActive;

        ClampGamesToBalance();
        UpdateBonusDisplay();
        UpdateSideBetInfo();
        Preferences.Default.Set(PrefWager, (double)_currentWager);
        UpdateStatus();
    }

    private async void BtnPlay_Clicked(object? sender, EventArgs e)
    {
        if (_selectedNumbers.Count == 0) return;
        await PlayGameAsync();
    }

    private async void BtnReplay_Clicked(object? sender, EventArgs e)
    {
        if (_lastPickedNumbers.Length == 0) return;

        // Restore board to the last set of picks, then play immediately.
        ResetRoundState();

        foreach (int n in _lastPickedNumbers)
        {
            _selectedNumbers.Add(n);
            _numberLabels[n].BackgroundColor = CellSelected;
        }
        UpdatePicksDisplay();
        UpdateStatus();

        await PlayGameAsync();
    }

    private async Task PlayGameAsync()
    {
        int gamesToPlay = _consecutiveGames;
        decimal wagerPerGame = EffectiveWager;
        decimal totalCost = wagerPerGame * gamesToPlay;

        // Auto-clamp in case wager/side-bets changed since last clamp
        if (totalCost > _bankBalance)
        {
            gamesToPlay = Math.Max(1, (int)Math.Floor(_bankBalance / wagerPerGame));
            totalCost = wagerPerGame * gamesToPlay;
        }

        if (_bankBalance < wagerPerGame)
        {
            await DisplayAlertAsync("Insufficient Funds", "Your balance is too low for this wager.", "OK");
            return;
        }

        SetPlayUiEnabled(false);
        _lastPickedNumbers = [.. _selectedNumbers];

        // Deduct total cost up front
        _bankBalance -= totalCost;
        UpdateStatus();

        decimal totalPayout = 0m;
        decimal totalFirstLastBonus = 0m;
        int lastMatches = 0;

        try
        {
            for (int g = 0; g < gamesToPlay; g++)
            {
                // Reset board & drawn strip for this game
                foreach (int sel in _selectedNumbers)
                    _numberLabels[sel].BackgroundColor = CellSelected;
                for (int i = 1; i <= 20; i++)
                {
                    _drawnLabels[i].Text = string.Empty;
                    _drawnLabels[i].BackgroundColor = DrawnCellEmpty;
                }

                List<int> drawn = DrawNumbers();

                // Reveal balls one by one
                for (int i = 0; i < 20; i++)
                {
                    int ball = drawn[i];
                    bool matched = _selectedNumbers.Contains(ball);
                    _drawnLabels[i + 1].Text = ball.ToString();
                    _drawnLabels[i + 1].BackgroundColor = matched ? CellMatched : CellDrawn;
                    _numberLabels[ball].BackgroundColor = matched ? CellMatched : CellDrawnBoard;
                    if (_drawDelayMs > 0) await Task.Delay(_drawDelayMs);
                }

                int matches = drawn.Count(n => _selectedNumbers.Contains(n));
                lastMatches = matches;

                decimal gamePayout = KenoPayouts.GetPayout(_selectedNumbers.Count, matches) * _currentWager;

                // Multiplier: scales base payout only
                if (_useMultiplier)
                {
                    _lastMultiplierValue = DrawMultiplier();
                    gamePayout *= _lastMultiplierValue;
                }

                // Powerball: ×4 if pick list contains the drawn ball
                if (_usePowerball)
                {
                    _lastPowerballNumber = Random.Shared.Next(1, 81);
                    if (_selectedNumbers.Contains(_lastPowerballNumber))
                        gamePayout *= 4;
                }

                // First/Last: flat bonus — NOT scaled by consecutive bonus
                decimal firstLastBonus = 0m;
                if (_useFirstLast &&
                    (_selectedNumbers.Contains(drawn[0]) || _selectedNumbers.Contains(drawn[19])))
                {
                    firstLastBonus = KenoPayouts.GetFirstLastBallBonus(_selectedNumbers.Count);
                    gamePayout += firstLastBonus;
                }

                totalPayout += gamePayout;
                totalFirstLastBonus += firstLastBonus;

                bool isWin = gamePayout > 0m;
                LblMatches.Text = $"Matches: {matches}";
                LblPayout.Text = $"Running: {totalPayout:C2}";
                UpdateSideBetInfo();

                _sessionHistory.Add(new GameRecord(DateTime.Now, _selectedNumbers.Count, matches, wagerPerGame, gamePayout));

                Preferences.Default.Set("at_games", Preferences.Default.Get("at_games", 0) + 1);
                Preferences.Default.Set("at_wins", Preferences.Default.Get("at_wins", 0) + (isWin ? 1 : 0));
                Preferences.Default.Set("at_wagered", Preferences.Default.Get("at_wagered", 0.0) + (double)wagerPerGame);
                Preferences.Default.Set("at_won", Preferences.Default.Get("at_won", 0.0) + (double)gamePayout);
                Preferences.Default.Set("at_best_win", Math.Max(Preferences.Default.Get("at_best_win", 0.0), (double)gamePayout));

                // Inter-game pause (skip after last game)
                if (g < gamesToPlay - 1 && _drawDelayMs > 0)
                    await Task.Delay(600);
            }
        }
        finally
        {
            // Apply consecutive bonus — First/Last flat bonuses are excluded from scaling
            decimal bonus = GetConsecutiveBonus();
            decimal bonusedPayout = (totalPayout - totalFirstLastBonus) * bonus + totalFirstLastBonus;

            _bankBalance += bonusedPayout;

            if (bonusedPayout > 0m) { _currentWinStreak++; _currentLossStreak = 0; }
            else { _currentLossStreak++; _currentWinStreak = 0; }

            LblMatches.Text = $"Matches: {lastMatches}";
            LblPayout.Text = $"Payout: {bonusedPayout:C2}";
            UpdateStreak();
            UpdateStatus();
            SetPlayUiEnabled(true);
        }
    }

    private async void BtnMenu_Clicked(object? sender, EventArgs e)
    {
        if (_isMenuBusy) return;

        try
        {
            _isMenuBusy = true;
            BtnMenu.IsEnabled = false;

            string? action = await DisplayActionSheetAsync("Menu", "Cancel", null,
                "Game History", "Payout Schedule", "How to Play");

            ContentPage? page = action switch
            {
                "Game History" => new HistoryPage(_sessionHistory),
                "Payout Schedule" => new PayoutSchedulePage(),
                "How to Play" => new HelpPage(),
                _ => null
            };

            if (page is not null)
            {
                // Let the action-sheet dialog fully close before pushing modal content.
                await Task.Delay(120);
                await Navigation.PushModalAsync(page);
            }
        }
        catch (InvalidOperationException ex)
        {
            await DisplayAlertAsync("Menu", ex.Message, "OK");
        }
        finally
        {
            BtnMenu.IsEnabled = true;
            _isMenuBusy = false;
        }
    }

    private void BtnClear_Clicked(object? sender, EventArgs e)
    {
        ResetRoundState();
        ResetWagerToDefault();
        ResetGameSettingsToDefault();

        UpdateSideBetInfo();
        UpdateStatus();
    }

    private void ResetRoundState()
    {
        // Reset all 80 cells (drawn-but-not-picked cells also need clearing)
        for (int n = 1; n <= 80; n++)
            _numberLabels[n].BackgroundColor = CellDefault;
        _selectedNumbers.Clear();

        for (int i = 1; i <= 20; i++)
        {
            _picksLabels[i].Text = string.Empty;
            _picksLabels[i].BackgroundColor = PicksCellEmpty;
            _drawnLabels[i].Text = string.Empty;
            _drawnLabels[i].BackgroundColor = DrawnCellEmpty;
        }

        LblMatches.Text = "Matches: —";
        LblPayout.Text = "Payout: $0.00";
        LblStreak.Text = "Streak: —";
        _lastMultiplierValue = 1;
        _lastPowerballNumber = 0;
    }

    private void ResetWagerToDefault()
    {
        _currentWager = 1m;
        _activeWagerBtn?.BackgroundColor = WagerDefault;

        if (_customWagerBtn is not null)
        {
            _customWagerBtn.Text = "CUSTOM";
            _customWagerBtn.BackgroundColor = WagerDefault;
        }

        _activeWagerBtn = null;
        foreach (IView child in WagerButtonsLayout)
        {
            if (child is not Button btn || btn.CommandParameter is not decimal amount || amount != 1m)
                continue;

            _activeWagerBtn = btn;
            btn.BackgroundColor = WagerActive;
            break;
        }

        Preferences.Default.Set(PrefWager, (double)_currentWager);
    }

    private void ResetGameSettingsToDefault()
    {
        _consecutiveGames = 1;
        LblGamesCount.Text = _consecutiveGames.ToString();
        UpdateBonusDisplay();
        Preferences.Default.Set(PrefGames, _consecutiveGames);

        _useMultiplier = false;
        _usePowerball = false;
        _useFirstLast = false;
        BtnSideBetMultiplier.BackgroundColor = SideBetOff;
        BtnSideBetPowerball.BackgroundColor = SideBetOff;
        BtnSideBetFirstLast.BackgroundColor = SideBetOff;
        Preferences.Default.Set(PrefMultiplier, _useMultiplier);
        Preferences.Default.Set(PrefPowerball, _usePowerball);
        Preferences.Default.Set(PrefFirstLast, _useFirstLast);
    }

    // ── Display helpers

    private void UpdatePicksDisplay()
    {
        var sorted = _selectedNumbers.Order().ToList();
        for (int i = 1; i <= 20; i++)
            _picksLabels[i].Text = i <= sorted.Count ? sorted[i - 1].ToString() : string.Empty;
    }

    private void UpdateStatus()
    {
        LblPicks.Text = _selectedNumbers.Count.ToString();
        decimal totalWager = EffectiveWager * _consecutiveGames;
        LblWagerTotal.Text = _consecutiveGames > 1 ? $"{totalWager:C2}" : $"{EffectiveWager:C2}";
        LblBank.Text = $"{_bankBalance:C2}";
        BtnReplenish.IsVisible = _bankBalance <= 0m;
        Preferences.Default.Set(PrefBank, (double)_bankBalance);
    }

    private static List<int> DrawNumbers()
    {
        var pool = Enumerable.Range(1, 80).ToList();
        var drawn = new List<int>(20);
        for (int i = 0; i < 20; i++)
        {
            int idx = Random.Shared.Next(pool.Count);
            drawn.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return drawn;
    }

    private void UpdateStreak()
    {
        LblStreak.Text = _currentWinStreak > 0 ? $"Win: {_currentWinStreak}" :
                         _currentLossStreak > 0 ? $"Loss: {_currentLossStreak}" :
                         "Streak: —";
    }

    // ── Quick Pick ────────────────────────────────────────────────────────────

    private void BtnAutoPickMinus_Clicked(object? sender, EventArgs e)
    {
        if (_autoPickCount > 1) _autoPickCount--;
        LblAutoPickCount.Text = _autoPickCount.ToString();
        Preferences.Default.Set(PrefAutoPick, _autoPickCount);
    }

    private void BtnAutoPickPlus_Clicked(object? sender, EventArgs e)
    {
        if (_autoPickCount < 20) _autoPickCount++;
        LblAutoPickCount.Text = _autoPickCount.ToString();
        Preferences.Default.Set(PrefAutoPick, _autoPickCount);
    }

    private void BtnAutoPick_Clicked(object? sender, EventArgs e)
    {
        for (int n = 1; n <= 80; n++)
            _numberLabels[n].BackgroundColor = CellDefault;
        _selectedNumbers.Clear();

        for (int i = 1; i <= 20; i++)
        {
            _picksLabels[i].Text = string.Empty;
            _picksLabels[i].BackgroundColor = PicksCellEmpty;
            _drawnLabels[i].Text = string.Empty;
            _drawnLabels[i].BackgroundColor = DrawnCellEmpty;
        }

        var pool = Enumerable.Range(1, 80).ToList();
        for (int i = 0; i < _autoPickCount; i++)
        {
            int idx = Random.Shared.Next(pool.Count);
            int num = pool[idx];
            pool.RemoveAt(idx);
            _selectedNumbers.Add(num);
            _numberLabels[num].BackgroundColor = CellSelected;
        }

        UpdatePicksDisplay();
        UpdateStatus();
    }

    // ── Side bets ─────────────────────────────────────────────────────────────

    private void BtnSideBetMultiplier_Clicked(object? sender, EventArgs e)
    {
        _useMultiplier = !_useMultiplier;
        BtnSideBetMultiplier.BackgroundColor = _useMultiplier ? SideBetMultiplierOn : SideBetOff;
        UpdateSideBetInfo();
        ClampGamesToBalance();
        Preferences.Default.Set(PrefMultiplier, _useMultiplier);
        UpdateStatus();
    }

    private void BtnSideBetPowerball_Clicked(object? sender, EventArgs e)
    {
        _usePowerball = !_usePowerball;
        if (!_usePowerball) _lastPowerballNumber = 0;
        BtnSideBetPowerball.BackgroundColor = _usePowerball ? SideBetPowerballOn : SideBetOff;
        UpdateSideBetInfo();
        Preferences.Default.Set(PrefPowerball, _usePowerball);
    }

    private void BtnSideBetFirstLast_Clicked(object? sender, EventArgs e)
    {
        _useFirstLast = !_useFirstLast;
        BtnSideBetFirstLast.BackgroundColor = _useFirstLast ? SideBetFirstLastOn : SideBetOff;
        UpdateSideBetInfo();
        ClampGamesToBalance();
        Preferences.Default.Set(PrefFirstLast, _useFirstLast);
        UpdateStatus();
    }

    /// <summary>Updates the three info labels below the side-bet pills.</summary>
    private void UpdateSideBetInfo()
    {
        decimal multiplierFee = MultiplierSideBetFee;
        LblMultiplierInfo.Text = _useMultiplier
            ? (_lastMultiplierValue > 1 ? $"+{multiplierFee:C2} · ×{_lastMultiplierValue}" : $"+{multiplierFee:C2} · ×1")
            : $"+{multiplierFee:C2} · ×1";
        LblMultiplierInfo.TextColor = _useMultiplier ? SideBetMultiplierOn : SideBetOff;

        LblPowerballInfo.Text = (_usePowerball && _lastPowerballNumber > 0)
            ? $"Ball: {_lastPowerballNumber}"
            : "×4 if match";
        LblPowerballInfo.TextColor = _usePowerball ? SideBetPowerballOn : SideBetOff;

        LblFirstLastInfo.TextColor = _useFirstLast ? SideBetFirstLastOn : SideBetOff;
    }

    /// <summary>Weighted multiplier draw: ×1=45%, ×2=30%, ×3=13%, ×5=9%, ×10=3%.</summary>
    private static int DrawMultiplier()
    {
        int roll = Random.Shared.Next(100);
        if (roll < 45) return 1;
        if (roll < 75) return 2;
        if (roll < 88) return 3;
        if (roll < 97) return 5;
        return 10;
    }

    // ── Favorites (3 slots via Preferences) ──────────────────────────────────

    /// <summary>Multiplier side-bet fee per game: max($1, 3% of base wager), rounded to cents.</summary>
    private decimal MultiplierSideBetFee => Math.Max(1m, Math.Round(_currentWager * 0.03m, 2));

    /// <summary>Total per-game cost including side-bet surcharges (Multiplier max($1,3%) and First/Last +$1).</summary>
    private decimal EffectiveWager => _currentWager + (_useMultiplier ? MultiplierSideBetFee : 0m) + (_useFirstLast ? 1m : 0m);

    /// <summary>Refreshes the three FAV button labels and colors from Preferences.</summary>
    private void UpdateFavoriteButtons()
    {
        for (int i = 0; i < 3; i++)
        {
            string raw = Preferences.Default.Get($"fav_{i}", string.Empty);
            Button btn = i switch { 0 => BtnFav0, 1 => BtnFav1, _ => BtnFav2 };

            if (string.IsNullOrEmpty(raw))
            {
                btn.Text = $"★ {i + 1}  —";
                btn.BackgroundColor = FavEmpty;
            }
            else
            {
                int count = raw.Split(',').Length;
                btn.Text = $"★{i + 1}  {count}#";
                btn.BackgroundColor = FavOccupied;
            }
        }
    }

    private async void BtnFav0_Clicked(object? sender, EventArgs e) => await HandleFavoriteAsync(0);

    private async void BtnFav1_Clicked(object? sender, EventArgs e) => await HandleFavoriteAsync(1);

    private async void BtnFav2_Clicked(object? sender, EventArgs e) => await HandleFavoriteAsync(2);

    private async Task HandleFavoriteAsync(int slot)
    {
        try
        {
            string raw = Preferences.Default.Get($"fav_{slot}", string.Empty);
            bool hasData = !string.IsNullOrEmpty(raw);

            string[] options = hasData ? ["Load", "Save", "Clear"] : ["Save"];
            string? action = await DisplayActionSheetAsync($"Slot {slot + 1}", "Cancel", null, options);

            switch (action)
            {
                case "Save":
                    if (_selectedNumbers.Count == 0)
                    {
                        await DisplayAlertAsync("Nothing to Save", "Select some numbers first.", "OK");
                        return;
                    }
                    Preferences.Default.Set($"fav_{slot}", string.Join(",", _selectedNumbers.Order()));
                    UpdateFavoriteButtons();
                    break;

                case "Load":
                    int[] nums = [.. raw.Split(',').Select(int.Parse)];
                    ResetRoundState();
                    foreach (int n in nums)
                    {
                        _selectedNumbers.Add(n);
                        _numberLabels[n].BackgroundColor = CellSelected;
                    }
                    UpdatePicksDisplay();
                    UpdateSideBetInfo();
                    UpdateStatus();
                    break;

                case "Clear":
                    Preferences.Default.Remove($"fav_{slot}");
                    UpdateFavoriteButtons();
                    break;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    // ── Consecutive games stepper ─────────────────────────────────────────────

    private void BtnGamesMinus_Clicked(object? sender, EventArgs e)
    {
        if (_consecutiveGames > 1) _consecutiveGames--;
        LblGamesCount.Text = _consecutiveGames.ToString();
        Preferences.Default.Set(PrefGames, _consecutiveGames);
        UpdateBonusDisplay();
    }

    private void BtnGamesPlus_Clicked(object? sender, EventArgs e)
    {
        if (_consecutiveGames < 20) _consecutiveGames++;
        LblGamesCount.Text = _consecutiveGames.ToString();
        Preferences.Default.Set(PrefGames, _consecutiveGames);
        UpdateBonusDisplay();
        ClampGamesToBalance();
    }

    // ── Custom wager ─────────────────────────────────────────────────────────

    private async void BtnCustomWager_Clicked(object? sender, EventArgs e)
    {
        string initial = _activeWagerBtn == _customWagerBtn ? $"{_currentWager:0.##}" : string.Empty;

        string? result = await DisplayPromptAsync(
            "Custom Wager",
            "Enter any wager amount:",
            initialValue: initial,
            keyboard: Keyboard.Numeric,
            maxLength: 10);

        if (string.IsNullOrWhiteSpace(result)) return;
        if (!decimal.TryParse(result, out decimal amount) || amount < 0.01m)
        {
            await DisplayAlertAsync("Invalid Amount", "Please enter a valid amount of at least $0.01.", "OK");
            return;
        }

        amount = Math.Round(amount, 2);

        _activeWagerBtn?.BackgroundColor = WagerDefault;
        _currentWager = amount;

        if (_customWagerBtn is not null)
        {
            _customWagerBtn.Text = $"${amount:0.##}";
            _customWagerBtn.BackgroundColor = WagerActive;
            _activeWagerBtn = _customWagerBtn;
        }

        ClampGamesToBalance();
        UpdateBonusDisplay();
        UpdateSideBetInfo();
        Preferences.Default.Set(PrefWager, (double)_currentWager);
        UpdateStatus();
    }

    // ── Replenish bank ────────────────────────────────────────────────────────

    private async void BtnReplenish_Clicked(object? sender, EventArgs e)
    {
        string? action = await DisplayActionSheetAsync(
            "Replenish Bank",
            "Cancel",
            null,
            "+$1,000", "+$5,000", "Reset to $10,000");

        _bankBalance = action switch
        {
            "+$1,000" => _bankBalance + 1_000m,
            "+$5,000" => _bankBalance + 5_000m,
            "Reset to $10,000" => 10_000m,
            _ => _bankBalance
        };

        ClampGamesToBalance();
        UpdateStatus();
    }

    // ── Draw speed ────────────────────────────────────────────────────────────

    /// <summary>Activates a speed button, highlights it, and stores the delay.</summary>
    private void SetSpeed(Button btn, int delayMs)
    {
        _activeSpeedBtn?.BackgroundColor = SpeedOff;
        _activeSpeedBtn = btn;
        btn.BackgroundColor = SpeedOn;
        _drawDelayMs = delayMs;
        Preferences.Default.Set(PrefSpeed, _drawDelayMs);
    }

    private void BtnSpeedSlow_Clicked(object? sender, EventArgs e) => SetSpeed(BtnSpeedSlow, 1000);

    private void BtnSpeedMed_Clicked(object? sender, EventArgs e) => SetSpeed(BtnSpeedMed, 500);

    private void BtnSpeedFast_Clicked(object? sender, EventArgs e) => SetSpeed(BtnSpeedFast, 200);

    private void BtnSpeedSS_Clicked(object? sender, EventArgs e) => SetSpeed(BtnSpeedSS, 0);
}