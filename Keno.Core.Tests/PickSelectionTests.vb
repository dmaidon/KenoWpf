Imports Xunit

Public Class PickSelectionTests

    <Fact>
    Public Sub Toggle_AddsRemovesAndCapsAtTwenty()
        Dim s As New PickSelection()
        Assert.True(s.Toggle(5))
        Assert.True(s.Contains(5))
        Assert.True(s.Toggle(5))
        Assert.Equal(0, s.Count)
        For i = 1 To 20 : s.Toggle(i) : Next
        Assert.False(s.Toggle(21))
        Assert.Equal(20, s.Count)
        Assert.True(s.Toggle(20))   ' removing is still allowed when full
    End Sub

    <Fact>
    Public Sub Toggle_RejectsOutOfRange()
        Dim s As New PickSelection()
        Assert.False(s.Toggle(0))
        Assert.False(s.Toggle(81))
    End Sub

    <Fact>
    Public Sub Toggle_ClearsBullseyeAndWayTicket()
        Dim s As New PickSelection()
        s.ActivateBullseye()
        s.WayGroups = New List(Of List(Of Integer))()
        s.Toggle(2)
        Assert.False(s.IsBullseye)
        Assert.Null(s.WayGroups)
    End Sub

    <Fact>
    Public Sub Bullseye_SelectsEightFixedNumbers()
        Dim s As New PickSelection()
        s.Toggle(3)
        s.ActivateBullseye()
        Assert.True(s.IsBullseye)
        Assert.Equal(PickSelection.BullseyeNumbers.OrderBy(Function(n) n), s.Numbers)
    End Sub

    <Fact>
    Public Sub QuadrantNumbers_AreCorrect()
        Assert.Equal(1, PickSelection.GetQuadrantNumbers(1).Min())
        Assert.Equal(35, PickSelection.GetQuadrantNumbers(1).Max())
        Assert.Equal(6, PickSelection.GetQuadrantNumbers(2).Min())
        Assert.Equal(41, PickSelection.GetQuadrantNumbers(3).Min())
        Assert.Equal(80, PickSelection.GetQuadrantNumbers(4).Max())
        Assert.Equal(20, PickSelection.GetQuadrantNumbers(4).Distinct().Count())
    End Sub

    <Fact>
    Public Sub ToggleQuadrant_CapsAtTwoActive()
        Dim s As New PickSelection()
        Assert.True(s.ToggleQuadrant(1))
        Assert.True(s.ToggleQuadrant(2))
        Assert.False(s.ToggleQuadrant(3))
        Assert.Equal(2, s.CountActiveQuadrants())
        Assert.True(s.ToggleQuadrant(1))     ' deselect
        Assert.Equal(1, s.CountActiveQuadrants())
    End Sub

    <Theory>
    <InlineData(1, 2, "TopBottom")>
    <InlineData(3, 4, "TopBottom")>
    <InlineData(1, 3, "LeftRight")>
    <InlineData(2, 4, "LeftRight")>
    <InlineData(1, 4, "LeftRight")>
    <InlineData(2, 3, "LeftRight")>
    Public Sub HalfType_FollowsQuadrantPairs(a As Integer, b As Integer, expected As String)
        Dim s As New PickSelection()
        s.ToggleQuadrant(a)
        s.ToggleQuadrant(b)
        Assert.Equal(expected, s.GetHalfType())
    End Sub

    <Fact>
    Public Sub HalfType_NothingWithOneQuadrant()
        Dim s As New PickSelection()
        s.ToggleQuadrant(1)
        Assert.Null(s.GetHalfType())
    End Sub

    <Fact>
    Public Sub QuickPick_ReturnsRequestedDistinctCount()
        Dim s As New PickSelection()
        s.QuickPick(7, New Random(3))
        Assert.Equal(7, s.Count)
    End Sub

    <Fact>
    Public Sub Replace_KeepsValidNumbersUpToTwenty()
        Dim s As New PickSelection()
        s.Replace(Enumerable.Range(0, 30))
        Assert.Equal(20, s.Count)
        Assert.DoesNotContain(0, s.Numbers)
    End Sub

End Class
