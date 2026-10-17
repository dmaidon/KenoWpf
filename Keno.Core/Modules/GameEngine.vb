' Pure game rules: wager cost, draw, per-game payout, consecutive bonus. No UI or disk access.

Public Enum TicketMode
    Regular
    Bullseye
    TopBottomHalf
    LeftRightHalf
End Enum

Public Class GameOptions
    Public Property Mode As TicketMode = TicketMode.Regular
    Public Property Picks As IReadOnlyCollection(Of Integer) = Array.Empty(Of Integer)()
    Public Property Bet As Decimal
    Public Property UseMultiplier As Boolean
    Public Property UseFirstLast As Boolean
    Public Property UsePowerball As Boolean
    Public Property UseWayTicket As Boolean
    ''' <summary>Way-ticket sub-tickets; Nothing or empty when Way Ticket is off.</summary>
    Public Property WayGroups As IReadOnlyList(Of List(Of Integer))
End Class

Public Class GameScore
    Public Property Matches As Integer
    Public Property Multiplier As Integer = 1
    ''' <summary>Powerball number drawn for this game, or 0 when Powerball is off.</summary>
    Public Property PowerballNumber As Integer
    Public Property FirstLastBonus As Decimal
    Public Property Payout As Decimal
End Class

Public Module GameEngine

    Public Const BoardSize As Integer = 80
    Public Const DrawSize As Integer = 20

    ''' <summary>Draws 20 distinct numbers from 1-80 in draw order.</summary>
    Public Function DrawNumbers(rng As Random) As List(Of Integer)
        Dim pool As New List(Of Integer)(BoardSize)
        For i = 1 To BoardSize : pool.Add(i) : Next
        Dim order As New List(Of Integer)(DrawSize)
        Do While order.Count < DrawSize
            Dim idx = rng.Next(pool.Count)
            order.Add(pool(idx))
            pool.RemoveAt(idx)
        Loop
        Return order
    End Function

    ''' <summary>Weighted multiplier: x1=45%, x2=30%, x3=13%, x5=9%, x10=3%.</summary>
    Public Function DrawMultiplier(rng As Random) As Integer
        Return MultiplierForRoll(rng.Next(100))
    End Function

    Public Function MultiplierForRoll(roll As Integer) As Integer
        If roll < 45 Then Return 1
        If roll < 75 Then Return 2
        If roll < 88 Then Return 3
        If roll < 97 Then Return 5
        Return 10
    End Function

    ''' <summary>Bonus applied to the (non First/Last) subtotal of a consecutive-game run.</summary>
    Public Function GetConsecutiveBonus(gameCount As Integer) As Decimal
        If gameCount > 16 Then Return 1.75D
        If gameCount >= 12 Then Return 1.5D
        If gameCount >= 8 Then Return 1.25D
        If gameCount >= 5 Then Return 1.1D
        Return 1D
    End Function

    ''' <summary>Applies the consecutive bonus to everything except the flat First/Last bonuses.</summary>
    Public Function ApplyConsecutiveBonus(payout As Decimal, firstLastBonus As Decimal, bonus As Decimal) As Decimal
        Return (payout - firstLastBonus) * bonus + firstLastBonus
    End Function

    ''' <summary>Full cost: base bet + $1 Multiplier + $1 First/Last + extra Way sub-tickets, per game.</summary>
    Public Function ComputeTotalWager(bet As Decimal, gamesToPlay As Integer, useMultiplier As Boolean,
                                      useFirstLast As Boolean, useWayTicket As Boolean,
                                      wayGroupCount As Integer, pickCount As Integer) As Decimal
        Dim perGame = bet
        If useMultiplier Then perGame += 1D
        If useFirstLast Then perGame += 1D
        If useWayTicket Then
            Dim subTickets = If(wayGroupCount > 0, wayGroupCount, Math.Max(1, pickCount \ 3))
            perGame += (subTickets - 1) * bet
        End If
        Return perGame * gamesToPlay
    End Function

    Public Function CountMatches(picks As IEnumerable(Of Integer), draw As ICollection(Of Integer)) As Integer
        Return picks.Count(Function(n) draw.Contains(n))
    End Function

    Public Function GetWayTicketPayout(groups As IEnumerable(Of List(Of Integer)), draw As ICollection(Of Integer), bet As Decimal) As Decimal
        If groups Is Nothing Then Return 0D
        Dim total = 0D
        For Each group In groups
            total += GetPayout(group.Count, CountMatches(group, draw)) * bet
        Next
        Return total
    End Function

    ''' <summary>Free game is awarded on a $2+ bet that hits nothing on a 5-9 spot ticket.</summary>
    Public Function IsFreeGameBonus(matches As Integer, bet As Decimal, pickCount As Integer) As Boolean
        If bet < 2D OrElse matches <> 0 Then Return False
        Return pickCount >= 5 AndAlso pickCount <= 9
    End Function

    ''' <summary>Scores one game: base payout, Multiplier, Powerball (x4 if picked) and flat First/Last bonus.</summary>
    Public Function ScoreGame(options As GameOptions, drawOrder As IReadOnlyList(Of Integer), rng As Random) As GameScore
        Dim picks = options.Picks
        Dim draw As New HashSet(Of Integer)(drawOrder)
        Dim score As New GameScore() With {.Matches = CountMatches(picks, draw)}

        Dim payout As Decimal
        If options.UseWayTicket Then
            payout = GetWayTicketPayout(options.WayGroups, draw, options.Bet)
        Else
            Select Case options.Mode
                Case TicketMode.Bullseye
                    payout = GetBullseyePayout(score.Matches) * options.Bet
                Case TicketMode.TopBottomHalf
                    payout = GetAreaPayout("TopBottom", score.Matches) * options.Bet
                Case TicketMode.LeftRightHalf
                    payout = GetAreaPayout("LeftRight", score.Matches) * options.Bet
                Case Else
                    payout = GetPayout(picks.Count, score.Matches) * options.Bet
            End Select
        End If

        If options.UseMultiplier Then
            score.Multiplier = DrawMultiplier(rng)
            payout *= score.Multiplier
        End If

        If options.UsePowerball Then
            score.PowerballNumber = rng.Next(1, BoardSize + 1)
            If picks.Contains(score.PowerballNumber) Then payout *= 4
        End If

        If options.UseFirstLast AndAlso drawOrder.Count > 0 Then
            If picks.Contains(drawOrder(0)) OrElse picks.Contains(drawOrder(drawOrder.Count - 1)) Then
                score.FirstLastBonus = GetFirstLastBallBonus(picks.Count)
                payout += score.FirstLastBonus
            End If
        End If

        score.Payout = payout
        Return score
    End Function

End Module
