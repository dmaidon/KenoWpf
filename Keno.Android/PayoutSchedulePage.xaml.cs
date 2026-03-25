// Last Edit: 2026-03-25 02:21 PM - Payout schedule code-behind: pick selector + match→payout table via KenoPayouts.
using Keno.Core;
using Microsoft.Maui.Controls.Shapes;

namespace Keno.Android;

public partial class PayoutSchedulePage : ContentPage
{
    // ── Colors ────────────────────────────────────────────────────────────────
    private static readonly Color PickOnBg       = Colors.White;
    private static readonly Color PickOnText     = Color.FromArgb("#2196A6");
    private static readonly Color PickOffBg      = Colors.Transparent;
    private static readonly Color PickOffText    = Colors.White;
    private static readonly Color ColHeaderBg    = Color.FromArgb("#2196A6");
    private static readonly Color ColHeaderText  = Colors.White;
    private static readonly Color RowOdd         = Colors.White;
    private static readonly Color RowEven        = Color.FromArgb("#F5F5F5");
    private static readonly Color PayGold        = Color.FromArgb("#E65100");
    private static readonly Color PayGreen       = Color.FromArgb("#2E7D32");
    private static readonly Color PayNone        = Colors.Gray;
    private static readonly Color ZeroCatchGreen = Color.FromArgb("#1B5E20");

    private Button? _activePickBtn;

    public PayoutSchedulePage()
    {
        InitializeComponent();
        BuildPickButtons();
        SelectPick(5);
    }

    // ── Pick selector ─────────────────────────────────────────────────────────

    private void BuildPickButtons()
    {
        for (int i = 1; i <= 20; i++)
        {
            int picks = i;
            var btn = new Button
            {
                Text            = picks.ToString(),
                FontSize        = 12,
                FontAttributes  = FontAttributes.Bold,
                WidthRequest    = 30,
                HeightRequest   = 30,
                CornerRadius    = 4,
                Padding         = new Thickness(0),
                BorderWidth     = 0,
                BackgroundColor = PickOffBg,
                TextColor       = PickOffText
            };
            btn.Clicked += (_, _) => SelectPick(picks);
            PickButtonsLayout.Add(btn);
        }
    }

    private void SelectPick(int picks)
    {
        if (_activePickBtn is not null)
        {
            _activePickBtn.BackgroundColor = PickOffBg;
            _activePickBtn.TextColor       = PickOffText;
        }

        var btn = (Button)PickButtonsLayout.Children[picks - 1];
        btn.BackgroundColor = PickOnBg;
        btn.TextColor       = PickOnText;
        _activePickBtn      = btn;

        BuildPayoutTable(picks);
    }

    // ── Payout table ──────────────────────────────────────────────────────────

    private void BuildPayoutTable(int picks)
    {
        PayoutContent.Clear();

        var infoCard = new Border
        {
            BackgroundColor = Color.FromArgb("#D4EBF5"),
            StrokeThickness = 0,
            StrokeShape     = new RoundRectangle { CornerRadius = new CornerRadius(6) },
            Padding         = new Thickness(10, 8),
            Margin          = new Thickness(0, 0, 0, 6),
            Content         = new Label
            {
                Text      = $"Pick {picks} — Payouts shown as multipliers × your wager. \"AT $5\" column shows the cash payout for a $5 bet.",
                FontSize  = 12,
                TextColor = Color.FromArgb("#1A5276")
            }
        };
        PayoutContent.Add(infoCard);
        PayoutContent.Add(BuildColumnHeader());

        var entries = KenoPayouts.GetPayoutScheduleEntries(picks);

        // Highest catch first; 0-catch (free match) at the bottom
        var sorted = entries
            .OrderByDescending(kv => kv.Key == 0 ? -1 : kv.Key)
            .ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            var kv = sorted[i];
            PayoutContent.Add(BuildPayRow(kv.Key, kv.Value, picks, i % 2 == 0 ? RowOdd : RowEven));
        }
    }

    private static Grid BuildColumnHeader()
    {
        var grid = new Grid
        {
            BackgroundColor = ColHeaderBg,
            Padding         = new Thickness(8, 6),
            ColumnSpacing   = 4
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0.45, GridUnitType.Star)));
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0.30, GridUnitType.Star)));
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0.25, GridUnitType.Star)));

        grid.Add(Cell("CATCH", 12, FontAttributes.Bold, ColHeaderText, TextAlignment.Start),  0, 0);
        grid.Add(Cell("PAYS",  12, FontAttributes.Bold, ColHeaderText, TextAlignment.Center), 1, 0);
        grid.Add(Cell("AT $5", 12, FontAttributes.Bold, ColHeaderText, TextAlignment.End),    2, 0);

        return grid;
    }

    private static Grid BuildPayRow(int matched, decimal mult, int picks, Color bg)
    {
        var grid = new Grid
        {
            BackgroundColor = bg,
            Padding         = new Thickness(8, 7),
            ColumnSpacing   = 4
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0.45, GridUnitType.Star)));
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0.30, GridUnitType.Star)));
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0.25, GridUnitType.Star)));

        bool isZeroCatch  = matched == 0;
        string catchLabel = isZeroCatch ? "0 Catch ★" : $"{matched} of {picks}";
        FontAttributes catchFont  = isZeroCatch ? FontAttributes.Italic : FontAttributes.Bold;
        Color catchColor          = isZeroCatch ? ZeroCatchGreen : Colors.Black;

        Color payColor  = mult >= 1000m ? PayGold : mult > 0m ? PayGreen : PayNone;
        string multStr  = mult > 0m ? $"×{mult:N0}" : "—";
        string exStr    = mult > 0m ? $"{mult * 5m:C0}" : "—";

        grid.Add(Cell(catchLabel, 12, catchFont,           catchColor, TextAlignment.Start),  0, 0);
        grid.Add(Cell(multStr,    12, FontAttributes.Bold, payColor,   TextAlignment.Center), 1, 0);
        grid.Add(Cell(exStr,      12, FontAttributes.None, payColor,   TextAlignment.End),    2, 0);

        return grid;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Creates a Label configured as a table cell.</summary>
    private static Label Cell(string text, double fontSize, FontAttributes attrs, Color color, TextAlignment align) =>
        new()
        {
            Text                    = text,
            FontSize                = fontSize,
            FontAttributes          = attrs,
            TextColor               = color,
            HorizontalTextAlignment = align,
            VerticalTextAlignment   = TextAlignment.Center
        };

    private async void BtnClose_Clicked(object? sender, EventArgs e) =>
        await Navigation.PopModalAsync();
}
