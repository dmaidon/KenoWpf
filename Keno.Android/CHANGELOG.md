# Last Edit: 2026-03-24 05:44 PM - Add UI scaffold and game loop entries.

# Changelog — Keno.Android (MAUI)

All notable changes to the MAUI / mobile project are documented here.  
Format: most-recent first. Entries are prefixed **(MAUI)**.  
See the [root CHANGELOG](../CHANGELOG.md) for WPF and WinForms history.

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
