// Last Edit: Apr 10, 2026 15:05 - Limit rendered session rows to keep history page responsive on Android.
using Microsoft.Maui.Controls.Shapes;

namespace Keno.Android;

public partial class HistoryPage : ContentPage
{
    private const int MaxSessionRows = 200;

    // ── Tab colors ────────────────────────────────────────────────────────────
    private static readonly Color TabActiveBg = Colors.White;

    private static readonly Color TabActiveText = Color.FromArgb("#2196A6");
    private static readonly Color TabInactiveBg = Colors.Transparent;
    private static readonly Color TabInactiveText = Colors.White;

    // ── Win/loss row colors ───────────────────────────────────────────────────
    private static readonly Color RowOdd = Colors.White;

    private static readonly Color RowEven = Color.FromArgb("#F5F5F5");
    private static readonly Color WinGreen = Color.FromArgb("#2E7D32");
    private static readonly Color LoseRed = Color.FromArgb("#C62828");
    private static readonly Color WinBadgeBg = Color.FromArgb("#4CAF50");
    private static readonly Color LoseBadgeBg = Color.FromArgb("#EF5350");

    private readonly IReadOnlyList<GameRecord> _session;

    public HistoryPage(IReadOnlyList<GameRecord> session)
    {
        InitializeComponent();
        _session = session;
        BuildSessionContent();
        BuildAllTimeContent();
    }

    // ── Session tab ───────────────────────────────────────────────────────────

    private void BuildSessionContent()
    {
        if (_session.Count == 0)
        {
            SessionContent.Add(new Label
            {
                Text = "No games played this session yet.",
                TextColor = Colors.Gray,
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 32)
            });
            return;
        }

        decimal totalWagered = _session.Sum(g => g.Wager);
        decimal totalWon = _session.Sum(g => g.Payout);
        decimal net = totalWon - totalWagered;
        int winCount = _session.Count(g => g.IsWin);

        SessionContent.Add(BuildSummaryCard(_session.Count, winCount, totalWagered, totalWon, net));
        SessionContent.Add(new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#CCCCCC"), Margin = new Thickness(0, 2) });

        int firstIndexToShow = Math.Max(0, _session.Count - MaxSessionRows);
        int rowIndex = 0;

        // Newest game first (limited to recent rows for UI performance)
        for (int i = _session.Count - 1; i >= firstIndexToShow; i--)
        {
            SessionContent.Add(BuildSessionRow(i + 1, _session[i], rowIndex % 2 == 0 ? RowEven : RowOdd));
            rowIndex++;
        }

        if (_session.Count > MaxSessionRows)
        {
            SessionContent.Add(new Label
            {
                Text = $"Showing latest {MaxSessionRows} of {_session.Count} games.",
                TextColor = Colors.Gray,
                FontSize = 11,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 8, 0, 0)
            });
        }
    }

    private static View BuildSummaryCard(int games, int wins, decimal wagered, decimal won, decimal net)
    {
        var border = new Border
        {
            BackgroundColor = Color.FromArgb("#D4EBF5"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(6) },
            Padding = new Thickness(8, 8),
            Margin = new Thickness(0, 0, 0, 4)
        };

        var grid = new Grid { ColumnSpacing = 4 };
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        Color netColor = net >= 0m ? WinGreen : LoseRed;
        string netText = net >= 0m ? $"+{net:C2}" : net.ToString("C2");

        grid.Add(BuildSummaryStat("GAMES", games.ToString()), 0, 0);
        grid.Add(BuildSummaryStat("WINS", $"{wins}/{games}"), 1, 0);
        grid.Add(BuildSummaryStat("WAGERED", wagered.ToString("C2")), 2, 0);
        grid.Add(BuildSummaryStat("NET", netText, netColor), 3, 0);

        border.Content = grid;
        return border;
    }

    private static VerticalStackLayout BuildSummaryStat(string label, string value, Color? valueColor = null)
    {
        var stack = new VerticalStackLayout { Spacing = 1, HorizontalOptions = LayoutOptions.Center };
        stack.Add(new Label
        {
            Text = label,
            FontSize = 8,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Gray,
            HorizontalOptions = LayoutOptions.Center
        });
        stack.Add(new Label
        {
            Text = value,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = valueColor ?? Colors.Black,
            HorizontalOptions = LayoutOptions.Center
        });
        return stack;
    }

    private static View BuildSessionRow(int gameNum, GameRecord g, Color bg)
    {
        var grid = new Grid
        {
            BackgroundColor = bg,
            Padding = new Thickness(6, 5),
            ColumnSpacing = 4
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(28)));  // #N
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));      // time + picks
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));      // wager → payout
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(38)));   // WIN/LOSE badge

        // Game number
        grid.Add(new Label
        {
            Text = $"#{gameNum}",
            FontSize = 10,
            TextColor = Colors.DarkGray,
            VerticalTextAlignment = TextAlignment.Center
        }, 0, 0);

        // Time + picks info
        var info = new VerticalStackLayout { Spacing = 0 };
        info.Add(new Label { Text = g.Time.ToString("h:mm tt"), FontSize = 10, TextColor = Colors.Gray });
        info.Add(new Label { Text = $"{g.Picked} pick → {g.Matched} match", FontSize = 11, FontAttributes = FontAttributes.Bold });
        grid.Add(info, 1, 0);

        // Wager → Payout
        var pay = new VerticalStackLayout { Spacing = 0, HorizontalOptions = LayoutOptions.End };
        pay.Add(new Label { Text = g.Wager.ToString("C2"), FontSize = 10, TextColor = Colors.Gray, HorizontalOptions = LayoutOptions.End });
        pay.Add(new Label
        {
            Text = g.Payout.ToString("C2"),
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = g.IsWin ? WinGreen : LoseRed,
            HorizontalOptions = LayoutOptions.End
        });
        grid.Add(pay, 2, 0);

        // WIN / LOSE badge
        var badge = new Border
        {
            BackgroundColor = g.IsWin ? WinBadgeBg : LoseBadgeBg,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(4) },
            WidthRequest = 36,
            HeightRequest = 22,
            VerticalOptions = LayoutOptions.Center,
            Padding = new Thickness(2, 0),
            Content = new Label
            {
                Text = g.IsWin ? "WIN" : "LOSE",
                FontSize = 9,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };
        grid.Add(badge, 3, 0);

        return grid;
    }

    // ── All-time tab ──────────────────────────────────────────────────────────

    private void BuildAllTimeContent()
    {
        int games = Preferences.Default.Get("at_games", 0);
        int wins = Preferences.Default.Get("at_wins", 0);
        decimal wagered = (decimal)Preferences.Default.Get("at_wagered", 0.0);
        decimal won = (decimal)Preferences.Default.Get("at_won", 0.0);
        decimal bestWin = (decimal)Preferences.Default.Get("at_best_win", 0.0);
        decimal net = won - wagered;

        if (games == 0)
        {
            AllTimeContent.Add(new Label
            {
                Text = "No all-time data yet. Play a game to start tracking!",
                TextColor = Colors.Gray,
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 32)
            });
            return;
        }

        double winRate = (double)wins / games * 100.0;
        Color netColor = net >= 0m ? WinGreen : LoseRed;
        string netText = net >= 0m ? $"+{net:C2}" : net.ToString("C2");

        AllTimeContent.Add(BuildStatRow(
            "GAMES PLAYED", games.ToString(),
            "WIN RATE", $"{winRate:F1}%"));

        AllTimeContent.Add(BuildStatRow(
            "TOTAL WAGERED", wagered.ToString("C2"),
            "TOTAL WON", won.ToString("C2")));

        AllTimeContent.Add(BuildStatRow(
            "NET P&L", netText,
            "BEST WIN", bestWin.ToString("C2"),
            valueColor1: netColor));
    }

    private static Grid BuildStatRow(
        string label1, string value1,
        string label2, string value2,
        Color? valueColor1 = null, Color? valueColor2 = null)
    {
        var row = new Grid { ColumnSpacing = 10 };
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        row.Add(BuildStatBox(label1, value1, valueColor1), 0, 0);
        row.Add(BuildStatBox(label2, value2, valueColor2), 1, 0);
        return row;
    }

    private static Border BuildStatBox(string label, string value, Color? valueColor = null)
    {
        var stack = new VerticalStackLayout { Spacing = 4 };
        stack.Add(new Label
        {
            Text = label,
            FontSize = 10,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Gray
        });
        stack.Add(new Label
        {
            Text = value,
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = valueColor ?? Colors.Black
        });

        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#DDDDDD"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            Padding = new Thickness(14, 12),
            Content = stack
        };
    }

    // ── Tab switching ─────────────────────────────────────────────────────────

    private void BtnTabSession_Clicked(object? sender, EventArgs e)
    {
        SessionScrollView.IsVisible = true;
        AllTimeScrollView.IsVisible = false;
        BtnTabSession.BackgroundColor = TabActiveBg;
        BtnTabSession.TextColor = TabActiveText;
        BtnTabAllTime.BackgroundColor = TabInactiveBg;
        BtnTabAllTime.TextColor = TabInactiveText;
    }

    private void BtnTabAllTime_Clicked(object? sender, EventArgs e)
    {
        SessionScrollView.IsVisible = false;
        AllTimeScrollView.IsVisible = true;
        BtnTabAllTime.BackgroundColor = TabActiveBg;
        BtnTabAllTime.TextColor = TabActiveText;
        BtnTabSession.BackgroundColor = TabInactiveBg;
        BtnTabSession.TextColor = TabInactiveText;
    }

    // ── Close ─────────────────────────────────────────────────────────────────

    private async void BtnClose_Clicked(object? sender, EventArgs e)
        => await Navigation.PopModalAsync();
}