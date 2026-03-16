' Last Edit: 2026-03-13 - Free games help updated to Pick 5-9; First/Last table corrected; consecutive multiplier note fixed.
Public Class FrmKenoHelp

    Private ReadOnly _helpContent As New Dictionary(Of String, Action)()
    Private _fntTitle As Font
    Private _fntHeading As Font

    Private Sub FrmKenoHelp_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _fntTitle = New Font(RtbContent.Font.FontFamily, 13.0F, FontStyle.Bold)
        _fntHeading = New Font(RtbContent.Font.FontFamily, 10.5F, FontStyle.Bold)
        BuildHelpContent()
        BuildTopicsTree()

        If TvwTopics.Nodes.Count > 0 Then
            TvwTopics.SelectedNode = TvwTopics.Nodes(0)
        End If
    End Sub

    Private Sub FrmKenoHelp_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        _fntTitle.Dispose()
        _fntHeading.Dispose()
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As EventArgs) Handles BtnClose.Click
        Close()
    End Sub

    Private Sub TvwTopics_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TvwTopics.AfterSelect
        If e.Node Is Nothing Then Return

        Dim renderTopic As Action = Nothing
        If _helpContent.TryGetValue(e.Node.Name, renderTopic) Then
            RtbContent.Clear()
            renderTopic()
            RtbContent.SelectionStart = 0
            RtbContent.ScrollToCaret()
        End If
    End Sub

    ' ── Rich text helpers ─────────────────────────────────────────────────────

    Private Sub RtbAppendTitle(text As String)
        RtbContent.SelectionStart = RtbContent.TextLength
        RtbContent.SelectionLength = 0
        RtbContent.SelectionFont = _fntTitle
        RtbContent.SelectionColor = Color.FromArgb(25, 70, 140)
        RtbContent.SelectedText = text & vbCrLf
    End Sub

    Private Sub RtbAppendHeading(text As String)
        RtbContent.SelectionStart = RtbContent.TextLength
        RtbContent.SelectionLength = 0
        RtbContent.SelectionFont = _fntHeading
        RtbContent.SelectionColor = Color.FromArgb(0, 110, 155)
        RtbContent.SelectedText = text & vbCrLf
    End Sub

    Private Sub RtbAppendBody(text As String)
        RtbContent.SelectionStart = RtbContent.TextLength
        RtbContent.SelectionLength = 0
        RtbContent.SelectionFont = RtbContent.Font
        RtbContent.SelectionColor = Color.FromArgb(30, 30, 30)
        RtbContent.SelectedText = text
    End Sub

    Private Sub RtbAppendBlank()
        RtbContent.SelectionStart = RtbContent.TextLength
        RtbContent.SelectionLength = 0
        RtbContent.SelectionFont = RtbContent.Font
        RtbContent.SelectedText = vbCrLf
    End Sub

    Private Sub BuildTopicsTree()
        TvwTopics.Nodes.Clear()

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

        For i As Integer = 0 To topics.Length - 1
            Dim node As New TreeNode(labels(i)) With {
                .Name = topics(i)
            }
            TvwTopics.Nodes.Add(node)
        Next
    End Sub

    ' ── Help content ─────────────────────────────────────────────────────────

    Private Sub BuildHelpContent()
        _helpContent("overview") = Sub()
                                       RtbAppendTitle("KENO — OVERVIEW")
                                       RtbAppendBlank()
                                       RtbAppendBody(
                                           "Keno is a lottery-style game where you pick up to 20 numbers from a board " &
                                           "numbered 1 through 80. The game then draws 20 numbers at random. You win " &
                                           "based on how many of your chosen numbers match the drawn numbers." & vbCrLf)
                                       RtbAppendBlank()
                                       RtbAppendHeading("HOW TO PLAY")
                                       RtbAppendBody(
                                           "  1. Click numbers on the Keno board (1–80) to select your picks." & vbCrLf &
                                           "  2. Choose a wager amount from the Wager panel." & vbCrLf &
                                           "  3. Press the Play button to draw 20 numbers." & vbCrLf &
                                           "  4. Matched numbers are highlighted in gold. Winnings are shown in the" & vbCrLf &
                                           "     Winnings display and added to your Bank." & vbCrLf)
                                       RtbAppendBlank()
                                       RtbAppendHeading("COLOUR GUIDE")
                                       RtbAppendBody(
                                           "  Light Green  — Numbers you selected" & vbCrLf &
                                           "  Light Blue   — Numbers drawn by the game" & vbCrLf &
                                           "  Gold         — Your picks that matched drawn numbers" & vbCrLf &
                                           "  Royal Blue   — Drawn numbers that were not in your picks" & vbCrLf)
                                       RtbAppendBlank()
                                       RtbAppendBody("Select a topic from the list on the left for detailed instructions on each feature.")
                                   End Sub

        _helpContent("picking") = Sub()
                                      RtbAppendTitle("PICKING NUMBERS")
                                      RtbAppendBlank()
                                      RtbAppendBody(
                                          "Click any number on the cyan Keno board (1–80) to select it. " &
                                          "Selected numbers turn light green. You may select between 1 and 20 numbers." & vbCrLf)
                                      RtbAppendBlank()
                                      RtbAppendHeading("YOUR PICKS DISPLAY")
                                      RtbAppendBody(
                                          "The orange strip below the board shows your selected numbers in sorted order " &
                                          "in slots 1–20. Slots fill left-to-right as you pick numbers." & vbCrLf)
                                      RtbAppendBlank()
                                      RtbAppendHeading("DESELECTING A NUMBER")
                                      RtbAppendBody(
                                          "Click a highlighted (light green) number again to deselect it. " &
                                          "It returns to its default colour and is removed from the picks strip." & vbCrLf)
                                      RtbAppendBlank()
                                      RtbAppendHeading("MAXIMUM PICKS")
                                      RtbAppendBody(
                                          "You may select a maximum of 20 numbers. The status bar shows the current " &
                                          "count as 'Picks: N'. Attempting to add a 21st number has no effect." & vbCrLf)
                                      RtbAppendBlank()
                                      RtbAppendHeading("CLEARING YOUR PICKS")
                                      RtbAppendBody(
                                          "Press the Clear button to deselect all numbers and reset the board " &
                                          "to its default state ready for a new selection.")
                                  End Sub

        _helpContent("wager") = Sub()
                                    RtbAppendTitle("PLACING A WAGER")
                                    RtbAppendBlank()
                                    RtbAppendBody(
                                        "Select a wager amount using the radio buttons in the Wager panel:" & vbCrLf &
                                        "  $1  $2  $3  $5  $10  $15  $20  $25  $30  $40  $50  $100  $150  $200" & vbCrLf)
                                    RtbAppendBlank()
                                    RtbAppendHeading("CUSTOM WAGER")
                                    RtbAppendBody(
                                        "Use the 'Wager:' numeric spinner at the bottom of the Wager panel to enter " &
                                        "any custom amount from $1 to $1,000. This overrides the radio button selection " &
                                        "when you press Play." & vbCrLf)
                                    RtbAppendBlank()
                                    RtbAppendHeading("WAGER DEDUCTED ON PLAY")
                                    RtbAppendBody(
                                        "The wager is deducted from your bank when you press Play. If your bank " &
                                        "balance is lower than the wager amount, play will be blocked." & vbCrLf)
                                    RtbAppendBlank()
                                    RtbAppendHeading("WAY TICKET WAGER NOTE")
                                    RtbAppendBody(
                                        "When Way Ticket is active the total cost equals the wager multiplied by the " &
                                        "number of sub-tickets. The summary label in the Special Play panel shows the " &
                                        "total cost before you press Play." & vbCrLf)
                                    RtbAppendBlank()
                                    RtbAppendHeading("LIVE WAGER PREVIEW")
                                    RtbAppendBody(
                                        "The dark red Wager Total label (below the Bank panel) updates automatically " &
                                        "as you make selections — before you press Play. It reflects the full projected " &
                                        "cost including Multiplier Keno (+$1 per game), consecutive count, and Way " &
                                        "Ticket sub-tickets, so you always know what will be deducted.")
                                End Sub

        _helpContent("playing") = Sub()
                                      RtbAppendTitle("PLAYING THE GAME")
                                      RtbAppendBlank()
                                      RtbAppendBody(
                                          "After selecting your numbers and wager, press Play to start a draw." & vbCrLf)
                                      RtbAppendBlank()
                                      RtbAppendHeading("DRAW ANIMATION")
                                      RtbAppendBody(
                                          "The game draws 20 numbers one at a time. Each drawn number lights up light blue. " &
                                          "When a drawn number matches one of your picks it turns gold on the board." & vbCrLf)
                                      RtbAppendBlank()
                                      RtbAppendHeading("RESULTS")
                                      RtbAppendBody(
                                          "After all 20 numbers are drawn:" & vbCrLf &
                                          "  • The Winnings label shows how much you won this round." & vbCrLf &
                                          "    During consecutive play it shows a running total, updating after each game." & vbCrLf &
                                          "  • The status bar shows Picks, Matches, and Payout." & vbCrLf &
                                          "  • Winnings are added to your bank automatically." & vbCrLf)
                                      RtbAppendBlank()
                                      RtbAppendHeading("PLAY BUTTON STATES")
                                      RtbAppendBody(
                                          "  Play  — Ready to start a new round." & vbCrLf &
                                          "  Stop  — A draw is in progress; click to stop the animation early " &
                                          "and jump to the result." & vbCrLf)
                                      RtbAppendBlank()
                                      RtbAppendHeading("CLEAR BUTTON")
                                      RtbAppendBody("Clears all selected numbers and resets the board for a fresh selection.")
                                  End Sub

        _helpContent("consecutive") = Sub()
                                          RtbAppendTitle("CONSECUTIVE GAMES")
                                          RtbAppendBlank()
                                          RtbAppendBody(
                                              "The Consecutive Games panel lets you queue 2–20 games to be played " &
                                              "automatically in a row using the same number selection and wager each time." & vbCrLf)
                                          RtbAppendBlank()
                                          RtbAppendBody(
                                              "Select the number of consecutive games (2–20) using the radio buttons, " &
                                              "then press Play. Each game plays in sequence without needing to press Play again." & vbCrLf)
                                          RtbAppendBlank()
                                          RtbAppendHeading("GAME PROGRESS INDICATORS")
                                          RtbAppendBody(
                                              "The numbered labels on the right of the form (1–20) track the sequence. " &
                                              "The current game label is highlighted; completed games are greyed out." & vbCrLf)
                                          RtbAppendBlank()
                                          RtbAppendHeading("BONUS MULTIPLIER")
                                          RtbAppendBody(
                                              "Playing multiple consecutive games unlocks a bonus on total series winnings:" & vbCrLf &
                                              "  2–4 games   : No bonus  (1×)" & vbCrLf &
                                              "  5–7 games   : 1.1× bonus" & vbCrLf &
                                              "  8–11 games  : 1.25× bonus" & vbCrLf &
                                              "  12–16 games : 1.5×  bonus" & vbCrLf &
                                              "  17–20 games : 1.75× bonus" & vbCrLf)
                                          RtbAppendBlank()
                                          RtbAppendBody("The active bonus is shown as 'Bonus: N×' inside the Consecutive Games box.")
                                      End Sub

        _helpContent("quickpick") = Sub()
                                        RtbAppendTitle("QUICK PICK")
                                        RtbAppendBlank()
                                        RtbAppendBody(
                                            "The Quick Pick button in the Special Play panel instantly fills your selection " &
                                            "with randomly chosen numbers." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendHeading("HOW TO USE")
                                        RtbAppendBody(
                                            "  1. Set the count spinner (next to Quick Pick) to the number of random picks " &
                                            "     you want (1–20)." & vbCrLf &
                                            "  2. Click Quick Pick." & vbCrLf &
                                            "  3. The board is cleared and the specified number of random numbers are selected." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendBody(
                                            "Quick Pick replaces any numbers you have already manually selected. " &
                                            "You can adjust your selection afterwards by clicking numbers on the board.")
                                    End Sub

        _helpContent("multiplier") = Sub()
                                         RtbAppendTitle("MULTIPLIER KENO")
                                         RtbAppendBlank()
                                         RtbAppendBody(
                                             "Enable Multiplier Keno by checking 'Multiplier Keno' in the Special Play panel." & vbCrLf)
                                         RtbAppendBlank()
                                         RtbAppendBody(
                                             "When active, a random multiplier is drawn at the start of each play:" & vbCrLf &
                                             "  x1 (45%)  x2 (30%)  x3 (13%)  x5 (9%)  x10 (3%)" & vbCrLf)
                                         RtbAppendBlank()
                                         RtbAppendBody(
                                             "In a consecutive game series, a fresh multiplier is drawn independently " &
                                             "at the start of every individual game — each round can land a different value." & vbCrLf)
                                         RtbAppendBlank()
                                         RtbAppendBody(
                                             "If you win, your payout is multiplied by this value before being added " &
                                             "to your bank. The checkbox label updates to show the drawn value (e.g. " &
                                             "'Multiplier: 3x'). Note: a 1× result is possible (40% chance) — " &
                                             "not every draw is guaranteed to boost your payout." & vbCrLf)
                                         RtbAppendBlank()
                                         RtbAppendHeading("NOTE")
                                         RtbAppendBody("Multiplier Keno cannot be combined with Way Ticket or Powerball.")
                                     End Sub

        _helpContent("wayticket") = Sub()
                                        RtbAppendTitle("WAY TICKET")
                                        RtbAppendBlank()
                                        RtbAppendBody(
                                            "A Way Ticket lets you play multiple sub-tickets from a single set of picks, " &
                                            "increasing your chances of winning on every draw." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendHeading("HOW TO USE")
                                        RtbAppendBody(
                                            "  1. Select your numbers on the board (minimum 4 required)." & vbCrLf &
                                            "  2. Check 'Way Ticket' in the Special Play panel." & vbCrLf &
                                            "  3. The game splits your picks into sub-tickets automatically using a" & vbCrLf &
                                            "     king-number grouping system." & vbCrLf &
                                            "  4. The summary label shows the sub-ticket count and total wager cost." & vbCrLf &
                                            "  5. Press Play — each sub-ticket is evaluated independently." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendBody(
                                            "Winnings are the combined total across all matching sub-tickets." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendHeading("NOTE")
                                        RtbAppendBody("Way Ticket cannot be combined with Multiplier Keno or Powerball.")
                                    End Sub

        _helpContent("powerball") = Sub()
                                        RtbAppendTitle("POWERBALL")
                                        RtbAppendBlank()
                                        RtbAppendBody(
                                            "Enable Powerball by checking 'Powerball (x4)' in the Special Play panel." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendBody(
                                            "When active, one additional Powerball number is drawn from 1–80 after " &
                                            "the regular 20-number draw. If the Powerball matches any of your picks, " &
                                            "your total winnings for that game are multiplied by 4." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendBody(
                                            "The Powerball number is shown next to the checkbox after the draw completes." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendHeading("NOTE")
                                        RtbAppendBody("Powerball cannot be combined with Way Ticket or Multiplier Keno.")
                                    End Sub

        _helpContent("bullseye") = Sub()
                                       RtbAppendTitle("BULLSEYE")
                                       RtbAppendBlank()
                                       RtbAppendBody(
                                           "Bullseye is a special preset play using only the eight corner and centre " &
                                           "positions of the Keno board." & vbCrLf)
                                       RtbAppendBlank()
                                       RtbAppendHeading("THE BULLSEYE NUMBERS")
                                       RtbAppendBody("  1, 10, 35, 36, 45, 46, 71, 80" & vbCrLf)
                                       RtbAppendBlank()
                                       RtbAppendHeading("HOW TO USE")
                                       RtbAppendBody(
                                           "Click Play Bullseye in the Special Play panel. Your current picks are " &
                                           "replaced with all eight Bullseye numbers and the game plays immediately." & vbCrLf)
                                       RtbAppendBlank()
                                       RtbAppendBody(
                                           "Payouts follow the standard table for 8 picks. Matching all 8 Bullseye " &
                                           "numbers to the draw delivers the maximum 8-pick payout.")
                                   End Sub

        _helpContent("quadrants") = Sub()
                                        RtbAppendTitle("QUADRANTS / HALVES")
                                        RtbAppendBlank()
                                        RtbAppendBody(
                                            "The Quadrants / Halves feature lets you wager on sections of the board " &
                                            "rather than individual numbers." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendHeading("THE FOUR QUADRANTS")
                                        RtbAppendBody(
                                            "  Q1 — Numbers  1–20  (top-left of the board)" & vbCrLf &
                                            "  Q2 — Numbers 21–40  (top-right of the board)" & vbCrLf &
                                            "  Q3 — Numbers 41–60  (bottom-left of the board)" & vbCrLf &
                                            "  Q4 — Numbers 61–80  (bottom-right of the board)" & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendHeading("HOW TO USE")
                                        RtbAppendBody(
                                            "Click a quadrant label (Q1–Q4) in the Special Play panel to select or " &
                                            "deselect it. Selected quadrants are highlighted. When you press Play, " &
                                            "all numbers in the selected quadrant(s) are treated as your picks." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendHeading("HALVES")
                                        RtbAppendBody(
                                            "  Top half    — Select Q1 + Q2 (numbers 1–40)" & vbCrLf &
                                            "  Bottom half — Select Q3 + Q4 (numbers 41–80)")
                                    End Sub

        _helpContent("bank") = Sub()
                                   RtbAppendTitle("BANK & WINNINGS")
                                   RtbAppendBlank()
                                   RtbAppendHeading("BANK")
                                   RtbAppendBody(
                                       "Your current balance is shown in the Bank panel (top right). It increases " &
                                       "when you win and decreases by your wager each game. The session starts with " &
                                       "a configured starting balance." & vbCrLf)
                                   RtbAppendBlank()
                                   RtbAppendHeading("WAGER TOTAL DISPLAY")
                                   RtbAppendBody(
                                       "The dark red Wager Total label shows your projected cost before you press Play. " &
                                       "It updates live as you select numbers, change your bet, adjust consecutive " &
                                       "count, or toggle Multiplier Keno. Click Clear to reset it to $0.00." & vbCrLf)
                                   RtbAppendBlank()
                                   RtbAppendHeading("WINNINGS DISPLAY")
                                   RtbAppendBody(
                                       "The green Winnings label shows how much you won. During a consecutive game " &
                                       "series it accumulates as a running total — each game's payout is added as it " &
                                       "completes so you can follow progress in real time. Zero winnings display as " &
                                       "'$0.00'. Applied multipliers (Consecutive bonus, Multiplier Keno, Powerball) " &
                                       "are factored into the displayed amount." & vbCrLf)
                                   RtbAppendBlank()
                                   RtbAppendHeading("PAYOUT SCHEDULE")
                                   RtbAppendBody(
                                       "Payouts depend on how many numbers you picked and how many matched the draw. " &
                                       "See the Payout Schedule (accessible from the main form) for the full table." & vbCrLf)
                                   RtbAppendBlank()
                                   RtbAppendHeading("STATUS BAR — BOTTOM")
                                   RtbAppendBody(
                                       "  Picks: N    — Number of numbers you selected" & vbCrLf &
                                       "  Matches: N  — How many of your picks were drawn" & vbCrLf &
                                       "  Payout: $N  — Winnings for the last game")
                               End Sub

        _helpContent("statistics") = Sub()
                                         RtbAppendTitle("HOT & COLD STATISTICS")
                                         RtbAppendBlank()
                                         RtbAppendBody(
                                             "The Stats panel (right side) tracks which numbers have appeared most and " &
                                             "least often across all draws in the current session." & vbCrLf)
                                         RtbAppendBlank()
                                         RtbAppendHeading("HOT NUMBERS (red background)")
                                         RtbAppendBody(
                                             "The five numbers drawn most frequently. These have come up often recently." & vbCrLf)
                                         RtbAppendBlank()
                                         RtbAppendHeading("COLD NUMBERS (blue background)")
                                         RtbAppendBody(
                                             "The five numbers drawn least frequently. These have appeared rarely." & vbCrLf)
                                         RtbAppendBlank()
                                         RtbAppendBody(
                                             "The Stats panel is informational only — each Keno draw is fully random " &
                                             "and independent of previous results." & vbCrLf)
                                         RtbAppendBlank()
                                         RtbAppendHeading("STREAK TRACKING")
                                         RtbAppendBody(
                                             "The top status bar shows your current win/loss streak and the best " &
                                             "winning streak recorded in the session." & vbCrLf)
                                         RtbAppendBlank()
                                         RtbAppendHeading("ALL-TIME SUMMARY")
                                         RtbAppendBody(
                                             "Click the 'Summary' button in the top status bar to view cumulative " &
                                             "statistics across all sessions (games played, total won/wagered, etc.).")
                                     End Sub

        _helpContent("favorites") = Sub()
                                        RtbAppendTitle("MY FAVORITES")
                                        RtbAppendBlank()
                                        RtbAppendBody(
                                            "Save up to 3 favourite number selections for quick replay." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendHeading("SAVING FAVORITES")
                                        RtbAppendBody(
                                            "  1. Select your numbers on the board." & vbCrLf &
                                            "  2. Click 'Save My Favorites' (bottom right of the form)." & vbCrLf &
                                            "  3. A slot picker appears — choose an empty slot (1–3) to store your set." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendHeading("PLAYING FAVORITES")
                                        RtbAppendBody(
                                            "  1. Click 'Play My Favorites' in the Special Play panel." & vbCrLf &
                                            "  2. A slot picker appears — choose the slot to load." & vbCrLf &
                                            "  3. Your saved numbers are automatically selected on the board." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendHeading("REPLAY LAST GAME")
                                        RtbAppendBody(
                                            "Click 'Replay Last Game' to instantly reload the exact numbers from your " &
                                            "most recently played game, without needing to have saved them first.")
                                    End Sub

        _helpContent("drawspeed") = Sub()
                                        RtbAppendTitle("DRAW SPEED")
                                        RtbAppendBlank()
                                        RtbAppendBody(
                                            "The Draw Speed control adjusts how quickly the 20 numbers are revealed " &
                                            "during a draw animation." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendBody(
                                            "  S — Slow    Numbers reveal slowly. Good for watching the draw unfold." & vbCrLf &
                                            "  M — Medium  Balanced speed for normal play." & vbCrLf &
                                            "  F — Fast    Numbers reveal quickly. Best for rapid consecutive games." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendBody(
                                            "Your draw speed preference is saved automatically and restored the next " &
                                            "time you launch the application.")
                                    End Sub

        _helpContent("progressive") = Sub()
                                          RtbAppendTitle("PROGRESSIVE JACKPOT")
                                          RtbAppendBlank()
                                          RtbAppendBody(
                                              "The Progressive Jackpot grows with every game played. A small portion of " &
                                              "each wager contributes to the jackpot pool." & vbCrLf)
                                          RtbAppendBlank()
                                          RtbAppendBody(
                                              "The current jackpot value is shown in purple in the top status bar:" & vbCrLf &
                                              "  'Progressive Jackpot: $N'" & vbCrLf)
                                          RtbAppendBlank()
                                          RtbAppendHeading("WINNING THE JACKPOT")
                                          RtbAppendBody(
                                              "Match ALL of your selected numbers to the drawn numbers in a single game " &
                                              "(minimum Pick 8) to win the entire Progressive Jackpot. The jackpot is then " &
                                              "reset to $25,000 and begins growing again from the next game." & vbCrLf)
                                          RtbAppendBlank()
                                          RtbAppendBody("The progressive jackpot is tracked and persisted between sessions.")
                                      End Sub

        _helpContent("freegames") = Sub()
                                        RtbAppendTitle("FREE GAMES")
                                        RtbAppendBlank()
                                        RtbAppendBody(
                                            "Free games are bonus rounds earned by picking 5 through 9 numbers and " &
                                            "matching none of the drawn numbers (a catch-none result) with a $2 or " &
                                            "higher wager." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendHeading("EARNING FREE GAMES")
                                        RtbAppendBody(
                                            "  • Select exactly 5–9 numbers." & vbCrLf &
                                            "  • Place a wager of $2 or more." & vbCrLf &
                                            "  • If none of your picks match the draw (0 matches), one free game" & vbCrLf &
                                            "    credit is awarded automatically." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendHeading("FREE GAME BUTTON")
                                        RtbAppendBody(
                                            "When free game credits are available the green 'Free Game: N' button " &
                                            "becomes enabled. Click it to play a free game round — no wager is " &
                                            "deducted from your bank." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendBody(
                                            "Free games use the same number selection as your most recently played " &
                                            "paid game. Your free game count decreases by one with each free play." & vbCrLf)
                                        RtbAppendBlank()
                                        RtbAppendHeading("SESSION TRACKING")
                                        RtbAppendBody(
                                            "The total free games earned in the current session is recorded in the " &
                                            "Session Summary.")
                                    End Sub

        _helpContent("firstlastball") = Sub()
                                            RtbAppendTitle("FIRST / LAST BALL BONUS")
                                            RtbAppendBlank()
                                            RtbAppendBody(
                                                "If the first or last ball drawn in a game matches any of your selected " &
                                                "numbers, a flat dollar bonus is added to your payout for that game." & vbCrLf)
                                            RtbAppendBlank()
                                            RtbAppendHeading("APPLIES TO")
                                            RtbAppendBody(
                                                "  • Pick 1–20 regular play, Bullseye, and Way Ticket sub-tickets." & vbCrLf &
                                                "  • Does not apply to Quadrant or Half-board (area) play." & vbCrLf)
                                            RtbAppendBlank()
                                            RtbAppendHeading("PAYOUT TABLE (flat dollar — not a multiplier)")
                                            RtbAppendBody(
                                                "  Pick  1 = $75      Pick 11 = $35" & vbCrLf &
                                                "  Pick  2 = $71      Pick 12 = $31" & vbCrLf &
                                                "  Pick  3 = $67      Pick 13 = $27" & vbCrLf &
                                                "  Pick  4 = $63      Pick 14 = $23" & vbCrLf &
                                                "  Pick  5 = $59      Pick 15 = $20" & vbCrLf &
                                                "  Pick  6 = $55      Pick 16 = $17" & vbCrLf &
                                                "  Pick  7 = $51      Pick 17 = $14" & vbCrLf &
                                                "  Pick  8 = $47      Pick 18 = $11" & vbCrLf &
                                                "  Pick  9 = $43      Pick 19 = $8" & vbCrLf &
                                                "  Pick 10 = $39      Pick 20 = $5" & vbCrLf)
                                            RtbAppendBlank()
                                            RtbAppendHeading("NOTES")
                                            RtbAppendBody(
                                                "  • The bonus is the same whether the first ball, last ball, or both match." & vbCrLf &
                                                "  • Added on top of any regular match payout." & vbCrLf &
                                                "  • Not affected by bet amount, Multiplier Keno, or Powerball." & vbCrLf &
                                                "  • Exempt from the consecutive game bonus multiplier — added flat on top.")
                                        End Sub

        _helpContent("gamelog") = Sub()
                                      RtbAppendTitle("GAME LOG")
                                      RtbAppendBlank()
                                      RtbAppendBody(
                                          "Every game you play is automatically recorded in a plain-text log file " &
                                          "located in the application's Data folder:" & vbCrLf &
                                          "  Data\game-log.txt" & vbCrLf)
                                      RtbAppendBlank()
                                      RtbAppendHeading("SINGLE GAME FORMAT")
                                      RtbAppendBody(
                                          "Each regular or free game produces one pipe-delimited line:" & vbCrLf &
                                          "  Timestamp | Mode | Bet | Match | Multiplier | Payout" & vbCrLf &
                                          "  (Free games append '| Free Game' at the end of the line)" & vbCrLf)
                                      RtbAppendBlank()
                                      RtbAppendHeading("CONSECUTIVE SERIES FORMAT")
                                      RtbAppendBody(
                                          "A consecutive game series is wrapped in a group block:" & vbCrLf &
                                          "  [ N-Consecutive Games" & vbCrLf &
                                          "  ... one line per game (each shows its own Multiplier and Bonus fields) ..." & vbCrLf &
                                          "  Final Payout Calculation:" & vbCrLf &
                                          "    N win(s) subtotal = $X.XX" & vbCrLf &
                                          "    $X.XX × Bx bonus = $Y.YY" & vbCrLf &
                                          "    Total Payout: $Y.YY" & vbCrLf &
                                          "  End Group ]" & vbCrLf)
                                      RtbAppendBlank()
                                      RtbAppendHeading("FIELDS")
                                      RtbAppendBody(
                                          "  Timestamp   — yyyy-MM-dd HH:mm:ss" & vbCrLf &
                                          "  Mode        — Regular N-spot, Bullseye, Way Ticket, Top/Bottom," & vbCrLf &
                                          "               Left/Right, Quadrants, or Free Game (...)" & vbCrLf &
                                          "  Bet         — Wager amount per game" & vbCrLf &
                                          "  Match       — Number of picks matched; N/A for Way Ticket" & vbCrLf &
                                          "  Multiplier  — Keno multiplier drawn (1x if none)" & vbCrLf &
                                          "  Bonus       — Consecutive series bonus (batch lines only)" & vbCrLf &
                                          "  Payout      — Per-game payout (includes multiplier; pre-bonus for batches)")
                                  End Sub
    End Sub

End Class