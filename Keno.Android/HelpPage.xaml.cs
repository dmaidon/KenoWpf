// Last Edit: Apr 10, 2026 15:15 - Clarify multiplier side-bet fee example and net-win behavior guidance.
using Microsoft.Maui.Controls.Shapes;

namespace Keno.Android;

public partial class HelpPage : ContentPage
{
    // ── Colors ────────────────────────────────────────────────────────────────
    private static readonly Color SectionBg    = Color.FromArgb("#2196A6");
    private static readonly Color SectionText  = Colors.White;
    private static readonly Color BodyText     = Color.FromArgb("#333333");
    private static readonly Color AccentText   = Color.FromArgb("#2196A6");
    private static readonly Color BonusGreen   = Color.FromArgb("#2E7D32");
    private static readonly Color BonusGray    = Colors.Gray;
    private static readonly Color DividerColor = Color.FromArgb("#DDDDDD");

    private bool _firstSection = true;

    public HelpPage()
    {
        InitializeComponent();
        BuildHelp();
    }

    private void BuildHelp()
    {
        AddSection("The Board");
        AddParagraph("The 8×10 grid shows numbers 1–80. Tap any number to select it (highlighted in teal). Tap again to deselect. You may pick 1–20 numbers per game. Your selections are shown in the PICKS strip below the board.");

        AddSection("Wager");
        AddParagraph("Choose a preset wager ($1–$200) or tap CUSTOM to enter any amount ≥ $0.01. The top bar shows the total cost for the current game queue (wager × games + any active side-bet fees).");

        AddSection("Quick Pick");
        AddParagraph("Use the − / + stepper to set a count, then tap PICK to auto-select that many random numbers. The stepper defaults to 5 picks and remembers your last choice across sessions.");

        AddSection("PLAY / REPLAY / CLEAR");
        AddBullet("PLAY",   "Deducts the total wager from your bank, draws 20 balls, and pays out based on how many of your picks match the draw.");
        AddBullet("REPLAY", "Re-selects your last picks and immediately plays a new game with the same wager — no need to tap numbers again.");
        AddBullet("CLEAR",  "Resets the board, pick/draw strips, and status labels without deducting from your bank.");

        AddSection("Consecutive Games");
        AddParagraph("Use the GAMES stepper (1–20) to queue multiple games in a single PLAY. The same picks are used for every game in the run. A series bonus multiplier is applied to the combined payout at the end:");
        AddBonusTable();

        AddSection("Side Bets");
        AddBullet("MULTIPLIER (+$1 or 3%)", "A weighted ×1–×10 multiplier is drawn before each game and applied to your base payout. The fee is max($1.00, 3% of your base wager) per game (example: $200 base wager = $6.00 fee). The multiplier draw favors lower values.");
        AddBullet("POWERBALL (free)",  "A random ball 1–80 is drawn. If it matches any of your picks, your payout is multiplied by ×4. No extra cost.");
        AddBullet("FIRST/LAST (+$1)",  "If the first or last ball drawn is one of your picks, you receive a flat cash bonus (Pick 1 = $75 down to Pick 20 = $5).");

        AddSection("Draw Speed");
        AddBullet("SLOW", "1 second per ball — full animation.");
        AddBullet("MED",  "0.5 seconds per ball.");
        AddBullet("FAST", "0.2 seconds per ball — default.");
        AddBullet("SS",   "Instant — no animation delay.");

        AddSection("Favorites");
        AddParagraph("Three ★ favorite slots let you save and restore pick sets. Tap a ★ button while numbers are selected to save that set to the slot. Tap it again to load or clear the saved set via the action sheet. Favorites are stored permanently and survive app restarts.");

        AddSection("Bank & Replenish");
        AddParagraph("Your bank balance is saved after every game and restored when you reopen the app. If your balance reaches $0, a REPLENISH BANK button appears. Tap it to add $1,000, $5,000, or reset to $10,000. The game automatically reduces the games count if your current wager × games exceeds your balance.");

        AddSection("Reading the Top Bar");
        AddBullet("Bank",  "Your current cash balance.");
        AddBullet("Wager", "Total cost of the next PLAY (wager × games + side-bet fees).");
        AddBullet("Picks", "Number of numbers currently selected / maximum allowed (20).");
    }

    // ── Section builders ─────────────────────────────────────────────────────

    private void AddSection(string title)
    {
        if (!_firstSection)
        {
            HelpContent.Add(new BoxView
            {
                HeightRequest   = 1,
                BackgroundColor = DividerColor,
                Margin          = new Thickness(0, 4, 0, 0)
            });
        }
        _firstSection = false;

        HelpContent.Add(new Border
        {
            BackgroundColor = SectionBg,
            StrokeThickness = 0,
            StrokeShape     = new RoundRectangle { CornerRadius = new CornerRadius(5) },
            Padding         = new Thickness(10, 6),
            Content         = new Label
            {
                Text           = title.ToUpperInvariant(),
                FontSize       = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor      = SectionText
            }
        });
    }

    private void AddParagraph(string text)
    {
        HelpContent.Add(new Label
        {
            Text          = text,
            FontSize      = 13,
            TextColor     = BodyText,
            LineBreakMode = LineBreakMode.WordWrap,
            Margin        = new Thickness(2, 2, 2, 0)
        });
    }

    private void AddBullet(string term, string description)
    {
        var row = new Grid { ColumnSpacing = 6, Margin = new Thickness(2, 2, 2, 0) };
        row.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(80)));
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        row.Add(new Label
        {
            Text                    = term,
            FontSize                = 13,
            FontAttributes          = FontAttributes.Bold,
            TextColor               = AccentText,
            HorizontalTextAlignment = TextAlignment.End,
            VerticalTextAlignment   = TextAlignment.Start
        }, 0, 0);

        row.Add(new Label
        {
            Text                  = description,
            FontSize              = 13,
            TextColor             = BodyText,
            LineBreakMode         = LineBreakMode.WordWrap,
            VerticalTextAlignment = TextAlignment.Start
        }, 1, 0);

        HelpContent.Add(row);
    }

    private void AddBonusTable()
    {
        var tiers = new (string Games, string Bonus, bool IsBonus)[]
        {
            ("1 – 4 games",  "No bonus", false),
            ("5 – 7 games",  "× 1.10",   true),
            ("8 – 11 games", "× 1.25",   true),
            ("12 – 16 games","× 1.50",   true),
            ("17 – 20 games","× 1.75",   true),
        };

        var table = new VerticalStackLayout { Spacing = 1, Margin = new Thickness(2, 4, 2, 2) };

        foreach (var (games, bonus, isBonus) in tiers)
        {
            var row = new Grid
            {
                BackgroundColor = Color.FromArgb("#FAFAFA"),
                Padding         = new Thickness(8, 5),
                ColumnSpacing   = 8
            };
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

            row.Add(new Label
            {
                Text                  = games,
                FontSize              = 12,
                TextColor             = BodyText,
                VerticalTextAlignment = TextAlignment.Center
            }, 0, 0);

            row.Add(new Label
            {
                Text                  = bonus,
                FontSize              = 12,
                FontAttributes        = FontAttributes.Bold,
                TextColor             = isBonus ? BonusGreen : BonusGray,
                VerticalTextAlignment = TextAlignment.Center
            }, 1, 0);

            table.Add(row);
        }

        HelpContent.Add(table);
    }

    private async void BtnClose_Clicked(object? sender, EventArgs e) =>
        await Navigation.PopModalAsync();
}
