# Last Edit: Apr 10, 2026 15:15 - Clarify multiplier pricing examples in help and README.

# Changelog — Keno.Android (MAUI)

All notable changes to the MAUI / mobile project are documented here.  
Format: most-recent first. Entries are prefixed **(MAUI)**.  
See the [root CHANGELOG](../CHANGELOG.md) for WPF and WinForms history.

---

## [2026-04-10] — Documentation/help clarification pass

- **(MAUI) `HelpPage.xaml.cs` — multiplier fee example** — side-bet help text now includes a concrete example (`$200` base wager = `$6.00` multiplier fee) for easier understanding of the new pricing rule.
- **(MAUI) `README.md` — net-win guidance note** — added concise guidance that `2×` (or higher) multiplier outcomes at `$200` base wager generally improve winning outcomes after the multiplier fee, depending on base payout.

---

## [2026-04-10] — Multiplier side-bet pricing update

- **(MAUI) `MainPage.xaml.cs` — dynamic multiplier fee** — changed per-game multiplier surcharge from a flat `$1` to `max($1.00, 3% of base wager)` (rounded to cents); this fee now feeds `EffectiveWager`, bank-cap clamping, and top-bar wager totals.
- **(MAUI) `MainPage.xaml.cs` — side-bet info refresh** — multiplier info label now displays the current computed dollar fee and updates immediately when wager changes (preset or custom).
- **(MAUI) `HelpPage.xaml.cs` — help text alignment** — updated side-bet documentation to match the new multiplier pricing rule.

---

## [2026-04-10] — Menu/history display reliability improvements

- **(MAUI) `MainPage.xaml.cs` — menu navigation hardening** — `BtnMenu_Clicked` now prevents re-entry while busy, briefly delays after `DisplayActionSheetAsync` so Android dialog teardown completes, and wraps modal push in guarded exception handling.
- **(MAUI) `HistoryPage.xaml.cs` — session list render cap** — limits rendered session rows to the latest 200 with a footer note when truncated, preventing large-session UI stalls when opening Game History.

---

## [2026-04-10] — Android launch crash fix (MaterialComponents)

- **(MAUI) `Platforms/Android/Resources/values/styles.xml` — new theme resource file** — added explicit `Maui.MainTheme` and `Maui.SplashTheme` definitions inheriting from MAUI base themes, including `postSplashScreenTheme`, to ensure Android controls receive a valid Material text appearance context.
- **(MAUI) `Platforms/Android/Resources/values/colors.xml` — splash color resource** — added `maui_splash_color` used by the splash theme so theme inflation resolves all required resources at startup.

---

## [2026-03-25] — Payout Schedule Sheet, In-App Help

- **(MAUI) `PayoutSchedulePage.xaml/cs` — new modal page** — accessible from the hamburger menu (☰ → Payout Schedule); interactive pick-count selector (1–20) in a horizontal strip; per-pick payout table with CATCH / PAYS / AT $5 columns built from `KenoPayouts.GetPayoutScheduleEntries()`; rows sorted highest-catch first with 0-catch special entry at the bottom; multipliers ≥ ×1,000 shown in gold, lower wins in green.
- **(MAUI) `HelpPage.xaml/cs` — new modal page** — accessible from the hamburger menu (☰ → How to Play); 10 scrollable sections: The Board, Wager, Quick Pick, PLAY/REPLAY/CLEAR, Consecutive Games (with bonus tier table), Side Bets, Draw Speed, Favorites, Bank & Replenish, Reading the Top Bar.
- **(MAUI) `MainPage.xaml.cs` — hamburger menu** — `BtnMenu_Clicked` now opens a `DisplayActionSheet` with three options: **Game History**, **Payout Schedule**, **How to Play**.

---

## [2026-03-25] — Persistent Bank & Settings, Replenish Button, Custom Wager

- **(MAUI) `MainPage.xaml.cs` — persistent bank balance** — `_bankBalance` is saved to `Preferences.Default` (key `bank_balance`) in `UpdateStatus()` — every wager deduction and payout credit is immediately durable across app restarts.
- **(MAUI) `MainPage.xaml.cs` — persistent settings** — `LoadSettings()` restores wager, side-bet toggles (Multiplier / Powerball / First-Last), Quick Pick count, draw speed, and consecutive games count from `Preferences.Default` on every launch. Each handler saves its own key on change.
- **(MAUI) `MainPage.xaml` — REPLENISH BANK button** — full-width green button (`BtnReplenish`) added as a third row inside the play section; `IsVisible = false` by default; appears automatically when `_bankBalance ≤ $0`. Offers three action-sheet options: **+$1,000** / **+$5,000** / **Reset to $10,000**.
- **(MAUI) `MainPage.xaml.cs` — CUSTOM wager button** — added at the end of the horizontal wager strip; tapping opens `DisplayPromptAsync`; any amount ≥ $0.01 is accepted; the button label updates to show the entered amount; selection and bank-cap clamping behave identically to preset buttons.

---

## [2026-03-25] — Consecutive Games, Bank Cap, Play Speeds

- **(MAUI) `MainPage.xaml` — GAMES row** — stepper (`−`/count/`+`) placed above the Quick Pick row; count ranges 1–20; a `LblBonusDisplay` label to the right shows the active consecutive bonus (×1.1 / ×1.25 / ×1.5 / ×1.75) when applicable.
- **(MAUI) `MainPage.xaml` — SPEED row** — four pill-buttons (SLOW / MED / FAST / SS) placed above PLAY/REPLAY/CLEAR; active pill highlights in teal (#2196A6); default is **FAST** (200 ms/ball).
- **(MAUI) `MainPage.xaml.cs` — multi-game loop in `PlayGameAsync`** — deducts `EffectiveWager × gamesToPlay` from bank up front; runs up to 20 consecutive games; accumulates payout + First/Last bonus separately; applies the consecutive bonus formula `(totalPayout − totalFirstLastBonus) × bonus + totalFirstLastBonus` at the very end (mirrors WPF).
- **(MAUI) `MainPage.xaml.cs` — bank cap / auto-clamp** — `ClampGamesToBalance()` called on wager change, Multiplier toggle, First/Last toggle, and Games `+` press; reduces `_consecutiveGames` so `EffectiveWager × games ≤ bank`; PLAY also re-clamps in case of any race.
- **(MAUI) `MainPage.xaml.cs` — `GetConsecutiveBonus()`** — returns ×1.0 (1–4 games), ×1.1 (5–7), ×1.25 (8–11), ×1.5 (12–16), ×1.75 (17–20).
- **(MAUI) `MainPage.xaml.cs` — `SetPlayUiEnabled(bool)`** — replaces all scattered `btn.IsEnabled` lines; disables/re-enables 17 controls atomically including the new GAMES stepper and SPEED pills.
- **(MAUI) `MainPage.xaml.cs` — `UpdateStatus()` total wager display** — top-bar WAGER label shows `EffectiveWager × consecutiveGames` when more than one game is queued so the player always sees the full commit.
- **(MAUI) `MainPage.xaml.cs` — inter-game pause** — 600 ms delay between games when speed is not SS; no pause after the final game.

---

## [2026-03-25] — Scroll, Quick Pick, Side Bets (Multiplier / Powerball / First-Last), Favorites

- **(MAUI) `MainPage.xaml` — scrollable layout** — outer `Grid` reduced to 2 rows (`Auto` top bar + `*` `ScrollView`); all gameplay content sits inside a `VerticalStackLayout` inside the `ScrollView` so the full board is reachable on any screen size.
- **(MAUI) `MainPage.xaml` — Quick Pick row** — compact `Grid` between the wager strip and the number board; `−`/`+` stepper adjusts count 1–20; **PICK** button clears and auto-selects that many numbers using a Fisher-Yates shuffle.
- **(MAUI) `MainPage.xaml` — Side Bets row** — three toggle-pill Buttons (MULTIPLIER / POWERBALL / FIRST-LAST) above a three-column info strip; each pill lights up in its accent color (purple / red / orange) when active.
- **(MAUI) `MainPage.xaml` — Favorites row** — `FAV` label plus three `★N` Buttons; each tap opens a `DisplayActionSheet` (Save / Load / Clear); button shows pick count when a slot is occupied.
- **(MAUI) `MainPage.xaml.cs` — `EffectiveWager` property** — computed `_currentWager + $1 × Multiplier + $1 × First/Last`; used for bank deduction, top-bar display, and `GameRecord`; Powerball adds no cost (payout multiplier only).
- **(MAUI) `MainPage.xaml.cs` — Multiplier side bet** — `DrawMultiplier()` weighted draw (×1 45% / ×2 30% / ×3 13% / ×5 9% / ×10 3%) scales base payout; current value shown in info strip after each game.
- **(MAUI) `MainPage.xaml.cs` — Powerball side bet** — draws a random 1–80 ball after each game; if the ball is in the player's picks the payout is multiplied ×4; drawn ball number shown in info strip.
- **(MAUI) `MainPage.xaml.cs` — First/Last side bet** — if the first or last ball drawn matches any pick, `KenoPayouts.GetFirstLastBallBonus(pickedCount)` (from **Keno.Core**) is added as a flat cash bonus on top of the (optionally multiplied) base payout.
- **(MAUI) `MainPage.xaml.cs` — Favorites persistence** — `UpdateFavoriteButtons()` reads `Preferences.Default` keys `fav_0`–`fav_2` (comma-separated sorted numbers); `HandleFavoriteAsync()` presents Save / Load / Clear action sheet; Load calls `BtnClear_Clicked` then restores cells.
- **(MAUI) `MainPage.xaml.cs` — play guard improvements** — Quick Pick stepper, side-bet pills, and all three FAV buttons are disabled for the duration of `PlayGameAsync` and re-enabled in `finally`.
- **(MAUI) `MainPage.xaml.cs` — CLEAR reset** — `_lastMultiplierValue` and `_lastPowerballNumber` reset to defaults on CLEAR; `UpdateSideBetInfo()` called to refresh info strip.

---

## [2026-03-25] — Game history viewer: SESSION tab + ALL TIME tab + hamburger menu

- **(MAUI) `GameRecord.cs` — new immutable record** — `record GameRecord(DateTime Time, int Picked, int Matched, decimal Wager, decimal Payout)` with computed `Net` and `IsWin`; stored in the in-memory `_sessionHistory` list on `MainPage`.
- **(MAUI) `HistoryPage.xaml` — new modal page** — teal header bar with GAME HISTORY title and ✕ close button; two-button tab strip (SESSION / ALL TIME); overlapping `ScrollView` panels toggled by `IsVisible`; content built entirely in code-behind.
- **(MAUI) `HistoryPage.xaml.cs` — history page code-behind** — `BuildSessionContent()` renders a 4-column summary card (Games / Wins / Wagered / Net) followed by per-game rows (newest first) showing game #, time, pick→match, wager→payout, and WIN/LOSE badge; `BuildAllTimeContent()` reads `Preferences.Default` keys (`at_games`, `at_wins`, `at_wagered`, `at_won`, `at_best_win`) and renders stat-box pairs; `BtnClose_Clicked` calls `Navigation.PopModalAsync()`.
- **(MAUI) `MainPage.xaml` — hamburger menu button** — top bar column definitions changed from `*,*,*` to `*,*,*,Auto`; `BtnMenu` (☰, 44×44, transparent) added at column 3.
- **(MAUI) `MainPage.xaml.cs` — session + all-time tracking** — added `_sessionHistory = []` field; after each game appends a `GameRecord` to the list and increments `Preferences.Default` all-time counters; `BtnMenu_Clicked` opens `HistoryPage(_sessionHistory)` with `Navigation.PushModalAsync`.
- **(MAUI) `Keno.Android.csproj` — version bump + icon update** — `AssemblyVersion`/`FileVersion` bumped to `26.3.25.28`; `MauiIcon` updated with `ForegroundFile="Resources\AppIcon\appiconfg.svg"` and `Color="#8b0000"`; `HistoryPage.xaml` added to `MauiXaml` item group; stale absolute `keno_2.png` path reference removed.

---

## [2026-03-24] — Playable game loop: board, draw animation, PLAY / REPLAY / CLEAR

- **(MAUI) `MainPage.xaml` — full 5-row game board layout** — `Grid` with top info bar (Bank / Wager / Picks), horizontal wager scroller, `Border`-wrapped 8×10 number board, picks + drawn strip containers, and PLAY / REPLAY / CLEAR button row with status strip (Matches · Payout · Streak).
- **(MAUI) `MainPage.xaml.cs` — board construction** — `BuildNumberGrid()` creates 80 `Label` cells in an 8×10 `Grid` with `TapGestureRecognizer`; `BuildPicksGrid()` and `BuildDrawnGrid()` build 2×10 display strips; `BuildWagerButtons()` generates 14 preset wager buttons ($1–$200).
- **(MAUI) `NumberLabel_Tapped` — pick selection** — toggles cells between `CellDefault` / `CellSelected`, enforces 15-pick maximum, updates picks display and status bar.
- **(MAUI) `WagerButton_Clicked` — wager selection** — highlights active wager button, updates `_currentWager` and status bar.
- **(MAUI) `PlayGameAsync()` — core game loop (async)** — deducts wager from in-memory `_bankBalance`; Fisher-Yates shuffle draws 20 balls; reveals each ball with an 80 ms animation (board cell + drawn strip coloured gold for matches, amber for drawn-not-picked); calls `KenoPayouts.GetPayout(picked, matched)` from **Keno.Core** for payout; credits bank; updates win/loss streak; saves `_lastPickedNumbers` for REPLAY; disables all action buttons during animation, re-enables in `finally`.
- **(MAUI) `BtnPlay_Clicked` — PLAY button** — guards on empty picks, delegates to `PlayGameAsync()`.
- **(MAUI) `BtnReplay_Clicked` — REPLAY button** — restores last set of picks onto the cleared board, delegates to `PlayGameAsync()`; button disabled until after first game completes.
- **(MAUI) `BtnClear_Clicked` — CLEAR button** — resets all 80 board cells (including any drawn/matched amber/gold state), clears picks and drawn strips, resets status labels.
- **(MAUI) `UpdateStatus()` — live top bar** — updates `LblBank`, `LblWagerTotal`, and `LblPicks` after every state change.
- **(MAUI) `UpdateStreak()` — streak label** — displays "Win: N" / "Loss: N" / "Streak: —" in the status strip.
- **(MAUI) Code quality fixes** — `IDE0028`: collection initialiser `[]`; `CA1868`: use `HashSet.Remove()` return value instead of `Contains` + `Remove`; `IDE0031`: C# 14 null-conditional assignment `?.BackgroundColor`.

---

## [2026-03-24] — Project scaffold + Keno.Core reference

- **(MAUI) `Keno.Android.csproj` — `Keno.Core` project reference added** — `Keno.Core` (VB.NET, `net10.0`) referenced directly; no language conversion required. All payout tables, stores, and globals are immediately available from C# code.
- **(MAUI) Initial MAUI scaffold** — project created from the `dotnet new maui` template targeting `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, and `net10.0-windows10.0.19041.0`. `WindowsPackageType` set to `None` (unpackaged).
- **(MAUI) `MauiProgram.cs`** — default DI/logging wired; `Microsoft.Extensions.Logging.Debug` registered for debug builds.
- **(MAUI) `AppShell.xaml`** — single-page shell routing to `MainPage`.
- **(MAUI) `MainPage.xaml`** — placeholder template page; full Keno game UI not yet implemented.

---
