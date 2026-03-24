# Last Edit: 2026-03-24 05:44 PM - Updated status/roadmap to reflect implemented game UI; updated project structure notes.

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
├── MainPage.xaml(.cs)          — Main game page (playable — board, wager, draw, replay)
├── MauiProgram.cs              — Dependency injection / services registration
├── Platforms\
│   ├── Android\
│   │   ├── MainActivity.cs     — Android activity
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

> **🟢 Core game loop playable on Android.**  
> The 8×10 board, wager selector, draw animation, and PLAY / REPLAY / CLEAR buttons are fully functional.

### Implemented

- [x] 8×10 number board with tap-to-select (up to 15 picks)
- [x] Wager selector — 14 preset amounts ($1 – $200)
- [x] Draw animation — 20 balls revealed one-by-one (80 ms/ball), board highlights matches
- [x] PLAY button — deducts wager, draws, shows payout via `KenoPayouts.GetPayout`
- [x] REPLAY button — re-selects last picks and auto-plays with same wager
- [x] CLEAR button — resets board, picks, drawn strip, and status labels
- [x] In-memory bank balance ($10,000 starting, win/loss tracked per game)
- [x] Win / Loss streak counter
- [x] Top bar: Bank · Wager · Picks counts (live)
- [x] Status strip: Matches · Payout · Streak (per-game)

### Planned (WPF parity)

- [ ] Regular, Bullseye, Quadrant, and Half-board game modes
- [ ] Way Ticket / King Ticket
- [ ] Multiplier Keno, Powerball, First/Last Ball side-bets
- [ ] Consecutive games (2–20) with series bonus
- [ ] Free games queue
- [ ] Progressive Jackpot
- [ ] Payout schedule sheet
- [ ] Session summary and All-Time history
- [ ] Hot & Cold number stats
- [ ] Favorites (3 slots)
- [ ] Game history log viewer
- [ ] Persistent bank / settings (via `FileSystem.AppDataDirectory`)
- [ ] In-app help

---

## License

© 2026 Dennis N. Maidon. All rights reserved.
