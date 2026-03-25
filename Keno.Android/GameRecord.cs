// Last Edit: 2026-03-25 08:05 AM - Immutable record for a single completed Keno game result.
namespace Keno.Android;

/// <summary>Immutable record of one completed Keno game, stored in the in-memory session history.</summary>
public record GameRecord(
    DateTime Time,
    int      Picked,
    int      Matched,
    decimal  Wager,
    decimal  Payout
)
{
    public decimal Net  => Payout - Wager;
    public bool    IsWin => Payout > 0m;
}
