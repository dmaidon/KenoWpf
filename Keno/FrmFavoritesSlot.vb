' Last Edit: 2026-03-23 - Favorites slot picker dialog (Save / Load, 3 slots).
Friend Class FrmFavoritesSlot
    Inherits Form

    Private _selectedSlot As Integer = -1
    Private ReadOnly _isSaveMode As Boolean

    Public ReadOnly Property SelectedSlot As Integer
        Get
            Return _selectedSlot
        End Get
    End Property

    Private ReadOnly _slotButtons As New List(Of Button)()
    Private WithEvents BtnCancel As New Button()
    Private ReadOnly _lblTitle As New Label()

    ' -------------------------------------------------------------------------
    ' Factory
    ' -------------------------------------------------------------------------
    Friend Shared Function ShowForSave(owner As Form, numberCount As Integer) As Integer
        Using frm As New FrmFavoritesSlot(isSave:=True, numberCount)
            If frm.ShowDialog(owner) = DialogResult.OK Then
                Return frm.SelectedSlot
            End If
        End Using
        Return -1
    End Function

    Friend Shared Function ShowForLoad(owner As Form) As Integer
        Using frm As New FrmFavoritesSlot(isSave:=False, 0)
            If frm.ShowDialog(owner) = DialogResult.OK Then
                Return frm.SelectedSlot
            End If
        End Using
        Return -1
    End Function

    ' -------------------------------------------------------------------------
    ' Constructor
    ' -------------------------------------------------------------------------
    Public Sub New(isSave As Boolean, numberCount As Integer)
        _isSaveMode = isSave

        Text = If(isSave, "Save Favorites", "Load Favorites")
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Size(320, 210)
        Font = New Font("Segoe UI", 9)

        _lblTitle.Location = New Point(0, 0)
        _lblTitle.Size = New Size(320, 36)
        _lblTitle.TextAlign = ContentAlignment.MiddleCenter
        _lblTitle.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        _lblTitle.BackColor = Color.SteelBlue
        _lblTitle.ForeColor = Color.White
        _lblTitle.Text = If(isSave,
            $"Save {numberCount} number{If(numberCount = 1, "", "s")} to which slot?",
            "Load numbers from which slot?")

        Dim slots = GetAllSlots()
        For i = 0 To SlotCount - 1
            Dim slotIndex = i
            Dim count = If(slots(i).Numbers IsNot Nothing, slots(i).Numbers.Length, 0)
            Dim name = If(Not String.IsNullOrWhiteSpace(slots(i).Name), slots(i).Name, $"Slot {i + 1}")
            Dim countText = If(count > 0, $"{count} number{If(count = 1, "", "s")}", "empty")

            Dim btn As New Button With {
                .Location = New Point(16, 46 + i * 46),
                .Size = New Size(288, 38),
                .Text = $"{name}  —  {countText}",
                .Font = New Font("Segoe UI", 9),
                .FlatStyle = FlatStyle.Flat,
                .TextAlign = ContentAlignment.MiddleLeft,
                .Padding = New Padding(8, 0, 0, 0)
            }
            btn.FlatAppearance.BorderSize = 1

            If Not isSave AndAlso count = 0 Then
                btn.Enabled = False
                btn.ForeColor = SystemColors.GrayText
            Else
                btn.BackColor = Color.AliceBlue
            End If

            AddHandler btn.Click, Sub(s, e)
                                      _selectedSlot = slotIndex
                                      DialogResult = DialogResult.OK
                                      Close()
                                  End Sub
            _slotButtons.Add(btn)
            Controls.Add(btn)
        Next

        BtnCancel.Location = New Point(218, 170)
        BtnCancel.Size = New Size(86, 28)
        BtnCancel.Text = "Cancel"
        BtnCancel.DialogResult = DialogResult.Cancel
        CancelButton = BtnCancel

        Controls.Add(_lblTitle)
        Controls.Add(BtnCancel)
    End Sub

    Private Sub BtnCancel_Click(sender As Object, e As EventArgs) Handles BtnCancel.Click
        Close()
    End Sub

End Class