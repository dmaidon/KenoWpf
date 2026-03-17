# Last Edit: 2026-03-17 - XAML designer errors (XDG0010) and BC40026 warnings fixed.

# Changelog

All notable changes to this project are documented here.  
Format: most-recent first. WPF entries are prefixed **(WPF)**, WinForms entries **(WF)**.

---

## [2026-03-17] — WPF XAML designer error + CLS warning cleanup

- **(WPF) `Application.xaml` — MahApps `ResourceDictionary.MergedDictionaries` removed** — the three pack-URI entries (`Controls.xaml`, `Fonts.xaml`, `Dark.Steel.xaml`) caused persistent XDG0010 XAML designer errors because the VS designer could not resolve `MahApps.Metro.dll` at design time.
- **(WPF) `Application.xaml.vb` — `Application_Startup` added** — calls `ControlzEx.Theming.ThemeManager.Current.ChangeTheme(Current, "Dark.Steel")` so the Dark.Steel theme is applied at runtime instead. Identical visual result; designer is no longer asked to load MahApps resources.
- **(WPF) `Keno.Wpf.vbproj` — `<NoWarn>BC40026</NoWarn>` added** — suppresses the 20 harmless CLS compliance warnings produced because all 10 `MetroWindow`-derived classes are themselves not CLS-compliant (expected for a WPF exe that is not a shared class library).

---

## [2026-03-17] — WPF MahApps.Metro Dark.Steel skin

- **(WPF) MahApps.Metro 2.4.11 installed** — `Keno.Wpf.vbproj` updated via NuGet.
- **(WPF) `Application.xaml` updated** — existing app-wide styles wrapped in a `ResourceDictionary` with `MergedDictionaries`; MahApps `Controls.xaml`, `Fonts.xaml`, and `Dark.Steel` theme loaded.
- **(WPF) All 10 windows converted to `MetroWindow`** — `MainWindow`, `WinBonusGame`, `WinGameLog`, `WinFavoritesSlot`, `WinSessionSummary`, `WinAllTimeSummary`, `WinConsecutiveSummary`, `WinPayoutSchedule`, `WinKenoHelp`, `WinWayTicket`. Each gains `xmlns:mah`, `GlowBrush="{DynamicResource MahApps.Brushes.Accent}"`, and a 30 px height adjustment to preserve content area.

---

## [2026-03-17] — WPF hot/cold persistence, First/Last payout fixes, AllTimeSummary bonus accuracy

- **(WPF) `DrawStatsStore` fully wired** — `RecordDraw` called after every game draw; per-number frequency and win/loss streaks persisted to `Data\draw-stats.json` and restored on launch.
- **(WPF) Hot & Cold status-bar pills** — top-5 hot numbers displayed as red pills in `StatBar2` (`TbkHot1–5`); top-5 cold numbers as blue pills in `StatBar1` (`TbkCold1–5`). `RefreshStatsDisplay` / `UpdateDrawStats` called after each draw to keep both bars live.
- **(WPF) Hot/Cold click-to-toggle** — clicking any hot or cold pill toggles that number on the keno grid, identical to clicking the grid directly; `HotColdNum_Click` handler wired to all 10 labels.
- **(WPF) Streak labels wired** — win/loss/best-streak `StatusBarItem` labels in `StatBar1` read from `DrawStatsStore` and update after every game.
- **(WPF) First/Last Ball payout fix** — `GetFirstLastBallBonus` computation block added inside the game loop; bonus credited to `gamePayout` each game and stored in `gameResults` as `FirstLastBonus`; `LblWinnings` reflects the First/Last contribution immediately.
- **(WPF) First/Last excluded from consecutive multiplier** — bonus is extracted before the series multiplier is applied and re-added flat afterward: `(subtotal − totalFirstLastBonus) × bonus + totalFirstLastBonus`.
- **(WPF) `LblWinnings` running total** — updated after every individual game inside the consecutive loop so the label accumulates live rather than only refreshing after the full series.
- **(WPF) `AllTimeSummaryStore` bonus-accuracy fix** — `RecordGame` moved out of the game loop and called post-series, recording the fully bonus-adjusted payout per game: `adjustedPayout = (r.Payout − r.FirstLastBonus) × bonus + r.FirstLastBonus`. Previously, pre-bonus per-game amounts were recorded, causing the all-time totals to under-count series winnings.

---

## [2026-04-01] — WPF Way Ticket full parity

- **(WPF) `WinWayTicket` — full WinForms feature parity**
  - Window widened to 660 × 640.
  - Row 3 replaced with a 2-column `Grid`: `WrapPanel` in `ScrollViewer` (left) + right panel.
  - **Group Summary** panel — live `ItemsControl` list with group name, number list, and row colour.
  - **Colour Legend** — static swatches: G1 Blue → G2 Green → G3 Salmon → G4 Plum → G5 Yellow → ♛ Goldenrod.
  - **Sub-ticket Preview** panel — live `ItemsControl`; each ST shows numbers + King ♕ suffix when active.
  - `KingBrushColor = Color.FromRgb(218, 165, 32)` (Goldenrod) — king button now renders correctly.
  - `RefreshButton` — king branch uses `KingBrushColor` + chess-queen ♛ (`U+265B`); was using grey + ★.
  - `OnNumberButtonClick` — always-unset-king-first guard added (matches WinForms `ClearKing : Return` pattern).
  - `RefreshGroupList → RefreshSubTicketPreview` chain called on every assignment, toggle, and dialog open.
  - `UpdateSummary` king-status bar: 3-way Goldenrod / DarkGoldenrod / Transparent (was single unstyled text).
  - `ChkKingTicket` label updated to match WinForms: `"♛  King Ticket — designate one number as King"`.
  - OK button renamed **"Play Ways"** with Forest Green background (matching WinForms).
  - `GroupDisplayRow` private nested class added for `DataTemplate` binding (`GroupName`, `NumbersText`, `RowBrush`).

---

## [2026-03-16] — WPF Way Ticket wired in MainWindow

- **(WPF) Way Ticket fully wired in `MainWindow`**
  - `ChkWayTicket_Changed` opens `WinWayTicket.ShowAssignment`; groups stored in `_wayTicketGroups` / `_wayTicketKingNumber`.
  - `GetWayTicketPayout` uses actual groups and king number.
  - `ComputeTotalWager` uses group count for way-ticket cost.
  - `ResetGrid` and `KenoNumber_Click` clear way-ticket state.
  - `UpdateSummary` — full validation: OK disabled until ≥ 2 equal-size groups with all numbers assigned.

---

## [2026-03-15] — WPF WinWayTicket stub + all-time summary store wired

- **(WPF) `WinWayTicket` stub created** — group-assignment picker with ComboBox group selector, King Ticket checkbox, number button grid, summary bar, OK/Cancel.
- **(WPF) `AllTimeSummaryStore` fully wired** — `IncrementSessions`, `RecordGame`, `RecordJackpotWon`, `RecordFreeGameEarned` called from `MainWindow`.
- **(WPF) `SbiSummary` click** → `WinAllTimeSummary.ShowHistory`.
- **(WPF) `MainWindow_Closing`** — on-close session summary dialog shown when ≥ 1 game played this session.

---

## [2026-03-27] — WPF game log, session summary, all-time summary dialogs

- **(WPF) Game log wired** — `AppendGame` / `AppendBatch` called for all game modes (regular, Way Ticket, free games, consecutive), including multiplier, bonus, First/Last Ball, and free-game-awarded fields.
- **(WPF) `WinSessionSummary`** — code-built dialog; net P/L, games played, win rate, best payout, wagered, payout, return %; coloured P/L and return % labels.
- **(WPF) `WinAllTimeSummary`** — code-built dialog; 11 stats from `AllTimeSummaryStore`; win/loss rate, net P/L, return %, jackpots won, free games earned.

---

## [2026-03-26] — WPF Progressive Jackpot, bonus game, payout schedule

- **(WPF) Progressive Jackpot** — `$25,000` seed; 5% of wagers ≥ $5 accumulates in `jackpot.json`; Pick 8+ full-match wins the pool; `WinBonusGame` announces jackpot win; pool resets to seed.
- **(WPF) `WinBonusGame`** — win/bonus popup with 7-second auto-close (mouse-over pauses timer).
- **(WPF) `WinPayoutSchedule`** — live payout table; highlighted row for last match count; footer shows `MARKED: N  HIT: N`; auto-close timer.

---

## [2026-03-25] — WPF remaining dialogs ported

- **(WPF) `WinGameLog`** — scrollable `TextBox` viewer for `Data\game-log.txt`; Refresh and Close buttons.
- **(WPF) `WinFavoritesSlot`** — 3-slot favorites picker; `ShowForSave` / `ShowForLoad` factory methods; empty slots disabled on load.
- **(WPF) `WinConsecutiveSummary`** — `DataGrid` with one row per game (Game #, Bet, Matched, Payout, Result ✓ / —); win rows highlighted Light Goldenrod; summary panel: games won, subtotal, bonus, total.
- **(WPF) `WinKenoHelp`** — `TreeView` + `FlowDocument` help viewer; all 17 topics ported; `StsHelp_Click` wired.

---

## [2026-03-24] — WPF MainWindow core layout and gameplay

- **(WPF) `MainWindow` initial port** — 4-column layout at 942 px: Payout Schedule | Keno Grid | Pick Tray + Controls | Game Play / Stats.
- Full gameplay loop ported: `DrawBallsAsync`, `EvaluateGameAsync`, `PlaySingleGameAsync`, consecutive game loop with per-game multiplier.
- All selection modes ported: regular pick, Bullseye, Quadrant, Top/Bottom, Left/Right, Way Ticket.
- All special-play options: Multiplier Keno, Powerball, First/Last Ball, Quick Pick.
- Hot & Cold panel with click-to-select.
- Bank balance, wager total (live preview), running winnings display.
- Top status bar: Win/Loss streaks, Best streak, Summary, Help, Progressive Jackpot.
- Bottom status bar: Picks, Matches, Payout, Games.
- `Keno.Core` class library extracted — shared by both WPF and WinForms projects.

---

## [2026-03-13] — WinForms: First/Last Ball bonus + free games extended

- **(WF)** Free games extended to Pick 5–9 (previously Pick 5–7). Pick 8 and 9 catch-none on $2+ now awards a free game.
- **(WF)** First/Last Ball Bonus (+$1 side-bet): flat payout if 1st or 20th drawn ball is a player pick. Pick 1 = $75 → Pick 20 = $5. Exempt from consecutive bonus multiplier.
- **(WF)** First/Last Ball hit symbols on consecutive-game progress labels: ♥ (first ball), ♦ (last ball), ♥♦ (both).
- **(WF)** Game log extended: `(Payout, FirstLastBonus)` return tuple; `AppendGame` / `AppendBatch` emit `| FirstLast: $X.XX` when non-zero.
- **(WF)** `FrmKenoHelp` and README updated for all above changes.

## [2026-03-09] — WinForms: multiplier per-game, help viewer, game log expansion

- **(WF)** Multiplier drawn independently for every game in a consecutive series (was drawn once per series).
- **(WF)** `AppendBatch` result tuple expanded: per-game multiplier in each line, `Bonus: Bx` in each batch entry.
- **(WF)** `AppendGame` / `AppendBatch`: `FreeGameAwarded` flag, `FirstLastBonus` field.
- **(WF)** `FrmKenoHelp` updated: consecutive tier table, jackpot win condition, free-games earn rule, favorites count, multiplier weights, new "Game Log" topic.
- **(WF)** Progressive Jackpot seed raised from $500 → $25,000. `GetJackpotBalance` returns seed when no file; `Math.Max(balance, seed)` on read.
- **(WF)** `FrmConsecutiveSummary` result tuple extended with `FirstLastBonus As Decimal`.
- **(WF)** `ApplicationEvents` `UnhandledException` handler added (logs and keeps app alive).
- **(WF)** `ApplicationEvents` reverted `SystemColorMode.System` (hardcoded colours not DarkMode-compatible).

## [2026-03-08] — WinForms: in-app help viewer

- **(WF)** `FrmKenoHelp` — modal help viewer with `SplitContainer`: `TreeView` (17 topics) + `RichTextBox` (content). Opened via `StsHelp_Click`.
- **(WF)** Help content styled with `SelectionFont`/`SelectionColor` lambdas; titles in 13pt navy, headings in 10.5pt teal.
- **(WF)** Topics: Overview, Picking Numbers, Placing a Wager, Playing the Game, Consecutive Games, Quick Pick, Multiplier Keno, Way Ticket, Powerball, Bullseye, Quadrants / Halves, Bank & Winnings, Hot & Cold Statistics, My Favorites, Draw Speed, Progressive Jackpot, Free Games.

## [2026-03-07] — WinForms: all-time history, heatmap removal, game-state enum

- **(WF)** `AllTimeSummaryStore` — persists `SessionsPlayed`, `TotalGamesPlayed`, `TotalWagered`, `TotalPayout`, `TotalWins`, `BestSinglePayout`, `TotalFreeGamesEarned`, `JackpotsWon`.
- **(WF)** `FrmAllTimeSummary` — opened by `StsSummary_Click`; renamed factory `ShowDialog` → `ShowHistory`.
- **(WF)** Heatmap overlay removed from keno grid; `ResetKenoGridHighlights` uses `Color.Transparent`.
- **(WF)** `GameState` enum (`Idle`, `Playing`, `Complete`) replaces `_lastGameComplete` / `_isPlayLocked` boolean pair.
- **(WF)** `InitializeWagerTags` + LINQ `GetSelectedBetAmount` replaces 14-branch `If` chain.
- **(WF)** `PlaySingleGameAsync` decomposed into `DrawBallsAsync`, `EvaluateGameAsync`, `PlaySingleGameAsync`.
- **(WF)** Hot/Cold click-to-select; draw-speed persistence; session total wagered in session summary.
- **(WF)** 3-slot favorites; jackpot display order fixed.

## [2026-03-06] — WinForms: progressive jackpot, streaks, session summary

- **(WF)** Progressive Jackpot — 5% of wagers ≥ $5 → `jackpot.json`; Pick 8+ full-match wins; resets to $0.
- **(WF)** Streak tracking — `CurrentWinStreak`, `CurrentLossStreak`, `BestWinStreak` in `draw-stats.json`.
- **(WF)** `SsTop` status strip — streak labels (green/red) + best streak + `StsSummary` + `StsProgressiveJackpot`.
- **(WF)** `FrmSessionSummary` — auto-shown on close after ≥ 1 game; net P/L, win rate, best payout, free games, wagered, payout, return %.
- **(WF)** Between-game pause tied to draw speed (Slow 5 s, Medium 3.5 s, Fast 2 s).
- **(WF)** Auto-close win/bonus popups after 7 seconds (mouse-over pauses).
- **(WF)** Starting bank balance raised $1,000 → $10,000. Consecutive games extended to 10. Powerball extended to Pick 6–20. Picks extended to 1–20. Wager radio buttons added ($25–$200). Custom wager NUD added.

## [2026-03-05] — WinForms: Bullseye, Powerball real draw, special-play GroupBox

- **(WF)** `GbxSpecialPlay` groupbox: merged Multiplier, Way Ticket, Powerball, Quick Pick, Bullseye.
- **(WF)** `BtnBullseye` — fixed 8-spot pattern (corners 1,10,71,80 + centre 35,36,45,46); own payout schedule (8=30000x … 0=10x).
- **(WF)** Powerball draws real 21st ball from unused 60; ×4 multiplier only when Powerball hits a pick.
- **(WF)** `FrmWayTicket` — sub-ticket preview `ListView` added; OK/Cancel shifted; group `ListView` colour-coded.
- **(WF)** `FrmPayoutSchedule` — live RTB payout table; HITS/WIN columns; MARKED/HIT footer; gold highlighted row.

- **Free games extended to Pick 5–9**: `IsFreeGameBonus` upper bound raised from Pick 7 to Pick 9. Pick 8 and 9 have no 0-match consolation payout, so a catch-none result on those spot counts now awards one free game on bets of $2+, same as Pick 5–7. `FrmKenoHelp` and README updated accordingly.
- **First/Last Ball — exempt from consecutive bonus multiplier**:
- **Payout Schedule — First/Last Ball row**:
- **First/Last Ball Play — $1 add-on**: `ChkFirstLastPlay` enables the First/Last Ball bonus. Cost is $1 per game; `GetFirstLastPlayCost()` added alongside `GetMultiplierCost`/`GetPowerballCost` and included in `GetTotalBet`. `EvaluateGameAsync` bonus block now guarded by `ChkFirstLastPlay.Checked` — unchecked means no bonus and no charge. Checkbox is reset after each play (same pattern as Multiplier/Powerball). Tooltip added.
- **First/Last Ball Bonus table calibrated**: Pick 1 = $75 (max) → Pick 20 = $5 (min). $70 range over 19 steps (~$3.68): $4 decrements for picks 1–14 (13 steps), $3 decrements for picks 14–20 (6 steps) — the only whole-dollar solution. Flat dollar award; not scaled by bet, multiplier, or Powerball.
- **LblGamePlay hit symbols**: when the first or last drawn ball is a player pick, the corresponding consecutive-game label shows ♥ (Crimson, first ball) or ♦ (RoyalBlue, last ball); both hitting shows ♥♦ (DarkMagenta). Labels reset to their game number before each new play.
- **Game log — First/Last Ball Bonus transparency**: `EvaluateGameAsync` now returns `(Payout, FirstLastBonus)` instead of a bare `Decimal`; `PlaySingleGameAsync` propagates `FirstLastBonus` in its result tuple. `AppendGame` gains an `Optional firstLastBonus As Decimal = 0D` parameter and emits `| FirstLast: $X.XX` when non-zero; `AppendBatch` receives the field per-game and appends the same annotation to each batch line. `FrmConsecutiveSummary` result tuple extended with `FirstLastBonus As Decimal` (signature-only; no display change needed).
table scales from $75 (Pick 1) down to $5 (Pick 20)
- **Consecutive game bonus — maximum capped at 1.75×**:
- **Multiplier Keno — 1× weight raised to 45%**
- **Multiplier Keno — reverted to NC Lottery distribution (1× re-enabled)**
- **Multiplier Keno — minimum draw raised to 2×**
- **`FrmSessionSummary` — form font reduced to 9pt**:
- **`FrmAllTimeSummary` — form and value-label font reduced to 9pt**: form-level `Font` changed from `New Font("Segoe UI", 10)` to `New Font("Segoe UI", 9)`; `AddRow` value label font changed from `New Font("Segoe UI", 10, FontStyle.Bold)` to `New Font("Segoe UI", 9, FontStyle.Bold)` to match.
- **Docs — README and Help updated for live wager preview and running winnings**:
- **Wager Total display — live preview (`LblWagerTotal`)**: `UpdateWagerTotalDisplay(totalBet)` is now called at the end of `UpdatePlayState()`, so `LblWagerTotal` reflects the projected wager as soon as the player selects picks, a bet amount, consecutive count, or multiplier — before `BtnPlay` is clicked. `totalBet` is already computed in `UpdatePlayState` for button-enable logic, so no extra calculation is added. `UpdatePlayState` is wired to every relevant change event (`RbWager_CheckedChanged`, `NudSpecialWager_ValueChanged`, `ChkMultiplierKeno_CheckedChanged`, `RbConsecutive_CheckedChanged`, `KenoNumber_Click`, etc.), so the label stays in sync automatically.
- **Wager Total display (`LblWagerTotal`)**: `UpdateWagerTotalDisplay(amount)` helper added. Called in `BtnPlay_Click` immediately after `DeductBetFromBank` so `LblWagerTotal` reflects the full amount wagered for that play (including consecutive multiplier and Way Ticket sub-tickets). Reset to `$0` by `BtnClear_Click`.
- **Running winnings during consecutive games (`LblWinningsValue`)**: `UpdateWinningsDisplay(totalPayout)` is now called after every individual game result is added to `totalPayout` inside the consecutive loop, so the label accumulates the running total as each game completes rather than only updating after the full set finishes.
- **Payout Schedule Panel — Live RTB display** (`FrmMain.PayoutSchedulePanel`, new): `RtbPayoutSchedule` inside `GbxPayoutSchedule` now shows a live payout schedule for the current pick. Header columns: **HITS** (left-justified) and **WIN** (right-justified), scaled by the active bet amount. Footer line: `MARKED: N  HIT: N`. The schedule refreshes on number selection, bet change, game result, and Clear. The row matching `_lastMatchedCount` is highlighted in gold when a game is in progress or complete. GroupBox title updates to reflect current mode (e.g. `Payout — Pick 5`). Fixed: `_lastMatchedCount` was never assigned in `EvaluateGameAsync`, so `StsMatches` always showed 0; now correctly reflects the matched count after each draw.
- **`ApplicationEvents` — reverted `SystemColorMode.System`**: `e.ColorMode = SystemColorMode.System` removed. The app uses many hardcoded colors (`Color.SteelBlue`, `Color.Gold`, `Color.WhiteSmoke`, etc.) that do not adapt when Windows DarkMode is active, producing an unusable black UI. DarkMode support requires auditing and theming all owner-drawn colors; tracked for a future session.

## [2026-03-09]
- **`ApplicationEvents` — `ApplyApplicationDefaults`**: added handler setting `e.ColorMode = SystemColorMode.System` (system DarkMode support) and `e.HighDpiMode = HighDpiMode.SystemAware`. This is the correct VB App Framework location for these settings; no `Program.vb` exists in VB and the manifest must not carry DPI settings.
- **`ApplicationEvents` — `UnhandledException`**: added last-resort handler that calls `LogError` and sets `e.ExitApplication = False`, keeping the app alive for recoverable UI-thread exceptions that slip past per-handler `Try/Catch` blocks.
- **`ApplicationEvents` — cleanup**: removed 20-line Visual Studio template comment block; consolidated blank `Imports` lines.
- **Game Log Viewer**
- **Multiplier Keno — per-game redraw in consecutive play**: The multiplier is now drawn independently at the start of every individual game in a consecutive series instead of once at the start of the whole series. Each game's `gameResults` tuple carries its own `Multiplier As Integer` so the value actually used for that game's payout is captured and logged correctly.
- **Game Log — per-game multiplier in batch entries**: `AppendBatch` results tuple expanded with `Multiplier As Integer`; the old single shared `Optional multiplier As Integer = 1` parameter removed. Each game line in a consecutive block now logs that game's actual drawn multiplier. "Final Payout Calculation" block simplified — per-game multipliers are already baked into each game's payout, so the summary shows win subtotal and bonus factor only (format: `N win(s) subtotal = $X.XX` → `$X.XX × Bx bonus = $Y.YY`). `FrmConsecutiveSummary` signature updated to accept the wider tuple.
- **Multiplier display — LblMultiplierValue removed**:
- **Help — 2026-03-09 updates**: `FrmKenoHelp` updated to reflect all changes made today. `"consecutive"` topic: range extended to 2–20 games; bonus table updated to all four tiers (1.5×/2×/2.5×/3×). `"progressive"` topic: jackpot win condition corrected to "match ALL picks, minimum Pick 8"; reset value updated to $25,000. `"freegames"` topic: earning rule corrected — awards one free game when Pick 5–7 matches 0 numbers on a $2+ wager. `"favorites"` topic: slot count corrected from 5 to 3. `"multiplier"` topic: added x1 (40%) to the possible multiplier values and weighted odds. **New topic** `"Game Log"` added to both `BuildTopicsTree` arrays and `BuildHelpContent`: explains `Data\game-log.txt` location, single-game line format, consecutive group block format, and all field definitions.
- **Game Log — Free Game Awarded**: `AppendGame` gains `Optional freeGameAwarded As Boolean = False`; when true, ` | Free Game Awarded` is appended to the line. `AppendBatch` result tuple expanded from `(Matched, Payout)` to `(Matched, Payout, FreeGameAwarded)`; each line annotated the same way. `BtnPlay_Click` adds a `freeGameIndices As HashSet(Of Integer)` alongside `gameResults`; the index is added to the set when `isFreeGame` is true. Single-game `AppendGame` call passes `freeGameIndices.Count > 0`; batch `AppendBatch` projects `freeGameIndices.Contains(r.Index)` per game.
- **Game Log — Consecutive bonus tracking**: `AppendBatch` gains `Optional bonus As Decimal = 1D`; each batch log line now includes `Bonus: {0.##}x` between Multiplier and Payout. `BtnPlay_Click` passes `bonus` to the `AppendBatch` call.
- **Progressive Jackpot — $25,000 seed**: `JackpotSeed` raised from `$500` to `$25,000`. `GetJackpotBalance` now returns `JackpotSeed` (instead of `0`) when no file exists, and uses `Math.Max(data.Balance, JackpotSeed)` so existing low-balance files are silently bumped to the seed floor on next read. `EnsureJackpot` added to `ProgressiveJackpotStore` and called from `FrmMain_Load` after `EnsureAllTimeSummary`.
- **Game Log — Multiplier tracking**: `AppendGame` and `AppendBatch` now accept an `Optional multiplier As Integer = 1` parameter; each log line includes a `Multiplier: Nx` field between Matched and Payout. `BtnPlay_Click` passes `_multiplierValue` to all three log call sites. `BtnFreeGames_Click` omits the argument (always 1, set explicitly at handler entry).
- **Game History Log** (`GameLogStore`, new): every game played is now appended to `Data/game-log.txt`. Each line contains timestamp, game mode, bet, matched count, and payout in pipe-delimited format. `AppendGame` handles single games (Way Ticket and free games use matched = N/A where applicable); `AppendBatch` handles consecutive series in one write. `GetGameModeLabel()` helper added to `FrmMain.KenoSelection` — returns `"Regular N-spot"`, `"Bullseye"`, `"Way Ticket"`, `"Top/Bottom"`, `"Left/Right"`, or `"Quadrants"` based on current selection state. `BtnPlay_Click` captures the label before play, then routes to `AppendGame` or `AppendBatch` after `ApplyTotalWinnings`. `BtnFreeGames_Click` logs with a `"Free Game (...)"` prefix.
- **Session Summary — Total Payout**: added a new "Total Payout" row (`totalPaidOut = totalWagered + netPL`) between *Total Wagered* and *Return %*. `FrmSessionSummary`: new `_lblTotalPayoutValue` field; `RowCount` 8 → 9; `BtnClose` shifted to row 8; form height 420 → 460; `Populate` sets the label from `totalPaidOut`.
- **Consecutive Series Summary** (`FrmConsecutiveSummary`, new): code-built dialog shown automatically after every consecutive game series of 2+ games. Displays a `ListView` with one row per game (Game #, Bet, Matched, Payout, Result — ✓ Win or —) with win rows highlighted in light goldenrod. Below the list, a summary panel shows Games Won, Subtotal (before bonus), Bonus Multiplier, and Total Payout (green when positive). `BtnPlay_Click` now collects `gameResults As List(Of (Index, Matched, Payout))` during the loop and captures `subtotalBeforeBonus` before the bonus multiplication; the dialog is shown after `ApplyTotalWinnings` when `gamesToPlay > 1`.
- **Consecutive games extended to 20**: added `RbConsecutive11`–`RbConsecutive20` radio buttons and `LblGamePlay11`–`LblGamePlay20` progress indicators. `GetConsecutiveGamesCount`, `RbConsecutive_CheckedChanged` Handles clause, `ResetConsecutiveSelection`, and `_gamePlayLabels` all updated.
- **4-tier consecutive bonus**: bonus tiers are now 5–7 games = 1.5×, 8–11 = 2×, 12–16 = 2.5×, 17–20 = 3×. `GetConsecutiveBonus` and `UpdateBonusMultiplierDisplay` updated; `LblBonusMultiplier` tooltip reflects all four tiers.
- **Draw match symbols**: matched numbers on the keno grid now display a symbol instead of only a colour change — ✓ (U+2713) for regular play, ◎ (U+25CE) for Bullseye, ♦ (U+2666) for Way Ticket. `GetMatchSymbol()` helper added; symbol is applied in `DrawBallsAsync` alongside the existing `MatchDrawColor` background. `ResetKenoGridHighlights` already restores label text to the number so no additional reset logic is needed.
- **Split keno grid**: `TlpKenoGrid` replaced by `TlpKenoGridTop` (numbers 1–40) and `TlpKenoGridBottom` (numbers 41–80). `InitializeKenoCaches` now iterates both panels via `For Each tlp In {TlpKenoGridTop, TlpKenoGridBottom}`; all other methods continue to operate through `_kenoLabels` unchanged.
- **Session Summary — Total Wagered & Return %**: added two new rows to `FrmSessionSummary`. `Populate()` now accepts `totalWagered As Decimal`; computes `returnPct = totalPaidOut / totalWagered`; Return % label is coloured green when ≥ 100% and red when below. `RowCount` extended from 6 to 8; Close button renamed `_btnClose` → `BtnClose`.
- **FrmFavoritesSlot** (new): code-built slot picker dialog used by Save / Load favorites. Shows all 3 slots with number count and optional slot name. `ShowForSave(owner, numberCount)` disables no slots; `ShowForLoad(owner)` disables empty slots. Returns the chosen slot index or −1 on cancel.
- **FavoritesStore — multi-slot favorites**: expanded from a single `Numbers` array to a `FavoritesSlot()` array with `SlotCount = 3` independent slots. New helpers: `LoadFavorites(slotIndex)`, `GetSlotCount(slotIndex)`, `HasFavorites()`, `GetAllSlots()`. Backward-compatible: existing single-slot `favorites.json` is auto-migrated to slot 0 on first load.
- **FrmAllTimeSummary**: renamed factory method `ShowDialog` → `ShowHistory` to avoid shadowing `Form.ShowDialog`; `StsSummary_Click` in `FrmMain.KenoSelection` updated accordingly.
- **FrmWayTicket — sub-ticket preview**: added `LvSubTickets` `ListView` (Details view, 2 columns: *Sub-ticket*, *Numbers*) below the group list. `RefreshSubTicketPreview()` rebuilds the list after every group assignment, showing each sub-ticket's numbers with its group background colour and the King number (♕) appended where active. OK/Cancel buttons and summary bar shifted down to accommodate the new panel.
- **ProgressiveJackpot seed**: `ResetJackpot()` now seeds the pool to **$500** instead of $0; `JackpotSeed As Decimal = 500D` constant added to `ProgressiveJackpotStore`.
- **AppSettingsStore**: added `DrawSpeedIndex As Integer` property (0 = Slow, 1 = Medium, 2 = Fast) to the settings model for persistence across sessions.
- **Timer naming**: renamed `_autoCloseTimer` → `AutoCloseTimer` in both `FrmPayoutSchedule` and `FrmBonusGame` to follow VB `WithEvents` naming conventions.
- **FrmMain.Designer**: renamed `RbWager45` → `RbWager50`; removed dead `LblMultiplierCaption` field; added `StsFiller`, `StsHelp`, `StsSummary`, `StsProgressiveJackpot` to `SsTop` status strip initialisation.

## [2026-03-08]
- **In-App Help Viewer**: added `FrmKenoHelp` — a modal help form opened by clicking the **Help** button (`StsHelp`) in the top status bar. The form displays a two-panel layout via a `SplitContainer`: a `TreeView` on the left lists 17 help topics and a read-only `RichTextBox` on the right shows the selected topic's content.
- Help topics covered: Overview, Picking Numbers, Placing a Wager, Playing the Game, Consecutive Games, Quick Pick, Multiplier Keno, Way Ticket, Powerball, Bullseye, Quadrants / Halves, Bank & Winnings, Hot & Cold Statistics, My Favorites, Draw Speed, Progressive Jackpot, Free Games.
- `BtnClose` at the bottom of `FrmKenoHelp` closes the viewer; the first topic (Overview) is selected automatically on open.
- `FrmMain.StsHelp_Click` opens `FrmKenoHelp` as a modal dialog and disposes it on close.
- **Help content formatting**: topic titles render in 13pt bold navy (`Color.FromArgb(25, 70, 140)`); section headings render in 10.5pt bold teal (`Color.FromArgb(0, 110, 155)`); body text renders in near-black. `_helpContent` dictionary changed from `Dictionary(Of String, String)` to `Dictionary(Of String, Action)` — each entry is a lambda that writes directly to the `RichTextBox` using `SelectionFont`/`SelectionColor`/`SelectedText`. `_fntTitle` and `_fntHeading` are class-level fields initialised in `Load` and disposed in `FormClosed`.

## [2026-03-07]
- **All-Time Play History**: added `AllTimeSummaryStore` (persists to `Data/all-time-summary.json`) tracking `SessionsPlayed`, `TotalGamesPlayed`, `TotalWagered`, `TotalPayout`, `TotalWins`, `BestSinglePayout`, `TotalFreeGamesEarned`, and `JackpotsWon` across all app runs. Updated on every game (`RecordGame`), free-game award (`RecordFreeGameEarned`), and jackpot win (`RecordJackpotWon`); session count incremented on `FrmMain_Load`.
- **FrmAllTimeSummary**: code-built dialog opened by clicking the **Play History** label (`StsSummary`) in the top status bar. Displays 11 stats including wins/losses, win rate, net P/L, return %, and jackpots won. Net P/L and Return % are coloured green when positive, red when negative.
- **Fix: heatmap overlay removed from keno grid** — `ResetKenoGridHighlights` and `UpdateLabelSelectionVisual` now use `Color.Transparent` so every non-picked, non-drawn label inherits the grid panel's `(192, 255, 255)` background. Previously the heatmap tints bled into the reset state, making the grid appear to not clear between games. Removed `_heatmapColors` field, `UpdateHeatmapColors`, and `ComputeHeatmapColor`. Hot/cold side-panel labels (`LblHot1–5`, `LblCold1–5`) are unaffected — frequency tracking continues via `BuildWindowCounts`.
- **Grid color contract after each game reset:**
  - Unselected / undrawn squares → `(192, 255, 255)` *(grid background)*
  - User's picks → 🟩 LightGreen
  - Drawn but not picked → 🔷 LightSkyBlue
  - Matched (drawn + picked) → 🔵 RoyalBlue / Gold text
- **#16 GameState enum**: replaced `_lastGameComplete`/`_isPlayLocked` boolean pair with a single `GameState` enum (`Idle`, `Playing`, `Complete`). All play-state guards and button-enable logic now read `_gameState` directly; no inconsistent boolean combinations possible.
- **#17 Wager RadioButton.Tag / LINQ**: added `InitializeWagerTags` (called at startup) that stamps each of the 14 wager `RadioButton`s with its `Decimal` amount as `Tag`. `GetSelectedBetAmount` is now a 4-line LINQ `FirstOrDefault` expression — the 14-branch `If` chain is gone.
- **#18 PlaySingleGameAsync split**: decomposed the monolithic 60-line method into three focused methods:
  - `DrawBallsAsync(picks, quadrantMode)` — animates the draw loop, highlights matches, returns `matchedCount`.
  - `EvaluateGameAsync(betAmount, matchedCount, picks, quadrantMode)` — resolves Powerball, calculates payout, updates stats, checks jackpot, returns final `payout`.
  - `PlaySingleGameAsync` — thin 5-line orchestrator that resets displays, draws picks, calls the two above, and returns the result tuple.
- Added **hot/cold click-to-select**: clicking any label in the Hot or Cold panel toggles that number on the grid, identical to clicking the grid directly.
- Added **draw speed persistence**: selected draw speed (Slow/Medium/Fast) is saved to `app-settings.json` and restored on next launch.
- Added **session total wagered** to Session Summary dialog.
- Fixed **jackpot display order**: jackpot is now checked and potentially awarded before the payout popup so the popup reflects the full jackpot-inclusive amount.
- Expanded **favorites to 3 slots**: `BtnSaveFavorites` and `BtnPlayFavorites` now prompt with `FrmFavoritesSlot` to choose slot 1, 2, or 3; all three slots persist independently in `favorites.json`.
- Added error logging: unhandled exceptions are logged to daily log files in `Logs/`.

## [2026-03-06]
- Added **Progressive Jackpot**: 5% of every wager ≥ $5 contributes to a shared jackpot pool persisted in `jackpot.json`. Matching **all** picked numbers on a **Pick 8 or greater** game wins the entire jackpot; the pool then resets to $0. The current jackpot total is displayed live in the `StsProgressiveJackpot` status label using the tag template.
- Added **streak tracking**: `CurrentWinStreak`, `CurrentLossStreak`, and `BestWinStreak` persisted in `draw-stats.json`. `RecordDraw` now accepts an optional `won` flag and updates all three counters each game.
- Added `SsTop` status strip above the bottom bar; displays current win/loss streak (green for win, red for loss) and all-time best win streak via `StsCurrentStreak` and `StsBestStreak`.
- Added **Session Summary** dialog (`FrmSessionSummary`): shown automatically on app close (when at least 1 game was played this session). Displays net profit/loss, games played, win rate, best single payout, and free games earned.
- Session data tracked entirely in memory: start balance captured at launch; wins, best payout, and free games earned accumulated per game.
- Fixed `FrmMain_FormClosing`: previously declared with no parameters and no `Handles` clause so it was never called; corrected to `(sender, e As FormClosingEventArgs) Handles MyBase.FormClosing`.
- Between-game pause now tied to **draw speed**: Slow = 5,000 ms, Medium = 3,500 ms, Fast = 2,000 ms (previously always 5,000 ms). Way Ticket inter-sub-ticket delay uses the same setting (previously hardcoded 3,000 ms).
- **Auto-close win/bonus popups**: `FrmPayoutSchedule` and `FrmBonusGame` now close automatically after **7 seconds** if the mouse cursor is not hovering over the form. Manual close stops the timer; timer is properly disposed.
- Default starting bank balance raised from **$1,000** to **$10,000**.
- Extended consecutive games to **10** games; added `RbConsecutive8`–`10` radio buttons and `LblGamePlay8`–`10` game-play indicators; all wired into `GetConsecutiveGamesCount`, `RbConsecutive_CheckedChanged`, `ResetConsecutiveSelection`, and `_gamePlayLabels`.
- Powerball Play availability extended from Pick 6–10 to **Pick 6–20**; validation guard, `UpdatePlayState` check, and tooltip updated.
- `NudSpecialWager.Maximum` is now capped to the current bank balance in `UpdateBankDisplay` so the custom wager can never exceed available funds; value is clamped automatically on each bank update.
- `NudQuickPick` and `NudSpecialWager` now auto-select their text when focused (`Nud_Enter` handler) so the user can immediately type a new value.
- Extended regular pick count from 1–10 to **1–20** numbers per game.
- Added `NumSelect11`–`NumSelect20` display boxes (second row of `TlpNumbersPicked`) for picks 11–20.
- Added payout schedules for Pick 11 through Pick 20; higher picks carry progressively larger jackpots (up to 1,000,000x) and increasing catch-none bonuses.
- Added wager radio buttons: **$25, $30, $40, $50, $100, $150, $200**.
- Added `NudSpecialWager`: a numeric up-down that accepts any custom wager from $1 to $1,000. Selecting a preset radio button resets it to 0; entering a value unchecks all preset radios. Maximum is capped to the current bank balance in real time.
- `UpdateBankDisplay` now synchronises `NudSpecialWager.Maximum` on every bank update so the custom wager can never exceed available funds.
- `ResetBetSelection` now clears all 14 wager radio buttons and resets `NudSpecialWager` to 0.
- Quick Pick (`NudQuickPick`) maximum raised from 10 to **20** to match the new pick limit.
- `BtnPlayFavorites` now restores up to 20 saved numbers (previously capped at 10).


## [2026-03-05]
- Merged Multiplier, Way Ticket, Powerball, and Quick Pick controls into single GbxSpecialPlay GroupBox.
- GbxQuickPick renamed to GbxSpecialPlay throughout designer and code.
- Added BtnBullseye (inside GbxSpecialPlay): selects the 4 corners (1,10,71,80) + 4 center cells (35,36,45,46); button turns Gold while active, resets to MistyRose on Clear/QuickPick/manual pick.
- Added Bullseye payout schedule: 8=30000x, 7=500x, 6=75x, 5=15x, 4=3x, 0=10x (catch-none bonus).
- Bullseye payout is separate from regular Pick 8; GetGamePayout routes through _isBullseyeActive flag.
- FrmPayoutSchedule handles Bullseye game type with dedicated title row.
- Powerball now draws a real 21st ball from the 60 numbers not in the regular draw; shown in OrangeRed on the keno grid and in LblPowerballValue inside GbxPowerball.
- x4 multiplier now only applies when the Powerball lands on one of the player's picks (previously always x4 on any win).
- LblPowerballValue shows "—" before play, the drawn number during/after play; OrangeRed normally, DarkOrange when it hits a pick.
- GbxPowerball resized to 82px to accommodate the new label.
- Added King Ticket to Way Ticket setup (FrmWayTicket): check 'King Ticket' to enter King designation mode, then click one number to crown it ♛.
- King number is shown in Goldenrod with a ♛ symbol; clicking it again unsets it.
- King is automatically added to every sub-ticket group (e.g., 2 groups of 2 + King = 3-spot sub-tickets each).
- Group list view shows a Goldenrod ♛ King row at the top; legend panel shows King color entry.
- Summary bar and LblWayTicketSummary reflect the actual spot count (group size + 1) and show ♛ marker.
- King Ticket mode is optional; Way Ticket without King enabled works exactly as before.
- Added Way Ticket (GbxWayTicket / ChkWayTicket): player selects numbers, checks Way Ticket, then assigns numbers to equal-size groups (min 2, max 5) in the FrmWayTicket dialog; each group plays as a separate Pick-M sub-ticket in sequence.
- FrmWayTicket is fully code-built: clickable number buttons cycle through group colors (G1-G5), group list view, real-time validation (equal size + min 2 groups), cost summary; OK enabled only when configuration is valid.
- LblWayTicketSummary on FrmMain shows "N sub-tickets (M-spot) - $X total"; updates when bet changes; shows sub-ticket progress during play; shows "Done - Total: $X" after.
- Way ticket total (groups x bet, + multiplier if active) deducted upfront; auto-unchecks after play; BtnClear resets way ticket.
- Added Multiplier Keno (GbxMultiplier): costs $1 extra per game; multiplier (x1–x5, uniform random) is drawn once when Play is clicked and applies to all games in the run.
- x1 = normal payout, x2–x5 multiply any win.
- Multiplier is color-coded on LblMultiplierValue: Transparent/PaleGreen/LightSalmon/Orange/Gold for x1–x5.
- ChkMultiplierKeno unchecks automatically after play completes; free games always use x1.

- Added BtnQuickPick and NudQuickPick: player sets a count (1–10) and clicks Quick Pick to randomly select that many numbers.
- Quick Pick clears any existing number or quadrant selection before applying the new picks.
- Fixed BtnFreeGames text color: FlatStyle set to Flat so ForeColor (White) is respected even when button is disabled.

## [2026-03-04]
- Added payout schedule popup on each win; player must close it before the next game starts.
- Winning games skip the 5-second inter-game delay; the popup serves as the pause.
- Added Pick 5/6/7 bonus: 0 matches with a $2+ bet awards one free $2 game; popup notifies the player.
- Bonus popup also skips the 5-second inter-game delay.
- Free game bonus requires a $2 or higher bet; $1 bets are ineligible.
- Playing a free game cannot award another free game.
- BtnFreeGames enabled whenever free games are available, regardless of current selection.
- BtnFreeGames ForeColor set to White for legibility on ForestGreen background.
- Added free game count persistence via free-games.json.
- Added tooltips to BtnPlay, BtnClear, BtnFreeGames, GbxBet, GbxConsecutiveGames, and TlpQuadrants.
- Rewrote README as a full player instruction manual with payout tables and game type descriptions.

## [2026-02-22]
- Added quadrant/half selection with separate payout schedules.
- Added consecutive game play with per-game indicators, delays, and accumulated winnings.
- Added settings storage for window location.

## [2026-02-20]
- Added bank settings persistence and payout schedule storage.
- Added animated draws, match highlighting, and winnings display.
- Added status strip metrics for picks, matches, payout, and games played.
- Added hot/cold number tracking with persisted draw stats.
- Hot/cold numbers now use the last 15 games after 20 total games.
- Draws now use a cryptographic shuffle for improved randomness.
- Cached JSON serializer options for settings and draw stats.
- Added error logging with daily log files and status strip indicator.
