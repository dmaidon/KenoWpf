' Last Edit: 2026-04-01 - Full WinForms parity: Group Summary, Sub-ticket Preview, Colour Legend, Goldenrod king, always-unset-king, Play Ways button.
Class WinWayTicket

    ' ── Group colours ─────────────────────────────────────────────────────────
    Private Shared ReadOnly GroupColors As Color() = {
        Color.FromRgb(230, 230, 230),
        Color.FromRgb(135, 206, 235),
        Color.FromRgb(152, 251, 152),
        Color.FromRgb(250, 128, 114),
        Color.FromRgb(221, 160, 221),
        Color.FromRgb(250, 250, 170)
    }

    Private Shared ReadOnly GroupLabels As String() = {"(none)", "G1", "G2", "G3", "G4", "G5"}
    Private Shared ReadOnly KingBrushColor As Color = Color.FromRgb(218, 165, 32)

    ' ── State
    Private ReadOnly _inputNumbers As List(Of Integer)

    Private ReadOnly _betAmount As Decimal
    Private ReadOnly _assignments As New Dictionary(Of Integer, Integer)()
    Private ReadOnly _numberButtons As New Dictionary(Of Integer, Button)()
    Private _kingNumber As Integer = 0
    Private _selectedGroupIndex As Integer = 1

    ' ── Output ────────────────────────────────────────────────────────────────
    Public ReadOnly Property KingNumber As Integer
        Get
            Return _kingNumber
        End Get
    End Property

    Public ReadOnly Property Groups As List(Of List(Of Integer))
        Get
            Dim result As New List(Of List(Of Integer))()
            Dim grps = _assignments _
                .Where(Function(kvp) kvp.Key <> _kingNumber AndAlso kvp.Value > 0) _
                .GroupBy(Function(kvp) kvp.Value) _
                .OrderBy(Function(g) g.Key)

            For Each grp In grps
                Dim nums = grp.Select(Function(kvp) kvp.Key).OrderBy(Function(n) n).ToList()
                If _kingNumber > 0 Then
                    nums.Add(_kingNumber)
                    nums.Sort()
                End If
                result.Add(nums)
            Next

            Return result
        End Get
    End Property

    ' ── Factory ───────────────────────────────────────────────────────────────
    Friend Shared Function ShowAssignment(owner As Window,
                                          numbers As List(Of Integer),
                                          betAmount As Decimal) As WinWayTicket
        Dim win As New WinWayTicket(numbers, betAmount) With {
            .Owner = owner
        }

        If win.ShowDialog() = True Then
            Return win
        End If

        Return Nothing
    End Function

    ' ── Constructor ───────────────────────────────────────────────────────────
    Public Sub New(numbers As List(Of Integer), betAmount As Decimal)
        _inputNumbers = numbers
        _betAmount = betAmount
        InitializeComponent()
    End Sub

    Private Sub WinWayTicket_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        For i = 0 To GroupLabels.Length - 1
            Dim item As New ComboBoxItem() With {
                .Content = GroupLabels(i),
                .Background = New SolidColorBrush(GroupColors(i))
            }
            CmbGroup.Items.Add(item)
        Next
        CmbGroup.SelectedIndex = 1
        AddHandler CmbGroup.SelectionChanged, AddressOf CmbGroup_SelectionChanged

        For Each num In _inputNumbers
            _assignments(num) = 0
            Dim capturedNum = num
            Dim btn As New Button() With {
                .Content = num.ToString(),
                .Width = 36,
                .Height = 30,
                .Margin = New Thickness(2),
                .FontFamily = New FontFamily("Segoe UI"),
                .FontSize = 9,
                .Background = New SolidColorBrush(GroupColors(0))
            }
            AddHandler btn.Click, Sub(s, ev) OnNumberButtonClick(capturedNum)
            _numberButtons(num) = btn
            WpNumbers.Children.Add(btn)
        Next

        RefreshGroupList()
        UpdateSummary()
    End Sub

    Private Sub CmbGroup_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        _selectedGroupIndex = CmbGroup.SelectedIndex
    End Sub

    Private Sub OnNumberButtonClick(num As Integer)
        ' Clicking the current king always unsets it regardless of mode (matches WinForms)
        If num = _kingNumber Then
            _kingNumber = 0
            _assignments(num) = 0
            RefreshButton(num)
            RefreshGroupList()
            UpdateSummary()
            Return
        End If

        If ChkKingTicket.IsChecked = True Then
            If _kingNumber > 0 Then
                _assignments(_kingNumber) = 0
                RefreshButton(_kingNumber)
            End If
            _kingNumber = num
            _assignments(num) = -1
        Else
            _assignments(num) = If(_assignments(num) = _selectedGroupIndex, 0, _selectedGroupIndex)
        End If

        RefreshButton(num)
        RefreshGroupList()
        UpdateSummary()
    End Sub

    Private Sub RefreshButton(num As Integer)
        Dim btn = _numberButtons(num)
        Dim grp = _assignments(num)

        If grp = -1 Then
            btn.Background = New SolidColorBrush(KingBrushColor)
            btn.Content = $"{num} {ChrW(&H265B)}"
        ElseIf grp > 0 Then
            btn.Background = New SolidColorBrush(GroupColors(grp))
            btn.Content = $"{num}{ChrW(&HB7)}{GroupLabels(grp)}"
        Else
            btn.Background = New SolidColorBrush(GroupColors(0))
            btn.Content = num.ToString()
        End If
    End Sub

    Private Sub ChkKingTicket_Changed(sender As Object, e As RoutedEventArgs)
        If ChkKingTicket.IsChecked <> True AndAlso _kingNumber > 0 Then
            _assignments(_kingNumber) = 0
            RefreshButton(_kingNumber)
            _kingNumber = 0
        End If
        RefreshGroupList()
        UpdateSummary()
    End Sub

    Private Sub UpdateSummary()
        If _kingNumber > 0 Then
            TbkKingStatus.Text = $"King: {_kingNumber} {ChrW(&H265B)}  {ChrW(&H2014)}  click to unset"
            TbkKingStatus.Background = New SolidColorBrush(KingBrushColor)
            TbkKingStatus.Foreground = Brushes.Black
        ElseIf ChkKingTicket.IsChecked = True Then
            TbkKingStatus.Text = $"Click a number to designate it King {ChrW(&H265B)}"
            TbkKingStatus.Background = Brushes.DarkGoldenrod
            TbkKingStatus.Foreground = Brushes.White
        Else
            TbkKingStatus.Text = String.Empty
            TbkKingStatus.Background = Brushes.Transparent
            TbkKingStatus.Foreground = Brushes.DarkOrange
        End If

        If ChkKingTicket.IsChecked = True AndAlso _kingNumber = 0 Then
            TbkSummary.Text = "Click a number to designate it King ♛."
            TbkSummary.Foreground = Brushes.DarkSlateGray
            BtnOk.IsEnabled = False
            Return
        End If

        Dim unassigned = _assignments.Where(Function(kvp) kvp.Key <> _kingNumber AndAlso kvp.Value = 0).Count()
        If unassigned > 0 Then
            TbkSummary.Text = $"{unassigned} number(s) still unassigned."
            TbkSummary.Foreground = Brushes.Firebrick
            BtnOk.IsEnabled = False
            Return
        End If

        Dim grps = _assignments.Where(Function(kvp) kvp.Key <> _kingNumber AndAlso kvp.Value > 0) _
                               .GroupBy(Function(kvp) kvp.Value).ToList()
        If grps.Count < 2 Then
            TbkSummary.Text = "Need at least 2 groups."
            TbkSummary.Foreground = Brushes.Firebrick
            BtnOk.IsEnabled = False
            Return
        End If

        Dim targetSize = grps(0).Count()
        If Not grps.All(Function(g) g.Count() = targetSize) Then
            TbkSummary.Text = "All groups must be the same size."
            TbkSummary.Foreground = Brushes.Firebrick
            BtnOk.IsEnabled = False
            Return
        End If

        Dim subTickets = grps.Count
        Dim spotCount = If(_kingNumber > 0, targetSize + 1, targetSize)
        Dim totalCost = subTickets * _betAmount
        TbkSummary.Text = If(_kingNumber > 0,
            $"{subTickets} sub-tickets  ({spotCount}-spot w/ King ♛)  {ChrW(&H2014)}  Total: {totalCost:C2}",
            $"{subTickets} sub-tickets  ({spotCount}-spot each)  {ChrW(&H2014)}  Total: {totalCost:C2}")
        TbkSummary.Foreground = Brushes.ForestGreen
        BtnOk.IsEnabled = True
    End Sub

    Private Sub BtnOk_Click(sender As Object, e As RoutedEventArgs)
        DialogResult = True
    End Sub

    Private Sub BtnCancel_Click(sender As Object, e As RoutedEventArgs)
        DialogResult = False
    End Sub

    ' ── Group summary & sub-ticket preview ────────────────────────────────────────────
    Private Sub RefreshGroupList()
        Dim rows As New List(Of GroupDisplayRow)()

        If _kingNumber > 0 Then
            rows.Add(New GroupDisplayRow() With {
                .GroupName = "♛ King",
                .NumbersText = _kingNumber.ToString(),
                .RowBrush = New SolidColorBrush(KingBrushColor)
            })
        End If

        Dim grps = _assignments _
            .Where(Function(kvp) kvp.Key <> _kingNumber AndAlso kvp.Value > 0) _
            .GroupBy(Function(kvp) kvp.Value) _
            .OrderBy(Function(g) g.Key)

        For Each grp In grps
            Dim nums = grp.Select(Function(kvp) kvp.Key).OrderBy(Function(n) n)
            rows.Add(New GroupDisplayRow() With {
                .GroupName = $"G{grp.Key}",
                .NumbersText = String.Join(", ", nums),
                .RowBrush = New SolidColorBrush(GroupColors(grp.Key))
            })
        Next

        IcGroups.ItemsSource = rows
        RefreshSubTicketPreview()
    End Sub

    Private Sub RefreshSubTicketPreview()
        Dim rows As New List(Of GroupDisplayRow)()

        Dim grps = _assignments _
            .Where(Function(kvp) kvp.Key <> _kingNumber AndAlso kvp.Value > 0) _
            .GroupBy(Function(kvp) kvp.Value) _
            .OrderBy(Function(g) g.Key)

        Dim subIndex = 1
        For Each grp In grps
            Dim nums = grp.Select(Function(kvp) kvp.Key).OrderBy(Function(n) n).ToList()
            Dim numsText = String.Join(", ", nums)
            If _kingNumber > 0 Then
                numsText &= $"  {ChrW(&H2655)}{_kingNumber}"
            End If
            rows.Add(New GroupDisplayRow() With {
                .GroupName = $"ST {subIndex}",
                .NumbersText = numsText,
                .RowBrush = New SolidColorBrush(GroupColors(grp.Key))
            })
            subIndex += 1
        Next

        IcSubTickets.ItemsSource = rows
    End Sub

    Private Class GroupDisplayRow
        Public Property GroupName As String
        Public Property NumbersText As String
        Public Property RowBrush As SolidColorBrush
    End Class

End Class