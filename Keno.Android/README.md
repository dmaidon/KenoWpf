# Last Edit: Apr 10, 2026 15:15 - Clarify multiplier pricing examples and net-win guidance.

# 📱 Keno — MAUI / Mobile

> A cross-platform mobile port of the **Keno** simulator, built with **.NET MAUI** and **C# / .NET 10**.  
> Shares the **Keno.Core** business logic library with the existing WPF and WinForms apps.

---

## Table of Contents

- [Overview](#overview)
- [Relationship to the Main Solution](#relationship-to-the-main-solution)
- [Platform Targets](#platform-targets)
- [Requirements](#requirements)
- [Build & Run](#build--run)
- [Project Structure](#project-structure)
- [Shared Core Logic](#shared-core-logic)
- [Data Storage on Mobile](#data-storage-on-mobile)
- [Status & Roadmap](#status--roadmap)
- [License](#license)

---

## Overview

This project is a mobile-first reimplementation of the Keno simulator using .NET MAUI.  
All game rules, payout schedules, statistics, and persistence logic live in **Keno.Core**  
and are shared verbatim — only the UI layer is new.

---

## Relationship to the Main Solution

```
KenoWpf.slnx
│
├── Keno.Core\        — Shared business logic (VB.NET, net10.0)  ← referenced here
├── Keno.Wpf\         — WPF front-end (Windows only)
├── Keno\             — WinForms front-end (Windows only, legacy)
└── Keno.Android\     — MAUI front-end (this project)
```

`Keno.Android` references `Keno.Core` directly.  
VB.NET and C# are fully interoperable in .NET — no code conversion is required.

---

## Platform Targets

| Platform | Minimum OS |
|----------|-----------|
| Android  | 5.0 (API 21) |
| iOS      | 15.0 |
| macOS (Mac Catalyst) | 15.0 |
| Windows  | 10 build 17763 |

---

## Requirements

| Requirement | Version |
|-------------|---------|
| .NET SDK | **10.0** or later |
| .NET MAUI workload | Installed via `dotnet workload install maui` |
| Android SDK | API 21+ (installed via Visual Studio or Android Studio) |
| IDE | Visual Studio 2026 (recommended) |

Install the MAUI workload once if not already present:

```powershell
dotnet workload install maui
```

---

## Build & Run

```powershell
# Build for Android only
dotnet build -f net10.0-android Keno.Android\Keno.Android.csproj

# Build for iOS (requires macOS or paired Mac)
dotnet build -f net10.0-ios Keno.Android\Keno.Android.csproj

# Build for Windows
dotnet build -f net10.0-windows10.0.19041.0 Keno.Android\Keno.Android.csproj

# Build all targets
dotnet build Keno.Android\Keno.Android.csproj
```

Or open `KenoWpf.slnx` in Visual Studio, set **Keno.Android** as the startup project,  
select the target device/emulator from the toolbar, and press **F5**.

---

## Project Structure

```
Keno.Android\
├── App.xaml(.cs)               — Application entry point, resource dictionaries
├── AppShell.xaml(.cs)          — Navigation shell
├── GameRecord.cs               — Immutable record for a single completed game (session history)
├── HistoryPage.xaml(.cs)       — Modal game history: SESSION per-game rows + ALL TIME stat cards
├── MainPage.xaml(.cs)          — Main game page (playable — board, wager, draw, replay, history)
├── MauiProgram.cs              — Dependency injection / services registration
├── Platforms\
│   ├── Android\
│   │   ├── MainActivity.cs     — Android activity
│   │   ├── Resources\values\styles.xml — MAUI splash/main Android themes
│   │   └── MainApplication.cs  — Android application class
│   ├── iOS\
│   ├── MacCatalyst\
│   └── Windows\
└── Resources\
    ├── AppIcon\
    ├── Fonts\
    ├── Images\
    ├── Raw\
    └── Splash\
```

---

## Shared Core Logic

All game logic is provided by **Keno.Core** (VB.NET, `net10.0`).  
From C# code, VB.NET modules are used as static classes:

```csharp
using Keno.Core;

// Payout lookups
int payout = KenoPayouts.GetPayout(pickedCount, matched);

// Persistence
GameLogStore.AppendGame("Standard", 1.00m, 5, payout);
var settings = AppSettingsStore.Load();

// Bank
BankSettingsStore.Credit(payout);
```

No conversion from VB.NET to C# is needed — the compiled assembly is language-agnostic.

---

## Data Storage on Mobile

The desktop apps write data to subdirectories of `AppContext.BaseDirectory`  
(`Data\`, `Logs\`, `Settings\`). On Android and iOS those paths are **read-only**.

The MAUI version redirects all file I/O to platform-safe locations via  
`Microsoft.Maui.Storage.FileSystem`:

| Purpose | Path |
|---------|------|
| Game data / saves | `FileSystem.AppDataDirectory` |
| Error logs | `FileSystem.AppDataDirectory` |
| Settings | `FileSystem.AppDataDirectory` |

> This path substitution is handled in the MAUI project layer and does not require  
> any changes to `Keno.Core`.

---

## Status & Roadmap

> **🟢 Core game loop fully playable on Android.**  
> Board, wager selector (preset + custom), draw animation, PLAY / REPLAY / CLEAR, Quick Pick, three side bets, 3-slot Favorites, game history viewer, consecutive games (1–20 with series bonus), four draw speeds, persistent bank & settings, and an automatic bank replenish button are all functional.

### Implemented

- [x] **Android theme hardening** — added `Maui.SplashTheme` and `Maui.MainTheme` resource styles (MaterialComponents-compatible via MAUI base themes) plus `maui_splash_color` to resolve startup crash: `Java.Lang.IllegalArgumentException` requiring valid `TextAppearance`
- [x] **History/menu reliability hardening** — menu modal navigation now prevents re-entry and waits briefly after action sheet close before opening modal pages; session history rendering is capped to recent rows to keep Game History responsive on large sessions
- [x] **Dynamic multiplier pricing** — multiplier side-bet fee is now `max($1.00, 3% of base wager)` per game (for example, a `$200` base wager uses a `$6.00` multiplier fee)
- [x] **Multiplier net guidance** — at a `$200` base wager, a `2×` (or higher) multiplier typically produces a stronger winning outcome after the `$6.00` fee, depending on the base payout result

- [x] 8×10 number board with tap-to-select (up to 20 picks)
- [x] Quick Pick — −/+/PICK stepper auto-selects 1–20 random numbers
- [x] Wager selector — 14 preset amounts ($1 – $200) **+ CUSTOM wager entry**
- [x] Draw animation — 20 balls revealed one-by-one at selected speed, board highlights matches
- [x] PLAY button — deducts wager, draws, shows payout via `KenoPayouts.GetPayout`
- [x] REPLAY button — re-selects last picks and auto-plays with same wager
- [x] CLEAR button — resets board, picks, drawn strip, and status labels
- [x] **Persistent bank balance** — saved to `Preferences.Default` after every game; survives app restarts
- [x] **Persistent settings** — wager, side-bet toggles, Quick Pick count, draw speed, and consecutive games all restored on next launch
- [x] **REPLENISH BANK button** — appears automatically when balance hits $0; offers +$1,000 / +$5,000 / Reset to $10,000
- [x] Win / Loss streak counter
- [x] Top bar: Bank · Wager (total for all queued games) · Picks counts (live)
- [x] Status strip: Matches · Payout · Streak (per-game)
- [x] **Multiplier side bet** — fee is `max($1.00, 3% of base wager)` per game; weighted ×1–×10 draw scales base payout
- [x] **Powerball side bet** — free; random 1–80 ball drawn; ×4 payout if ball is a pick
- [x] **First/Last side bet** — +$1/game; flat `KenoPayouts.GetFirstLastBallBonus` cash bonus if first or last drawn ball is a pick
- [x] **Favorites (3 slots)** — tap any ★ button to Save / Load / Clear pick sets via action sheet; persisted with `Preferences.Default`
- [x] Hamburger menu (☰) in top bar opens game history modal
- [x] SESSION history — per-game rows (time, pick→match, wager→payout, WIN/LOSE badge) with summary card
- [x] ALL TIME stats — Games / Win Rate / Total Wagered / Total Won / Net P&L / Best Win persisted via `Preferences.Default`
- [x] **Consecutive games (1–20)** — GAMES stepper queues 1–20 games per PLAY; cost auto-clamped so total wager never exceeds bank; series bonus ×1.1–×1.75 applied at end
- [x] **Bank cap / auto-clamp** — games count reduced automatically when wager × games would exceed balance; triggered on wager change, Multiplier/First-Last toggle, and Games `+` press
- [x] **Draw speed** — SLOW (1 s/ball) / MED (0.5 s/ball) / FAST (0.2 s/ball) / SS (instant); FAST is the default

### Planned (WPF parity)

- [x] **Payout schedule sheet** — ☰ → Payout Schedule; pick-count selector (1–20); CATCH / PAYS / AT $5 table via `KenoPayouts.GetPayoutScheduleEntries()`
- [x] **In-app help** — ☰ → How to Play; 10 sections: board, wager, Quick Pick, PLAY/REPLAY/CLEAR, consecutive games + bonus table, side bets, draw speed, favorites, bank/replenish, top bar
- [ ] Regular, Bullseye, Quadrant, and Half-board game modes
- [ ] Way Ticket / King Ticket
- [ ] Free games queue
- [ ] Progressive Jackpot
- [ ] Payout schedule sheet
- [ ] Hot & Cold number stats
- [ ] In-app help

---

## License

© 2026 Dennis N. Maidon. All rights reserved.
