' Last Edit: 2026-03-15 - WinFavoritesSlot: 3-slot favorites picker; ShowForSave/ShowForLoad factory methods.
Class WinFavoritesSlot

    Private _selectedSlot As Integer = -1
    Private ReadOnly _isSaveMode As Boolean
    Private ReadOnly _numberCount As Integer

    Public ReadOnly Property SelectedSlot As Integer
        Get
            Return _selectedSlot
        End Get
    End Property

    ' ── Factories ─────────────────────────────────────────────────────────────
    Friend Shared Function ShowForSave(owner As Window, numberCount As Integer) As Integer
        Dim win As New WinFavoritesSlot(isSave:=True, numberCount) With {
            .Owner = owner
        }

        If win.ShowDialog() = True Then
            Return win.SelectedSlot
        End If

        Return -1
    End Function

    Friend Shared Function ShowForLoad(owner As Window) As Integer
        Dim win As New WinFavoritesSlot(isSave:=False, 0) With {
            .Owner = owner
        }

        If win.ShowDialog() = True Then
            Return win.SelectedSlot
        End If

        Return -1
    End Function

    ' ── Constructor ───────────────────────────────────────────────────────────
    Public Sub New(isSave As Boolean, numberCount As Integer)
        _isSaveMode = isSave
        _numberCount = numberCount
        InitializeComponent()
    End Sub

    Private Sub WinFavoritesSlot_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Title = If(_isSaveMode, "Save Favorites", "Load Favorites")

        TbkTitle.Text = If(_isSaveMode,
            $"Save {_numberCount} number{If(_numberCount = 1, "", "s")} to which slot?",
            "Load numbers from which slot?")

        Dim slots = GetAllSlots()
        Dim slotCount = If(slots IsNot Nothing, slots.Length, 3)

        For i = 0 To slotCount - 1
            Dim slotIndex = i
            Dim count = If(slots IsNot Nothing AndAlso slots(i).Numbers IsNot Nothing,
                           slots(i).Numbers.Length, 0)
            Dim name = If(slots IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(slots(i).Name),
                          slots(i).Name, $"Slot {i + 1}")
            Dim countText = If(count > 0, $"{count} number{If(count = 1, "", "s")}", "empty")

            Dim btn As New Button() With {
                .Content = $"{name}  —  {countText}",
                .FontFamily = New FontFamily("Segoe UI"),
                .FontSize = 10,
                .HorizontalContentAlignment = HorizontalAlignment.Left,
                .Padding = New Thickness(8, 0, 0, 0),
                .Height = 36,
                .Margin = New Thickness(0, 0, 0, 4),
                .Background = New SolidColorBrush(Color.FromRgb(240, 248, 255)),
                .Foreground = If(count > 0, Brushes.Black, Brushes.Gray),
                .IsEnabled = _isSaveMode OrElse count > 0
            }

            Dim capturedIndex = slotIndex
            AddHandler btn.Click, Sub(s, ev)
                                      _selectedSlot = capturedIndex
                                      DialogResult = True
                                  End Sub

            SpSlots.Children.Add(btn)
        Next
    End Sub

    Private Sub BtnCancel_Click(sender As Object, e As RoutedEventArgs)
        DialogResult = False
    End Sub

End Class