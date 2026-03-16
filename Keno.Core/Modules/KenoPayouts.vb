' Last Edit: 2026-03-13 - FirstLastBallBonus: Pick 1=$75 (max) to Pick 20=$5; -$4/pick for picks 1-14, -$3/pick for picks 14-20. Moved to Keno.Core.
Public Module KenoPayouts

    ' Bullseye fixed-pattern: 4 corners (1,10,71,80) + 4 center (35,36,45,46)
    Private ReadOnly BullseyePayout As New Dictionary(Of Integer, Decimal) From {
        {8, 30000D},
        {7, 500D},
        {6, 75D},
        {5, 15D},
        {4, 3D},
        {0, 10D}
    }

    Private ReadOnly PayoutSchedule As New Dictionary(Of Integer, Dictionary(Of Integer, Decimal)) From {
        {1, New Dictionary(Of Integer, Decimal) From {{1, 2D}}},
        {2, New Dictionary(Of Integer, Decimal) From {{2, 10D}}},
        {3, New Dictionary(Of Integer, Decimal) From {{3, 25D}, {2, 2D}}},
        {4, New Dictionary(Of Integer, Decimal) From {{4, 50D}, {3, 5D}, {2, 1D}}},
        {5, New Dictionary(Of Integer, Decimal) From {{5, 500D}, {4, 15D}, {3, 2D}}},
        {6, New Dictionary(Of Integer, Decimal) From {{6, 1500D}, {5, 50D}, {4, 5D}, {3, 1D}}},
        {7, New Dictionary(Of Integer, Decimal) From {{7, 5000D}, {6, 150D}, {5, 15D}, {4, 2D}, {3, 1D}}},
        {8, New Dictionary(Of Integer, Decimal) From {{8, 15000D}, {7, 400D}, {6, 50D}, {5, 10D}, {4, 2D}}},
        {9, New Dictionary(Of Integer, Decimal) From {{9, 25000D}, {8, 2500D}, {7, 200D}, {6, 25D}, {5, 4D}, {4, 1D}}},
        {10, New Dictionary(Of Integer, Decimal) From {{10, 200000D}, {9, 10000D}, {8, 500D}, {7, 50D}, {6, 10D}, {5, 3D}, {0, 3D}}},
        {11, New Dictionary(Of Integer, Decimal) From {{11, 500000D}, {10, 50000D}, {9, 2000D}, {8, 100D}, {7, 20D}, {6, 4D}, {5, 1D}, {0, 4D}}},
        {12, New Dictionary(Of Integer, Decimal) From {{12, 500000D}, {11, 50000D}, {10, 5000D}, {9, 500D}, {8, 75D}, {7, 10D}, {6, 2D}, {5, 1D}, {0, 10D}}},
        {13, New Dictionary(Of Integer, Decimal) From {{13, 750000D}, {12, 75000D}, {11, 10000D}, {10, 1000D}, {9, 100D}, {8, 20D}, {7, 4D}, {6, 1D}, {0, 25D}}},
        {14, New Dictionary(Of Integer, Decimal) From {{14, 1000000D}, {13, 100000D}, {12, 15000D}, {11, 2000D}, {10, 200D}, {9, 40D}, {8, 8D}, {7, 2D}, {0, 50D}}},
        {15, New Dictionary(Of Integer, Decimal) From {{15, 1000000D}, {14, 200000D}, {13, 25000D}, {12, 5000D}, {11, 500D}, {10, 75D}, {9, 15D}, {8, 4D}, {7, 1D}, {0, 100D}}},
        {16, New Dictionary(Of Integer, Decimal) From {{16, 1000000D}, {15, 250000D}, {14, 50000D}, {13, 10000D}, {12, 1000D}, {11, 150D}, {10, 25D}, {9, 5D}, {8, 1D}, {0, 250D}}},
        {17, New Dictionary(Of Integer, Decimal) From {{17, 1000000D}, {16, 300000D}, {15, 75000D}, {14, 15000D}, {13, 2500D}, {12, 400D}, {11, 60D}, {10, 10D}, {9, 2D}, {0, 500D}}},
        {18, New Dictionary(Of Integer, Decimal) From {{18, 1000000D}, {17, 500000D}, {16, 100000D}, {15, 25000D}, {14, 5000D}, {13, 1000D}, {12, 150D}, {11, 25D}, {10, 5D}, {9, 1D}, {0, 1000D}}},
        {19, New Dictionary(Of Integer, Decimal) From {{19, 1000000D}, {18, 500000D}, {17, 200000D}, {16, 50000D}, {15, 10000D}, {14, 2500D}, {13, 500D}, {12, 75D}, {11, 15D}, {10, 3D}, {0, 2500D}}},
        {20, New Dictionary(Of Integer, Decimal) From {{20, 1000000D}, {19, 500000D}, {18, 250000D}, {17, 100000D}, {16, 25000D}, {15, 5000D}, {14, 1000D}, {13, 200D}, {12, 40D}, {11, 8D}, {10, 2D}, {0, 10000D}}}
    }

    Private ReadOnly AreaPayouts As New Dictionary(Of String, Dictionary(Of Integer, Decimal)) From {
        {
            "TopBottom",
            New Dictionary(Of Integer, Decimal) From {
                {0, 50D}, {1, 10D}, {2, 5D}, {3, 2D}, {4, 1D},
                {5, 0D}, {6, 0D}, {7, 0D}, {8, 0D}, {9, 0D},
                {10, 0D}, {11, 0D}, {12, 0D}, {13, 0D}, {14, 0D},
                {15, 0D}, {16, 1D}, {17, 2D}, {18, 10D}, {19, 50D}, {20, 500D}
            }
        },
        {
            "LeftRight",
            New Dictionary(Of Integer, Decimal) From {
                {0, 40D}, {1, 8D}, {2, 4D}, {3, 2D}, {4, 1D},
                {5, 0D}, {6, 0D}, {7, 0D}, {8, 0D}, {9, 0D},
                {10, 0D}, {11, 0D}, {12, 0D}, {13, 0D}, {14, 0D},
                {15, 0D}, {16, 1D}, {17, 2D}, {18, 8D}, {19, 40D}, {20, 400D}
            }
        },
        {
            "Quadrants",
            New Dictionary(Of Integer, Decimal) From {
                {0, 20D}, {1, 5D}, {2, 2D}, {3, 1D},
                {4, 0D}, {5, 0D}, {6, 0D}, {7, 0D}, {8, 0D}, {9, 0D}, {10, 0D},
                {11, 1D}, {12, 2D}, {13, 5D}, {14, 20D}, {15, 100D},
                {16, 500D}, {17, 2000D}, {18, 10000D}, {19, 50000D}, {20, 250000D}
            }
        }
    }

    Public Function GetPayout(picked As Integer, matched As Integer) As Decimal
        Dim matchSchedule As Dictionary(Of Integer, Decimal) = Nothing
        If Not PayoutSchedule.TryGetValue(picked, matchSchedule) Then
            Return 0D
        End If

        Dim payout As Decimal
        If matchSchedule.TryGetValue(matched, payout) Then
            Return payout
        End If

        Return 0D
    End Function

    Public Function GetAreaPayout(areaType As String, matched As Integer) As Decimal
        Dim matchSchedule As Dictionary(Of Integer, Decimal) = Nothing
        If Not AreaPayouts.TryGetValue(areaType, matchSchedule) Then
            Return 0D
        End If

        Dim payout As Decimal
        If matchSchedule.TryGetValue(matched, payout) Then
            Return payout
        End If

        Return 0D
    End Function

    Public Function GetPayoutScheduleEntries(pickedCount As Integer) As Dictionary(Of Integer, Decimal)
        Dim matchSchedule As Dictionary(Of Integer, Decimal) = Nothing
        If PayoutSchedule.TryGetValue(pickedCount, matchSchedule) Then
            Return matchSchedule
        End If

        Return New Dictionary(Of Integer, Decimal)()
    End Function

    Public Function GetAreaPayoutScheduleEntries(areaType As String) As Dictionary(Of Integer, Decimal)
        Dim matchSchedule As Dictionary(Of Integer, Decimal) = Nothing
        If AreaPayouts.TryGetValue(areaType, matchSchedule) Then
            Return matchSchedule
        End If

        Return New Dictionary(Of Integer, Decimal)()
    End Function

    Public Function GetBullseyePayout(matched As Integer) As Decimal
        Dim payout As Decimal
        If BullseyePayout.TryGetValue(matched, payout) Then
            Return payout
        End If

        Return 0D
    End Function

    Public Function GetBullseyePayoutScheduleEntries() As Dictionary(Of Integer, Decimal)
        Return BullseyePayout
    End Function

    ' $70 range over 19 steps (~$3.68/step). Whole-dollar solution: 13 steps of $4 (picks 1-14) then 6 steps of $3 (picks 14-20).
    Private ReadOnly FirstLastBallBonus As New Dictionary(Of Integer, Decimal) From {
        {1, 75D}, {2, 71D}, {3, 67D}, {4, 63D}, {5, 59D},
        {6, 55D}, {7, 51D}, {8, 47D}, {9, 43D}, {10, 39D},
        {11, 35D}, {12, 31D}, {13, 27D}, {14, 23D}, {15, 20D},
        {16, 17D}, {17, 14D}, {18, 11D}, {19, 8D}, {20, 5D}
    }

    Public Function GetFirstLastBallBonus(pickedCount As Integer) As Decimal
        Dim bonus As Decimal
        If FirstLastBallBonus.TryGetValue(pickedCount, bonus) Then
            Return bonus
        End If

        Return 0D
    End Function

End Module
