# Last Edit: 2026-03-20 05:02 AM - Free $2 Game section rewritten: queue-up behaviour, 10-cell strip, straight-through PLAY, CLEAR cancels.

# 🎰 Keno

> A full-featured **Windows Keno simulator** built with VB.NET and .NET 10 —  
> a modern **WPF front-end** sitting on top of a shared **Keno.Core** class library,  
> with the original **WinForms** app retained alongside it.

---

## Table of Contents

- [Overview](#overview)
- [Solution Structure](#solution-structure)
- [Requirements](#requirements)
- [Build & Run](#build--run)
- [Game Features](#game-features)
- [Selecting Numbers](#selecting-numbers)
- [Placing a Bet](#placing-a-bet)
- [Game Modes](#game-modes)
- [Consecutive Games](#consecutive-games)
- [Special Play Options](#special-play-options)
- [Way Ticket](#way-ticket)
- [Payout Schedules](#payout-schedules)
- [Bonuses](#bonuses)
- [Progressive Jackpot](#progressive-jackpot)
- [Session & All-Time Summary](#session--all-time-summary)
- [Hot & Cold Numbers](#hot--cold-numbers)
- [My Favorites](#my-favorites)
- [Game History Log](#game-history-log)
- [Data Files](#data-files)
- [License](#license)

---

## Overview

Keno is a lottery-style game where you pick 1–20 numbers from a board of 80 and watch 20 balls  
drawn at random. The more of your picks that match, the bigger the payout.

This simulator reproduces the full casino experience including Multiplier Keno, Powerball,  
Bullseye, Way Tickets with King Numbers, consecutive game bonuses, a Progressive Jackpot,  
First/Last Ball bonuses, free games, and persistent statistics.

---

## Solution Structure

```
KenoWpf.slnx
│
├── Keno.Core\          — Shared business logic (payouts, stores, globals)
│   ├── Globals.vb
│   ├── Modules\
│   │   ├── KenoPayouts.vb
│   │   ├── AllTimeSummaryStore.vb
│   │   ├── AppSettingsStore.vb
│   │   ├── BankSettingsStore.vb
│   │   ├── DrawStatsStore.vb
│   │   ├── FavoritesStore.vb
│   │   ├── FreeGamesStore.vb
│   │   ├── GameLogStore.vb
│   │   └── ProgressiveJackpotStore.vb
│
├── Keno.Wpf\           — WPF front-end (net10.0-windows) ← PRIMARY
│   ├── MainWindow.xaml(.vb)
│   ├── WinBonusGame.xaml(.vb)
│   ├── WinPayoutSchedule.xaml(.vb)
│   ├── WinGameLog.xaml(.vb)
│   ├── WinFavoritesSlot.xaml(.vb)
│   ├── WinSessionSummary.xaml(.vb)
│   ├── WinConsecutiveSummary.xaml(.vb)
│   ├── WinAllTimeSummary.xaml(.vb)
│   ├── WinKenoHelp.xaml(.vb)
│   └── WinWayTicket.xaml(.vb)
│
└── Keno\               — WinForms front-end (net10.0-windows) ← legacy / reference
    ├── FrmMain.vb
    ├── FrmWayTicket.vb
    └── ...
```

Both front-ends share the same `Keno.Core` library and write to the same `Data\`, `Logs\`,  
and `Settings\` folders under the application directory.

---

## Requirements

| Requirement | Version |
|-------------|---------|
| .NET SDK | **10.0** or later |
| OS | **Windows 10 build 22000** (21H2) or later |
| IDE | Visual Studio 2026 (recommended) or VS 2022 17.8+ |

---

## Build & Run

```powershell
# Clone and build
git clone https://github.com/<your-username>/KenoWpf.git
cd KenoWpf
dotnet build

# Run the WPF version
dotnet run --project Keno.Wpf

# Run the WinForms version (legacy)
dotnet run --project Keno
```

Or open `KenoWpf.slnx` in Visual Studio, set **Keno.Wpf** as the startup project, and press **F5**.

---

## Game Features

| Feature | Description |
|---------|-------------|
| Pick 1–20 numbers | Any combination from the 80-number board |
| Quadrant / Half play | Bet on a quarter or half of the board |
| Bullseye | Fixed 8-spot corner + centre pattern |
| Way Ticket | Split picks into equal groups; each group plays as its own sub-ticket |
| King Ticket | One number added to every Way Ticket sub-ticket |
| Multiplier Keno | A random multiplier (1×–10×) is applied to each game's payout |
| Powerball | Optional +$1 side-bet; hitting it multiplies your win by ×4 |
| First / Last Ball | Flat dollar bonus if the 1st or 20th drawn ball is one of your picks |
| Consecutive games | Auto-play 2–20 rounds; a series bonus multiplier rewards longer runs |
| Quick Pick | Randomly select any count of numbers |
| Free Games | Catch-none on Pick 5–9 (bet ≥ $2) earns one free $2 game |
| Progressive Jackpot | 5 % of wagers ≥ $5 feeds a shared pool; Pick 8+ full-match wins it |
| Favorites | Save and load up to 3 slots of your favourite numbers |
| Hot & Cold stats | Top-5 most / least frequent numbers over the last 15 draws |
| Session Summary | Net P/L, win rate, best payout, return % shown on close |
| All-Time History | Cumulative stats across every session |
| Game History Log | Every game written to `Data\game-log.txt` |
| In-App Help | Searchable topic tree covering all features |

---

## Selecting Numbers

### Regular Pick (1–20 numbers)

Click any numbers on the 8×10 grid to select them (highlights in **green**).  
Click a selected number again to deselect it.  
Your picks fill the two rows of numbered boxes below the grid (slots 1–10 top, 11–20 bottom).

### Quadrant Play

Click a **Q1 / Q2 / Q3 / Q4** button to **toggle** that quadrant on or off (20 numbers each).  
Click a selected button again to deselect it. **At most 2 quadrants may be active at once.**

```
+----------+----------+
|    Q1    |    Q2    |   Rows 1–4  (numbers 1–40)
+----------+----------+
|    Q3    |    Q4    |   Rows 5–8  (numbers 41–80)
+----------+----------+
```

| Selection | Coverage | Payout scale |
|-----------|----------|--------------|
| Single Q  | 20 numbers | Regular Pick 20 |
| Q1 + Q2   | Top half — 40 numbers | Top/Bottom Half |
| Q3 + Q4   | Bottom half — 40 numbers | Top/Bottom Half |
| Q1 + Q3   | Left half — 40 numbers | Left/Right Half |
| Q2 + Q4   | Right half — 40 numbers | Left/Right Half |
| Q1 + Q4   | Diagonal — 40 numbers | Left/Right Half |
| Q2 + Q3   | Diagonal — 40 numbers | Left/Right Half |

> Quadrant / half selections and manual number picks are mutually exclusive — selecting one clears the other.  
> **First / Last Ball** is unavailable during half-board (2-quadrant) play.

---

## Placing a Bet

Select a preset bet: **$1, $2, $3, $5, $10, $15, $20, $25, $30, $40, $50, $100, $150, $200**.  
Or enter any amount from **$1 to $1,000** in the **Custom Wager** spinner (auto-capped to your balance).

The **Wager Total** label updates **live** as you select numbers, change the bet, toggle Multiplier  
Keno, or adjust consecutive count — so you always know the full cost before clicking Play.

---

## Game Modes

| Mode | How to activate | Numbers in play |
|------|-----------------|-----------------|
| Regular | Pick 1–20 numbers manually or via Quick Pick | Your picks |
| Bullseye | Click **Play Bullseye** | 8 fixed: 1, 10, 35, 36, 45, 46, 71, 80 |
| Quadrant | Toggle a Q-button (Q1–Q4); max 2 active | 20 numbers |
| Top / Bottom Half | Select **Q1+Q2** or **Q3+Q4** | 40 numbers |
| Left / Right Half | Select any other pair of Q-buttons | 40 numbers |
| Way Ticket | Tick **Way Ticket**, then assign groups | Groups × sub-tickets |

Draw animation highlights:

| Result | Colour / Symbol |
|--------|-----------------|
| Your pick, matched | 🟡 Gold — **✓** (Regular), **◎** (Bullseye), **♦** (Way Ticket) |
| Drawn, not your pick | 🔵 LightSkyBlue |
| Your pick, not drawn | 🟩 LightGreen |

---

## Consecutive Games

Play **2–20** rounds in a row with the same numbers and bet. The total cost is deducted upfront.  
A **series bonus** multiplies combined winnings at the end:

| Games | Bonus |
|-------|-------|
| 2–4   | 1× (none) |
| 5–7   | 1.1× |
| 8–11  | 1.25× |
| 12–16 | 1.5× |
| 17–20 | 1.75× |

A **Consecutive Games Summary** dialog appears after every series of 2+ games, showing  
each game's result, subtotal before bonus, the bonus applied, and the total payout.

---

## Special Play Options

### Multiplier Keno

Check **Multiplier Keno** (+$1) to draw a random multiplier each game:

| Multiplier | Approximate weight |
|------------|--------------------|
| 1× | 45 % |
| 2× | 25 % |
| 3× | 15 % |
| 4× | 8 % |
| 5× | 4 % |
| 8× | 2 % |
| 10× | 1 % |

The multiplier is drawn fresh for every individual game in a consecutive series.

### Powerball (+$1, Pick 6–20)

After the regular draw, a 21st ball is drawn from the 60 unused numbers.  
If it lands on one of your picks → any win is multiplied by **×4**.  
Stacks with Multiplier Keno. Unchecks automatically after play.

### First / Last Ball Bonus (+$1)

If the **1st** or **20th** drawn ball is one of your picks, you receive a flat bonus on top of  
your regular payout. The bonus is independent of bet size, multiplier, and Powerball, and is  
**exempt from the consecutive series multiplier** (added flat after).

| Pick | Bonus | Pick | Bonus |
|------|-------|------|-------|
| 1    | $75   | 11   | $35   |
| 2    | $71   | 12   | $31   |
| 3    | $67   | 13   | $27   |
| 4    | $63   | 14   | $23   |
| 5    | $59   | 15   | $20   |
| 6    | $55   | 16   | $17   |
| 7    | $51   | 17   | $14   |
| 8    | $47   | 18   | $11   |
| 9    | $43   | 19   | $8    |
| 10   | $39   | 20   | $5    |

---

## Way Ticket

Tick **Way Ticket**, then click **Play** to open the **Way Ticket** dialog.

1. Choose a **Group colour** (G1–G5) from the combo box.
2. Click each number to assign it to that group.  
   Click a number again with the same group selected to remove it; with a different group to reassign it.
3. You need **at least 2 groups of equal size** to enable **Play Ways**.

The dialog shows live:

| Panel | Content |
|-------|---------|
| **Group Summary** | Each group's colour, name, and assigned numbers |
| **Colour Legend** | G1 Blue → G2 Green → G3 Salmon → G4 Plum → G5 Yellow → ♛ Goldenrod |
| **Sub-ticket Preview** | Each sub-ticket with its numbers; King number appended with ♕ |

### King Ticket

Check **♛ King Ticket** to enter King mode, then click one number to crown it as King.

- The King button turns **Goldenrod** with a ♛ symbol.
- The King is automatically added to **every sub-ticket** (does not belong to a single group).
- Each sub-ticket becomes `group size + 1` spots.
- Click the King number again at any time to unset it.
- The king-status bar shows gold (king set) or dark gold (king mode, no king yet).

---

## Payout Schedules

All payouts are **multipliers × bet**. A 5x multiplier on a $10 bet returns $50.

### Regular Pick 1–20

| Pick | Winning combinations (hits → multiplier) |
|------|------------------------------------------|
| 1    | 1=2x |
| 2    | 2=10x |
| 3    | 3=25x, 2=2x |
| 4    | 4=50x, 3=5x, 2=1x |
| 5    | 5=500x, 4=15x, 3=2x |
| 6    | 6=1500x, 5=50x, 4=5x, 3=1x |
| 7    | 7=5000x, 6=150x, 5=15x, 4=2x, 3=1x |
| 8    | 8=15000x, 7=400x, 6=50x, 5=10x, 4=2x |
| 9    | 9=25000x, 8=2500x, 7=200x, 6=25x, 5=4x, 4=1x |
| 10   | 10=200000x, 9=10000x, 8=500x, 7=50x, 6=10x, 5=3x, **0=3x** |
| 11   | 11=500000x, 10=50000x, 9=2000x, 8=100x, 7=20x, 6=4x, 5=1x, **0=4x** |
| 12   | 12=500000x, 11=50000x, 10=5000x, 9=500x, 8=75x, 7=10x, 6=2x, 5=1x, **0=10x** |
| 13   | 13=750000x, 12=75000x, 11=10000x, 10=1000x, 9=100x, 8=20x, 7=4x, 6=1x, **0=25x** |
| 14   | 14=1000000x, 13=100000x, 12=15000x, 11=2000x, 10=200x, 9=40x, 8=8x, 7=2x, **0=50x** |
| 15   | 15=1000000x, 14=200000x, 13=25000x, 12=5000x, 11=500x, 10=75x, 9=15x, 8=4x, 7=1x, **0=100x** |
| 16   | 16=1000000x, 15=250000x, 14=50000x, 13=10000x, 12=1000x, 11=150x, 10=25x, 9=5x, 8=1x, **0=250x** |
| 17   | 17=1000000x, 16=300000x, 15=75000x, 14=15000x, 13=2500x, 12=400x, 11=60x, 10=10x, 9=2x, **0=500x** |
| 18   | 18=1000000x, 17=500000x, 16=100000x, 15=25000x, 14=5000x, 13=1000x, 12=150x, 11=25x, 10=5x, 9=1x, **0=1000x** |
| 19   | 19=1000000x, 18=500000x, 17=200000x, 16=50000x, 15=10000x, 14=2500x, 13=500x, 12=75x, 11=15x, 10=3x, **0=2500x** |
| 20   | 20=1000000x, 19=500000x, 18=250000x, 17=100000x, 16=25000x, 15=5000x, 14=1000x, 13=200x, 12=40x, 11=8x, 10=2x, **0=10000x** |

> **Catch-none bonus**: picks 10–20 pay when you match **zero** numbers (bold **0=** entries above).

### Bullseye (fixed 8-spot)

| Matched | Multiplier |
|---------|------------|
| 8 | 30,000x |
| 7 | 500x |
| 6 | 75x |
| 5 | 15x |
| 4 | 3x |
| 0 | 10x *(catch-none)* |

### Top / Bottom Half

| Matched | Multiplier |
|---------|------------|
| 0 | 50x |
| 1 | 10x |
| 2 | 5x |
| 3 | 2x |
| 4 | 1x |
| 16 | 1x |
| 17 | 2x |
| 18 | 10x |
| 19 | 50x |
| 20 | 500x |

### Left / Right Half

| Matched | Multiplier |
|---------|------------|
| 0 | 40x |
| 1 | 8x |
| 2 | 4x |
| 3 | 2x |
| 4 | 1x |
| 16 | 1x |
| 17 | 2x |
| 18 | 8x |
| 19 | 40x |
| 20 | 400x |

### Single Quadrant

| Matched | Multiplier |
|---------|------------|
| 0 | 20x |
| 1 | 5x |
| 2 | 2x |
| 3 | 1x |
| 11 | 1x |
| 12 | 2x |
| 13 | 5x |
| 14 | 20x |
| 15 | 100x |
| 16 | 500x |
| 17 | 2,000x |
| 18 | 10,000x |
| 19 | 50,000x |
| 20 | 250,000x |

---

## Bonuses

### Free $2 Game

Playing **Pick 5–9** with a bet of **$2 or more** and matching **zero numbers** earns one free $2 game.  
Free games persist between sessions and cannot be earned while playing a free game.

**Queuing free games:**  
When credits are available the green **Free Games Won (N)** button becomes enabled.  
Each click stages one game in the 10-cell queue strip below the keno grid (cells light up gold).  
The button label counts down the remaining credits still available to stage.  
Once you have queued as many as you want, click **PLAY** — all queued games run straight through at the selected draw speed with no popups or delays between them.  
Each cell turns **green** for a win or **red** for a loss as it completes.  
Clicking **CLEAR** cancels any staged games and returns the credits to the pool.

---

## Progressive Jackpot

Any wager of **$5 or more** contributes **5%** to the shared progressive jackpot (persisted in `jackpot.json`).  
To win: select **Pick 8–20** and match **every single number** you picked.  
The pool resets to a **$25,000 seed** after each win.

---

## Session & All-Time Summary

**Session Summary** appears when you close the app (after ≥ 1 game played):

| Stat | Description |
|------|-------------|
| Net Profit / Loss | Balance change since launch |
| Games Played | Games this session |
| Win Rate | Wins ÷ total games |
| Best Single Payout | Highest payout this session |
| Free Games Earned | Bonus games awarded |
| Total Wagered | Sum of all bets |
| Total Payout | Sum of all payouts |
| Return % | Payout ÷ Wagered (green ≥ 100 %, red below) |

**All-Time History** (click **Play History** in the status bar): cumulative totals across all sessions,  
including jackpots won, total free games earned, net P/L, and all-time best single payout.

---

## Hot & Cold Numbers

After 20 games, the app tracks frequency across the last 15 draws (persisted in `Data\draw-stats.json`).

- **Hot numbers** — top 5 most frequent, shown as red pills in **StatBar2** (bottom status bar).
- **Cold numbers** — top 5 least frequent, shown as blue pills in **StatBar1** (top status bar).
- Click any hot or cold pill to toggle that number on the grid, same as clicking directly.

---

## My Favorites

Save up to **3 independent slots** of favourite number sets.  
`BtnSaveFavorites` and `BtnPlayFavorites` prompt a slot picker dialog showing each slot's  
number count and name. Slots persist in `Settings\favorites.json`.

---

## Game History Log

Every game is appended to `Data\game-log.txt`:

```
2026-03-16 14:05:22 | Regular 8-spot | Bet: $2.00 | Matched: 5 | Multiplier: 3x | Payout: $30.00
2026-03-16 14:05:30 | Way Ticket     | Bet: $2.00 | Matched: N/A | Multiplier: 1x | Payout: $25.00
2026-03-16 14:05:45 | Free Game (Bullseye) | Bet: $2.00 | Matched: 6 | Multiplier: 1x | Payout: $75.00
```

Consecutive series group multiple game lines under a shared timestamp and close with:

```
Final Payout Calculation:
  N win(s) subtotal = $X.XX
  $X.XX × Bx bonus = $Y.YY
  Total Payout: $Y.YY
End Group ]
```

---

## Data Files

All runtime data is stored relative to the application binary (`AppContext.BaseDirectory`):

| File | Contents |
|------|----------|
| `Data\game-log.txt` | Pipe-delimited record of every game played |
| `Data\all-time-summary.json` | Cumulative stats across all sessions |
| `Data\draw-stats.json` | Per-number frequency, win/loss streaks |
| `Data\bank-settings.json` | Current bank balance |
| `Data\app-settings.json` | Draw speed, last-used preferences |
| `Data\favorites.json` | 3-slot favorites |
| `Data\jackpot.json` | Current progressive jackpot pool |
| `Logs\err_MMMdd.log` | Daily error log (last 10 days retained) |

---

## In-App Help

Click **Help** in the top status bar to open the built-in help viewer.  
Topics include: Overview, Picking Numbers, Placing a Wager, Consecutive Games,  
Quick Pick, Multiplier Keno, Way Ticket, Powerball, Bullseye, Quadrants / Halves,  
Bank & Winnings, Hot & Cold Statistics, My Favorites, Draw Speed, Progressive Jackpot,  
Free Games, and Game Log.

---

## License

This project is licensed under the **GNU General Public License v3.0**.  
See [LICENSE.txt](LICENSE.txt) for the full text.

© 2026 Dennis N. Maidon. All rights reserved.


---