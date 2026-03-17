' Last Edit: 2026-03-17 01:21 PM - freegames topic expanded to match WinForms detail (Earning, Button, Session Tracking).
Class WinKenoHelp

    Private ReadOnly _helpContent As New Dictionary(Of String, Action)()

    Friend Shared Sub ShowWindow(owner As Window)
        Dim win As New WinKenoHelp With {
            .Owner = owner
        }
        win.ShowDialog()
    End Sub

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub WinKenoHelp_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        BuildHelpContent()
        BuildTopicsTree()

        If TvwTopics.Items.Count > 0 Then
            Dim first = CType(TvwTopics.Items(0), TreeViewItem)
            first.IsSelected = True
        End If
    End Sub

    ' ── Tree ──────────────────────────────────────────────────────────────────
    Private Sub BuildTopicsTree()
        Dim topics() As String = {
            "overview", "picking", "wager", "playing", "consecutive",
            "quickpick", "multiplier", "wayticket", "powerball", "bullseye",
            "quadrants", "bank", "statistics", "favorites", "drawspeed",
            "progressive", "freegames", "firstlastball", "gamelog"
        }
        Dim labels() As String = {
            "Overview", "Picking Numbers", "Placing a Wager", "Playing the Game",
            "Consecutive Games", "Quick Pick", "Multiplier Keno", "Way Ticket",
            "Powerball", "Bullseye", "Quadrants / Halves", "Bank & Winnings",
            "Hot & Cold Statistics", "My Favorites", "Draw Speed",
            "Progressive Jackpot", "Free Games", "First/Last Ball Bonus", "Game Log"
        }

        For i = 0 To topics.Length - 1
            Dim item As New TreeViewItem() With {
                .Header = labels(i),
                .Tag = topics(i)
            }
            TvwTopics.Items.Add(item)
        Next
    End Sub

    Private Sub TvwTopics_SelectedItemChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Object))
        Dim item = TryCast(e.NewValue, TreeViewItem)
        If item Is Nothing Then Return

        Dim key = TryCast(item.Tag, String)
        If key Is Nothing Then Return

        Dim renderTopic As Action = Nothing
        If _helpContent.TryGetValue(key, renderTopic) Then
            Dim doc As New FlowDocument() With {
                .FontFamily = New FontFamily("Segoe UI"),
                .FontSize = 11,
                .PagePadding = New Thickness(10)
            }
            DocViewer.Document = doc
            _currentDoc = doc
            renderTopic()
        End If
    End Sub

    Private _currentDoc As FlowDocument

    ' ── Rich text helpers ─────────────────────────────────────────────────────
    Private Sub AppendTitle(text As String)
        _currentDoc.Blocks.Add(New Paragraph(New Run(text)) With {
            .FontSize = 14,
            .FontWeight = FontWeights.Bold,
            .Foreground = New SolidColorBrush(Color.FromRgb(25, 70, 140)),
            .Margin = New Thickness(0, 0, 0, 4)
        })
    End Sub

    Private Sub AppendHeading(text As String)
        _currentDoc.Blocks.Add(New Paragraph(New Run(text)) With {
            .FontSize = 11,
            .FontWeight = FontWeights.Bold,
            .Foreground = New SolidColorBrush(Color.FromRgb(0, 110, 155)),
            .Margin = New Thickness(0, 8, 0, 2)
        })
    End Sub

    Private Sub AppendBody(text As String)
        _currentDoc.Blocks.Add(New Paragraph(New Run(text)) With {
            .Foreground = New SolidColorBrush(Color.FromRgb(30, 30, 30)),
            .Margin = New Thickness(0, 0, 0, 2)
        })
    End Sub

    Private Sub AppendBlank()
        _currentDoc.Blocks.Add(New Paragraph() With {.Margin = New Thickness(0, 0, 0, 2)})
    End Sub

    ' ── Help content ─────────────────────────────────────────────────────────
    Private Sub BuildHelpContent()
        _helpContent("overview") = Sub()
                                       AppendTitle("KENO — OVERVIEW")
                                       AppendBlank()
                                       AppendBody("Keno is a lottery-style game where you pick up to 20 numbers from a board " &
                                                  "numbered 1 through 80. The game then draws 20 numbers at random. You win " &
                                                  "based on how many of your chosen numbers match the drawn numbers.")
                                       AppendBlank()
                                       AppendHeading("HOW TO PLAY")
                                       AppendBody("  1. Click numbers on the Keno board (1–80) to select your picks." & vbCrLf &
                                                  "  2. Choose a wager amount from the Wager panel." & vbCrLf &
                                                  "  3. Press the Play button to draw 20 numbers." & vbCrLf &
                                                  "  4. Matched numbers are highlighted in gold. Winnings appear in the Winnings display.")
                                       AppendBlank()
                                       AppendHeading("COLOUR GUIDE")
                                       AppendBody("  Light Green  — Numbers you selected" & vbCrLf &
                                                  "  Light Blue   — Numbers drawn by the game" & vbCrLf &
                                                  "  Gold         — Your picks that matched drawn numbers" & vbCrLf &
                                                  "  Royal Blue   — Drawn numbers not in your picks")
                                       AppendBlank()
                                       AppendBody("Select a topic from the list on the left for details on each feature.")
                                   End Sub

        _helpContent("picking") = Sub()
                                      AppendTitle("PICKING NUMBERS")
                                      AppendBlank()
                                      AppendBody("Click any number on the cyan Keno board (1–80) to select it. " &
                                                 "Selected numbers turn light green. You may select 1 to 20 numbers.")
                                      AppendBlank()
                                      AppendHeading("DESELECTING")
                                      AppendBody("Click a highlighted number again to deselect it.")
                                      AppendBlank()
                                      AppendHeading("MAXIMUM PICKS")
                                      AppendBody("You may select a maximum of 20 numbers. The status bar shows 'Picks: N'.")
                                  End Sub

        _helpContent("wager") = Sub()
                                    AppendTitle("PLACING A WAGER")
                                    AppendBlank()
                                    AppendBody("Select a wager using the radio buttons: $1 $2 $3 $5 $10 $15 $20 $25 $30 $40 $50 $100 $150 $200")
                                    AppendBlank()
                                    AppendHeading("CUSTOM WAGER")
                                    AppendBody("Enter any amount in the custom wager field to override the radio buttons.")
                                    AppendBlank()
                                    AppendHeading("LIVE WAGER PREVIEW")
                                    AppendBody("The Wager Total label updates live as you change picks, wager, consecutive count, or special play options.")
                                End Sub

        _helpContent("playing") = Sub()
                                      AppendTitle("PLAYING THE GAME")
                                      AppendBlank()
                                      AppendBody("Select numbers and wager, then press Play to start a draw.")
                                      AppendBlank()
                                      AppendHeading("RESULTS")
                                      AppendBody("After 20 numbers are drawn the Winnings label shows your payout and winnings are added to your bank.")
                                  End Sub

        _helpContent("consecutive") = Sub()
                                          AppendTitle("CONSECUTIVE GAMES")
                                          AppendBlank()
                                          AppendBody("Queue 2–20 games to be played automatically using the same picks and wager.")
                                          AppendBlank()
                                          AppendHeading("BONUS MULTIPLIER")
                                          AppendBody("  5–7 games  : 1.1×" & vbCrLf &
                                                     "  8–11 games : 1.25×" & vbCrLf &
                                                     "  12–16 games: 1.5×" & vbCrLf &
                                                     "  17–20 games: 1.75×")
                                      End Sub

        _helpContent("quickpick") = Sub()
                                        AppendTitle("QUICK PICK")
                                        AppendBlank()
                                        AppendBody("Instantly fills your selection with randomly chosen numbers. " &
                                                   "Enter the count (1–20) in the Quick Pick box then click Quick Pick.")
                                    End Sub

        _helpContent("multiplier") = Sub()
                                         AppendTitle("MULTIPLIER KENO")
                                         AppendBlank()
                                         AppendBody("Adds $1 per game to your wager. A multiplier is drawn each game before payouts are calculated:")
                                         AppendBlank()
                                         AppendBody("  1×  (45 %)   2×  (25 %)   3×  (15 %)" & vbCrLf &
                                                    "  4×  ( 8 %)   5×  ( 4 %)   8×  ( 2 %)   10× ( 1 %)")
                                         AppendBlank()
                                         AppendBody("The multiplier drawn applies only to that individual game. " &
                                                    "In a consecutive series every game draws its own multiplier independently.")
                                     End Sub

        _helpContent("wayticket") = Sub()
                                        AppendTitle("WAY TICKET")
                                        AppendBlank()
                                        AppendBody("Splits your picks into equal-sized groups (G1–G5). Each unique combination " &
                                                   "of groups becomes a sub-ticket played independently. " &
                                                   "Total wager = base bet × number of sub-tickets.")
                                        AppendBlank()
                                        AppendHeading("KING TICKET")
                                        AppendBody("Crown one number as King (♛). The King is added to every sub-ticket automatically, " &
                                                   "increasing each sub-ticket by one spot. Click the King number again to unset it.")
                                        AppendBlank()
                                        AppendBody("Open the Way Ticket dialog by checking Way Ticket and pressing Play.")
                                    End Sub

        _helpContent("powerball") = Sub()
                                        AppendTitle("POWERBALL")
                                        AppendBlank()
                                        AppendBody("Optional +$1 side-bet available on Pick 6–20. After the regular 20-ball draw, " &
                                                   "a 21st ball is drawn from the 60 unused numbers. " &
                                                   "If it lands on one of your picks, any win is multiplied by ×4.")
                                        AppendBlank()
                                        AppendBody("Powerball stacks with Multiplier Keno and is unchecked automatically after each play.")
                                    End Sub

        _helpContent("bullseye") = Sub()
                                       AppendTitle("BULLSEYE")
                                       AppendBlank()
                                       AppendBody("Bullseye plays a fixed 8-spot pattern — the four corners, two inner-top, and two inner-bottom:")
                                       AppendBlank()
                                       AppendBody("  Numbers: 1, 10, 35, 36, 45, 46, 71, 80")
                                       AppendBlank()
                                       AppendHeading("PAYOUT SCHEDULE")
                                       AppendBody("  Match 8 — 30,000×   Match 7 — 500×   Match 6 — 75×" & vbCrLf &
                                                  "  Match 5 —     15×   Match 4 —   3×   Catch 0 — 10×")
                                       AppendBlank()
                                       AppendBody("Click Play Bullseye to activate. Standard bet applies.")
                                   End Sub

        _helpContent("quadrants") = Sub()
                                        AppendTitle("QUADRANTS / HALVES")
                                        AppendBlank()
                                        AppendBody("The Quick Select buttons (Q1–Q4, Top Half, Bottom Half, Left Half, Right Half) " &
                                                   "fill your picks with a predefined region of the board instantly.")
                                        AppendBlank()
                                        AppendHeading("SELECTION SIZES")
                                        AppendBody("  Single quadrant (Q1, Q2, Q3, or Q4) — 20 numbers" & vbCrLf &
                                                   "  Two quadrants (e.g. Q1+Q2, Top, Left)  — 40 numbers")
                                        AppendBlank()
                                        AppendBody("Quadrant/Half selections and manual number picks are mutually exclusive — " &
                                                   "selecting one clears the other.")
                                    End Sub

        _helpContent("bank") = Sub()
                                   AppendTitle("BANK & WINNINGS")
                                   AppendBlank()
                                   AppendBody("Your bank balance persists between sessions. Winnings are added and wagers are deducted automatically. " &
                                              "The bank is saved to disk after every game.")
                               End Sub

        _helpContent("statistics") = Sub()
                                         AppendTitle("HOT & COLD STATISTICS")
                                         AppendBlank()
                                         AppendBody("After 20 games the app tracks how often each number has been drawn " &
                                                    "across the last 15 draws. Statistics persist between sessions in " &
                                                    "Data\draw-stats.json.")
                                         AppendBlank()
                                         AppendHeading("STATUS BAR PILLS")
                                         AppendBody("  Hot numbers  — top 5 most frequent, shown as red pills in the bottom status bar (StatBar2)." & vbCrLf &
                                                    "  Cold numbers — top 5 least frequent, shown as blue pills in the top status bar (StatBar1).")
                                         AppendBlank()
                                         AppendHeading("CLICK TO TOGGLE")
                                         AppendBody("Click any hot or cold pill to toggle that number on the Keno grid, " &
                                                    "exactly the same as clicking the number on the board directly.")
                                         AppendBlank()
                                         AppendHeading("WIN/LOSS STREAKS")
                                         AppendBody("Current win streak, loss streak, and all-time best streak are shown " &
                                                    "in the top status bar and update after every game.")
                                     End Sub

        _helpContent("favorites") = Sub()
                                        AppendTitle("MY FAVORITES")
                                        AppendBlank()
                                        AppendBody("Save up to 3 sets of favorite numbers. Click Save My Favorites after selecting numbers. " &
                                                   "Click Play Favorites to instantly reload a saved set.")
                                    End Sub

        _helpContent("drawspeed") = Sub()
                                        AppendTitle("DRAW SPEED")
                                        AppendBlank()
                                        AppendBody("Controls how fast draws animate:" & vbCrLf &
                                                   "  Slow   — 1000 ms per number" & vbCrLf &
                                                   "  Medium —  500 ms per number" & vbCrLf &
                                                   "  Fast   —  100 ms per number")
                                    End Sub

        _helpContent("progressive") = Sub()
                                          AppendTitle("PROGRESSIVE JACKPOT")
                                          AppendBlank()
                                          AppendBody("A growing jackpot displayed in the status bar. " &
                                                     "Any wager of $5 or more contributes 5 % of the bet to the pool.")
                                          AppendBlank()
                                          AppendHeading("HOW TO WIN")
                                          AppendBody("Select Pick 8–20 and match every single number you picked. " &
                                                     "The pool resets to a $25,000 seed after each win.")
                                      End Sub

        _helpContent("freegames") = Sub()
                                        AppendTitle("FREE GAMES")
                                        AppendBlank()
                                        AppendBody("Free games are bonus rounds earned by picking 5 through 9 numbers and " &
                                                   "matching none of the drawn numbers (a catch-none result) with a $2 or " &
                                                   "higher wager.")
                                        AppendBlank()
                                        AppendHeading("EARNING FREE GAMES")
                                        AppendBody("  • Select exactly 5–9 numbers." & vbCrLf &
                                                   "  • Place a wager of $2 or more." & vbCrLf &
                                                   "  • If none of your picks match the draw (0 matches), one free game" & vbCrLf &
                                                   "    credit is awarded automatically.")
                                        AppendBlank()
                                        AppendHeading("FREE GAME BUTTON")
                                        AppendBody("When free game credits are available the green 'Free Games Won (N)' button " &
                                                   "becomes enabled. Click it to play a free game round — no wager is " &
                                                   "deducted from your bank.")
                                        AppendBlank()
                                        AppendBody("Free games use the same number selection as your current picks. " &
                                                   "Your free game count decreases by one with each free play.")
                                        AppendBlank()
                                        AppendHeading("SESSION TRACKING")
                                        AppendBody("The total free games earned in the current session is recorded in the " &
                                                   "Session Summary. Free games persist between sessions and are tracked in " &
                                                   "Data\free-games.json. A free game cannot itself earn another free game.")
                                    End Sub

        _helpContent("firstlastball") = Sub()
                                            AppendTitle("FIRST / LAST BALL BONUS")
                                            AppendBlank()
                                            AppendBody("Optional +$1 per game. If the 1st or 20th drawn ball is one of your picks, " &
                                                       "you receive a flat dollar bonus on top of your regular payout.")
                                            AppendBlank()
                                            AppendHeading("BONUS BY PICK COUNT")
                                            AppendBody("  Pick  1 — $75    Pick  2 — $71    Pick  3 — $67    Pick  4 — $63" & vbCrLf &
                                                       "  Pick  5 — $59    Pick  6 — $55    Pick  7 — $51    Pick  8 — $47" & vbCrLf &
                                                       "  Pick  9 — $43    Pick 10 — $39    Pick 11 — $35    Pick 12 — $31" & vbCrLf &
                                                       "  Pick 13 — $27    Pick 14 — $23    Pick 15 — $20    Pick 16 — $17" & vbCrLf &
                                                       "  Pick 17 — $14    Pick 18 — $11    Pick 19 — $8     Pick 20 — $5")
                                            AppendBlank()
                                            AppendBody("The bonus is independent of bet size, Multiplier Keno, and Powerball. " &
                                                       "In a consecutive series the bonus is added flat after the series multiplier " &
                                                       "is applied (i.e. it is not multiplied by the series bonus).")
                                        End Sub

        _helpContent("gamelog") = Sub()
                                      AppendTitle("GAME LOG")
                                      AppendBlank()
                                      AppendBody("The Game Log records every draw result to disk. Open it from the status bar. " &
                                                 "Use Clear File to permanently delete the log. Entries include date, picks, matches, and payout.")
                                  End Sub
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
        Close()
    End Sub

End Class