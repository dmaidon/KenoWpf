Imports Xunit

Public Class GameEngineTests

    Private Shared Function Draw(ParamArray nums As Integer()) As List(Of Integer)
        ' pad to 20 with numbers not in nums (from 80 downwards)
        Dim list As New List(Of Integer)(nums)
        Dim n = 80
        Do While list.Count < 20
            If Not list.Contains(n) Then list.Add(n)
            n -= 1
        Loop
        Return list
    End Function

    <Fact>
    Public Sub DrawNumbers_ReturnsTwentyDistinctInRange()
        Dim d = GameEngine.DrawNumbers(New Random(1))
        Assert.Equal(20, d.Count)
        Assert.Equal(20, d.Distinct().Count())
        Assert.All(d, Sub(n) Assert.InRange(n, 1, 80))
    End Sub

    <Theory>
    <InlineData(0, 1)>
    <InlineData(44, 1)>
    <InlineData(45, 2)>
    <InlineData(75, 3)>
    <InlineData(88, 5)>
    <InlineData(97, 10)>
    Public Sub MultiplierForRoll_MatchesWeights(roll As Integer, expected As Integer)
        Assert.Equal(expected, GameEngine.MultiplierForRoll(roll))
    End Sub

    <Theory>
    <InlineData(1, 1.0)>
    <InlineData(4, 1.0)>
    <InlineData(5, 1.1)>
    <InlineData(8, 1.25)>
    <InlineData(12, 1.5)>
    <InlineData(16, 1.5)>
    <InlineData(17, 1.75)>
    Public Sub ConsecutiveBonus_Tiers(games As Integer, expected As Double)
        Assert.Equal(CDec(expected), GameEngine.GetConsecutiveBonus(games))
    End Sub

    <Fact>
    Public Sub ApplyConsecutiveBonus_LeavesFirstLastFlat()
        Assert.Equal(10D * 1.5D + 5D, GameEngine.ApplyConsecutiveBonus(15D, 5D, 1.5D))
    End Sub

    <Fact>
    Public Sub TotalWager_AddsOptionCosts()
        Assert.Equal(4D * 4D, GameEngine.ComputeTotalWager(2D, 4, True, True, False, 0, 5))
    End Sub

    <Fact>
    Public Sub TotalWager_WayTicketAddsSubTickets()
        ' 3 groups at $2 -> extra 2 sub-tickets = $4 + base $2 = $6 per game
        Assert.Equal(12D, GameEngine.ComputeTotalWager(2D, 2, False, False, True, 3, 9))
    End Sub

    <Theory>
    <InlineData(0, 2, 5, True)>
    <InlineData(0, 2, 9, True)>
    <InlineData(0, 2, 10, False)>
    <InlineData(0, 1, 5, False)>
    <InlineData(1, 2, 5, False)>
    Public Sub FreeGameBonus_Rules(matches As Integer, bet As Decimal, picks As Integer, expected As Boolean)
        Assert.Equal(expected, GameEngine.IsFreeGameBonus(matches, bet, picks))
    End Sub

    <Fact>
    Public Sub ScoreGame_RegularPayout()
        Dim opts As New GameOptions With {.Picks = {1, 2, 3}, .Bet = 2D}
        Dim s = GameEngine.ScoreGame(opts, Draw(1, 2, 3), New Random(1))
        Assert.Equal(3, s.Matches)
        Assert.Equal(50D, s.Payout) ' pick 3 / 3 hits pays 25x
    End Sub

    <Fact>
    Public Sub ScoreGame_NoHitsPaysZero()
        Dim opts As New GameOptions With {.Picks = {1, 2, 3}, .Bet = 1D}
        Dim s = GameEngine.ScoreGame(opts, Draw(10, 11, 12), New Random(1))
        Assert.Equal(0, s.Matches)
        Assert.Equal(0D, s.Payout)
    End Sub

    <Fact>
    Public Sub ScoreGame_FirstLastBonusAddedWhenFirstBallPicked()
        Dim opts As New GameOptions With {.Picks = {1}, .Bet = 1D, .UseFirstLast = True}
        Dim s = GameEngine.ScoreGame(opts, Draw(1), New Random(1))
        Assert.Equal(75D, s.FirstLastBonus)
        Assert.Equal(2D + 75D, s.Payout)
    End Sub

    <Fact>
    Public Sub ScoreGame_BullseyeUsesBullseyeTable()
        Dim opts As New GameOptions With {.Mode = TicketMode.Bullseye, .Picks = {1, 10, 35, 36, 45, 46, 71, 80}, .Bet = 1D}
        Dim s = GameEngine.ScoreGame(opts, Draw(1, 10, 35, 36), New Random(1))
        ' padding 80..65 also hits 71 and 80, so 6 bullseye hits
        Assert.Equal(75D, s.Payout)
    End Sub

    <Fact>
    Public Sub ScoreGame_WayTicketSumsGroups()
        Dim groups As New List(Of List(Of Integer)) From {New List(Of Integer) From {1, 2, 3}, New List(Of Integer) From {4, 5, 6}}
        Dim opts As New GameOptions With {.Picks = {1, 2, 3, 4, 5, 6}, .Bet = 1D, .UseWayTicket = True, .WayGroups = groups}
        Dim s = GameEngine.ScoreGame(opts, Draw(1, 2, 3), New Random(1))
        Assert.Equal(25D, s.Payout)
    End Sub

    <Fact>
    Public Sub ScoreGame_PowerballSetsNumberAndStaysInRange()
        Dim opts As New GameOptions With {.Picks = {1}, .Bet = 1D, .UsePowerball = True}
        Dim s = GameEngine.ScoreGame(opts, Draw(1), New Random(1))
        Assert.InRange(s.PowerballNumber, 1, 80)
    End Sub

End Class
