// Last Edit: 2026-03-24 12:48 PM - Add REPLAY button: saves last picks after each game, re-selects them and auto-plays with same wager.
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

    // ── Wager presets (mirrors WPF radio buttons) ─────────────────────────────
    private static readonly decimal[] WagerPresets =
        [1m, 2m, 3m, 5m, 10m, 15m, 20m, 25m, 30m, 40m, 50m, 100m, 150m, 200m];

    // ── UI element references ─────────────────────────────────────────────────
    private readonly Label[] _numberLabels = new Label[81]; // 1-indexed, cells 1–80

    private readonly Label[] _picksLabels = new Label[21]; // 1-indexed, slots 1–20
    private readonly Label[] _drawnLabels = new Label[21]; // 1-indexed, slots 1–20
    private readonly HashSet<int> _selectedNumbers = [];
    private decimal _currentWager = 1m;
    private Button? _activeWagerBtn;
    private decimal _bankBalance = 10_000m;
    private int _currentWinStreak = 0;
    private int _currentLossStreak = 0;
    private int[] _lastPickedNumbers = [];

    public MainPage()
    {
        InitializeComponent();
        BuildNumberGrid();
        BuildPicksGrid();
        BuildDrawnGrid();
        BuildWagerButtons();
        UpdateStatus();
    }

    // ── Board construction ────────────────────────────────────────────────────

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

        // Highlight the default $1 wager
        foreach (IView child in WagerButtonsLayout)
        {
            if (child is Button first)
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

        // Restore board to the last set of picks, then play immediately
        BtnClear_Clicked(null, EventArgs.Empty);
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
        if (_bankBalance < _currentWager)
        {
            await DisplayAlertAsync("Insufficient Funds", "Your balance is too low for this wager.", "OK");
            return;
        }

        BtnPlay.IsEnabled = false;
        BtnReplay.IsEnabled = false;
        BtnClear.IsEnabled = false;

        try
        {
            _bankBalance -= _currentWager;
            UpdateStatus();

            // Reset drawn display from prior game
            for (int i = 1; i <= 20; i++)
            {
                _drawnLabels[i].Text = string.Empty;
                _drawnLabels[i].BackgroundColor = DrawnCellEmpty;
            }

            List<int> drawn = DrawNumbers();

            // Reveal balls one by one with animation
            for (int i = 0; i < 20; i++)
            {
                int ball = drawn[i];
                bool matched = _selectedNumbers.Contains(ball);
                _drawnLabels[i + 1].Text = ball.ToString();
                _drawnLabels[i + 1].BackgroundColor = matched ? CellMatched : CellDrawn;
                _numberLabels[ball].BackgroundColor = matched ? CellMatched : CellDrawnBoard;
                await Task.Delay(80);
            }

            int matches = drawn.Count(n => _selectedNumbers.Contains(n));
            decimal payout = KenoPayouts.GetPayout(_selectedNumbers.Count, matches) * _currentWager;
            _bankBalance += payout;

            if (payout > 0m) { _currentWinStreak++; _currentLossStreak = 0; }
            else { _currentLossStreak++; _currentWinStreak = 0; }

            LblMatches.Text = $"Matches: {matches}";
            LblPayout.Text = $"Payout: {payout:C2}";
            _lastPickedNumbers = [.. _selectedNumbers];
            UpdateStreak();
            UpdateStatus();
        }
        finally
        {
            BtnPlay.IsEnabled = true;
            BtnClear.IsEnabled = true;
            BtnReplay.IsEnabled = _lastPickedNumbers.Length > 0;
        }
    }

    private void BtnClear_Clicked(object? sender, EventArgs e)
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
        UpdateStatus();
    }

    // ── Display helpers ───────────────────────────────────────────────────────

    private void UpdatePicksDisplay()
    {
        var sorted = _selectedNumbers.Order().ToList();
        for (int i = 1; i <= 20; i++)
            _picksLabels[i].Text = i <= sorted.Count ? sorted[i - 1].ToString() : string.Empty;
    }

    private void UpdateStatus()
    {
        LblPicks.Text = _selectedNumbers.Count.ToString();
        LblWagerTotal.Text = $"{_currentWager:C2}";
        LblBank.Text = $"{_bankBalance:C2}";
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
}