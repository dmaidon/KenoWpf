' Player's current ticket: picked numbers, Bullseye flag, quadrant helpers and Way Ticket groups. No UI.

Public Class PickSelection

    Public Const MaxPicks As Integer = 20

    Public Shared ReadOnly BullseyeNumbers As Integer() = {1, 10, 35, 36, 45, 46, 71, 80}

    Private ReadOnly _numbers As New SortedSet(Of Integer)()

    Public ReadOnly Property Numbers As SortedSet(Of Integer)
        Get
            Return _numbers
        End Get
    End Property

    Public ReadOnly Property Count As Integer
        Get
            Return _numbers.Count
        End Get
    End Property

    Public Property IsBullseye As Boolean

    ''' <summary>Way Ticket sub-tickets, or Nothing when not assigned.</summary>
    Public Property WayGroups As List(Of List(Of Integer))

    Public Property WayKingNumber As Integer

    Public Function Contains(number As Integer) As Boolean
        Return _numbers.Contains(number)
    End Function

    Public Sub ClearWayTicket()
        WayGroups = Nothing
        WayKingNumber = 0
    End Sub

    Public Sub Clear()
        _numbers.Clear()
        IsBullseye = False
        ClearWayTicket()
    End Sub

    ''' <summary>Toggles one number. Returns False when the ticket is full and nothing changed.</summary>
    Public Function Toggle(number As Integer) As Boolean
        If number < 1 OrElse number > GameEngine.BoardSize Then Return False
        If _numbers.Contains(number) Then
            _numbers.Remove(number)
        ElseIf _numbers.Count < MaxPicks Then
            _numbers.Add(number)
        Else
            Return False
        End If
        IsBullseye = False
        ClearWayTicket()
        Return True
    End Function

    ''' <summary>Replaces the ticket (favorites / replay); keeps at most 20 valid numbers.</summary>
    Public Sub Replace(numbers As IEnumerable(Of Integer))
        Clear()
        For Each n In numbers
            If n >= 1 AndAlso n <= GameEngine.BoardSize AndAlso _numbers.Count < MaxPicks Then _numbers.Add(n)
        Next
    End Sub

    Public Sub QuickPick(count As Integer, rng As Random)
        Clear()
        Dim pool = Enumerable.Range(1, GameEngine.BoardSize).ToList()
        Do While _numbers.Count < Math.Min(count, MaxPicks) AndAlso pool.Count > 0
            Dim idx = rng.Next(pool.Count)
            _numbers.Add(pool(idx))
            pool.RemoveAt(idx)
        Loop
    End Sub

    Public Sub ActivateBullseye()
        Clear()
        For Each n In BullseyeNumbers
            _numbers.Add(n)
        Next
        IsBullseye = True
    End Sub

    ' ── quadrants: Q1=top-left Q2=top-right Q3=bottom-left Q4=bottom-right ──
    Public Shared Function GetQuadrantNumbers(quadrant As Integer) As Integer()
        Dim rowStart = If(quadrant <= 2, 0, 4)
        Dim colStart = If(quadrant Mod 2 = 1, 0, 5)
        Dim result(19) As Integer
        Dim idx = 0
        For r = 0 To 3
            For c = 0 To 4
                result(idx) = (rowStart + r) * 10 + (colStart + c) + 1
                idx += 1
            Next
        Next
        Return result
    End Function

    Public Function IsQuadrantActive(quadrant As Integer) As Boolean
        Return GetQuadrantNumbers(quadrant).All(Function(n) _numbers.Contains(n))
    End Function

    Public Function CountActiveQuadrants() As Integer
        Return Enumerable.Range(1, 4).Count(Function(q) IsQuadrantActive(q))
    End Function

    ''' <summary>Toggles a quadrant. At most two may be active; returns False when blocked by that cap.</summary>
    Public Function ToggleQuadrant(quadrant As Integer) As Boolean
        Dim active = IsQuadrantActive(quadrant)
        If Not active AndAlso CountActiveQuadrants() >= 2 Then Return False

        IsBullseye = False
        ClearWayTicket()
        For Each n In GetQuadrantNumbers(quadrant)
            If active Then _numbers.Remove(n) Else _numbers.Add(n)
        Next
        Return True
    End Function

    ''' <summary>"TopBottom" or "LeftRight" when exactly two quadrants are active, otherwise Nothing.</summary>
    Public Function GetHalfType() As String
        Dim active = Enumerable.Range(1, 4).Where(Function(q) IsQuadrantActive(q)).ToList()
        If active.Count <> 2 Then Return Nothing
        Dim a = active(0), b = active(1)
        If (a = 1 AndAlso b = 2) OrElse (a = 3 AndAlso b = 4) Then Return "TopBottom"
        Return "LeftRight"
    End Function

End Class
