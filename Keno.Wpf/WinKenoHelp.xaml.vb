' Last Edit: 2026-03-15 - WinKenoHelp: TreeView topics + FlowDocument rich content; mirrors FrmKenoHelp topic set.
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
                                         AppendBody("Adds $1 per game to your wager. A multiplier is drawn each game:")
                                         AppendBlank()
                                         AppendBody("  1× (45%)   2× (30%)   3× (13%)   5× (9%)   10× (3%)")
                                         AppendBlank()
                                         AppendBody("Winnings for that game are multiplied by the drawn value.")
                                     End Sub

        _helpContent("wayticket") = Sub()
                                        AppendTitle("WAY TICKET")
                                        AppendBlank()
                                        AppendBody("Splits your picks into groups of 3 and evaluates each sub-ticket independently. " &
                                                   "Total wager = base bet × number of sub-tickets. Total payout is the sum of all sub-ticket payouts.")
                                    End Sub

        _helpContent("powerball") = Sub()
                                        AppendTitle("POWERBALL")
                                        AppendBlank()
                                        AppendBody("A bonus number 1–80 is drawn each game. If it matches one of your picks, " &
                                                   "your payout for that game is multiplied by 4. No extra wager required.")
                                    End Sub

        _helpContent("bullseye") = Sub()
                                       AppendTitle("BULLSEYE")
                                       AppendBlank()
                                       AppendBody("Bullseye plays the four corners plus the center of the board (numbers 1, 10, 71, 80, 40). " &
                                                  "Payouts follow a separate schedule. Currently reserved for future release.")
                                   End Sub

        _helpContent("quadrants") = Sub()
                                        AppendTitle("QUADRANTS / HALVES")
                                        AppendBlank()
                                        AppendBody("The Quick Select buttons (Q1–Q4, Top, Bottom, Left, Right) fill your picks with " &
                                                   "a predefined region of the board. 20 numbers are selected instantly.")
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
                                         AppendBody("Statistics tracking is available in the full version. Hot numbers appear most frequently; " &
                                                    "Cold numbers appear least frequently across all draws.")
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
                                          AppendBody("A growing jackpot displayed in the status bar. Match all 20 picks out of 20 drawn to win. " &
                                                     "Jackpot contribution and triggering rules are shown in the status bar.")
                                      End Sub

        _helpContent("freegames") = Sub()
                                        AppendTitle("FREE GAMES")
                                        AppendBlank()
                                        AppendBody("Earn a free $2 game when you match 0 numbers on a Pick 5 or higher ticket. " &
                                                   "Free games are added to your bank automatically.")
                                    End Sub

        _helpContent("firstlastball") = Sub()
                                            AppendTitle("FIRST/LAST BALL BONUS")
                                            AppendBlank()
                                            AppendBody("When First/Last Play is enabled (+$1 per game):" & vbCrLf &
                                                       "  • If the 1st drawn number matches a pick  → ♥ shown in the game progress cell." & vbCrLf &
                                                       "  • If the last drawn number matches a pick → ♦ shown in the game progress cell.")
                                            AppendBlank()
                                            AppendHeading("PAYOUTS")
                                            AppendBody("  First ball match : 3× the base bet" & vbCrLf &
                                                       "  Last ball match  : 5× the base bet" & vbCrLf &
                                                       "  Both match       : 10× the base bet")
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