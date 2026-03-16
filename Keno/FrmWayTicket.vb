' Last Edit: 2026-03-10 - Constructor calls InitializeComponent() to resolve BC40054.
Public Class FrmWayTicket
    Inherits Form

    ' -------------------------------------------------------------------------
    ' Input / State
    ' -------------------------------------------------------------------------
    Private ReadOnly _inputNumbers As List(Of Integer)

    Private ReadOnly _betAmount As Decimal
    Private ReadOnly _assignments As New Dictionary(Of Integer, Integer)()  ' number -> group (0=unassigned)
    Private ReadOnly _numberButtons As New Dictionary(Of Integer, Button)()
    Private _kingNumber As Integer = 0

    ' -------------------------------------------------------------------------
    ' Output
    ' -------------------------------------------------------------------------
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

    ' -------------------------------------------------------------------------
    ' Group colors: index 0 = unassigned, 1-5 = groups
    ' -------------------------------------------------------------------------
    Private Shared ReadOnly GroupColors As Color() = {
        Color.FromKnownColor(KnownColor.Control),
        Color.LightSkyBlue,
        Color.PaleGreen,
        Color.LightSalmon,
        Color.Plum,
        Color.LightGoldenrodYellow
    }

    Private Shared ReadOnly GroupLabels As String() = {
        "", "G1", "G2", "G3", "G4", "G5"
    }

    ' -------------------------------------------------------------------------
    ' Controls
    ' -------------------------------------------------------------------------
    Private WithEvents BtnOk As New Button()
    Private WithEvents ChkKingTicket As New CheckBox()
    Private ReadOnly FlpNumbers As New FlowLayoutPanel()
    Private ReadOnly LvGroups As New ListView()
    Private ReadOnly LvSubTickets As New ListView()
    Private ReadOnly LblSummary As New Label()
    Private ReadOnly LblKingStatus As New Label()

    ' -------------------------------------------------------------------------
    ' Factory
    ' -------------------------------------------------------------------------
    ''' <summary>
    ''' Opens the Way Ticket setup dialog. Returns the configured form on OK,
    ''' or Nothing on Cancel.
    ''' </summary>
    Public Shared Function ShowSetup(owner As Form, selectedNumbers As IEnumerable(Of Integer), betAmount As Decimal) As FrmWayTicket
        Dim frm As New FrmWayTicket(selectedNumbers, betAmount)
        If frm.ShowDialog(owner) = DialogResult.OK Then
            Return frm
        End If
        frm.Dispose()
        Return Nothing
    End Function

    ' -------------------------------------------------------------------------
    ' Constructor
    ' -------------------------------------------------------------------------
    Public Sub New(selectedNumbers As IEnumerable(Of Integer), betAmount As Decimal)
        InitializeComponent()  ' satisfies BC40054; VB-generated stub for code-only form
        _inputNumbers = selectedNumbers.OrderBy(Function(n) n).ToList()
        _betAmount = betAmount
        For Each n In _inputNumbers
            _assignments(n) = 0
        Next
        BuildForm()
    End Sub

    ' -------------------------------------------------------------------------
    ' Form construction  (Designer.vb provides Dispose + InitializeComponent stub only)
    ' -------------------------------------------------------------------------
    ' Layout constants — all absolute, no DockStyle, so nothing overlaps
    Private Const FrmW As Integer = 540

    Private Const FrmH As Integer = 700
    Private Const RightX As Integer = 322   ' x-start of right panel
    Private Const RightW As Integer = 210   ' width of right panel

    Private Sub BuildForm()
        Me.Text = "Way Ticket - Group Setup"
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.StartPosition = FormStartPosition.CenterParent
        Me.ClientSize = New Size(FrmW, FrmH)
        Me.Font = New Font("Segoe UI", 9)

        ' ?? Instruction bar  (y=0..44) ????????????????????????????
        Dim lblInstr As New Label With {
            .Location = New Point(0, 0),
            .Size = New Size(FrmW, 44),
            .TextAlign = ContentAlignment.MiddleCenter,
            .Font = New Font("Segoe UI", 9),
            .BackColor = Color.SteelBlue,
            .ForeColor = Color.White,
            .Text = "Click to assign groups (G1-G5).  Check 'King Ticket' to designate one number as King ♛—it joins every sub-ticket automatically."
        }

        ' ?? Number buttons  (x=8,y=52 .. w=306,h=220) ????????????
        FlpNumbers.Location = New Point(8, 52)
        FlpNumbers.Size = New Size(306, 306)
        FlpNumbers.BorderStyle = BorderStyle.FixedSingle
        FlpNumbers.AutoScroll = True
        FlpNumbers.BackColor = Color.WhiteSmoke
        FlpNumbers.Padding = New Padding(4)
        FlpNumbers.WrapContents = True

        For Each n In _inputNumbers
            Dim captured = n
            Dim btn As New Button With {
                .Size = New Size(66, 50),
                .Font = New Font("Segoe UI", 10, FontStyle.Bold),
                .Text = n.ToString(),
                .BackColor = GroupColors(0),
                .FlatStyle = FlatStyle.Flat
            }
            btn.FlatAppearance.BorderSize = 2
            btn.Tag = captured
            AddHandler btn.Click, AddressOf NumberButton_Click
            _numberButtons(n) = btn
            FlpNumbers.Controls.Add(btn)
        Next

        ' ?? Group summary header  (y=52..74) ??????????????????????
        Dim lblGrpHeader As New Label With {
            .Location = New Point(RightX, 52),
            .Size = New Size(RightW, 22),
            .Text = "Group Summary",
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleCenter,
            .BackColor = Color.DarkSlateGray,
            .ForeColor = Color.White
        }

        ' ?? Group list view  (y=74..274) ??????????????????????????
        LvGroups.Location = New Point(RightX, 74)
        LvGroups.Size = New Size(RightW, 160)
        LvGroups.View = View.Details
        LvGroups.FullRowSelect = True
        LvGroups.GridLines = True
        LvGroups.MultiSelect = False
        LvGroups.HeaderStyle = ColumnHeaderStyle.Nonclickable
        LvGroups.Columns.Add("Group", 55)
        LvGroups.Columns.Add("Numbers", 149)

        ' ?? Color legend  (y=282..388) ????????????????????????????
        Dim pnlLegend As New Panel With {
            .Location = New Point(RightX, 242),
            .Size = New Size(RightW, 193),
            .BorderStyle = BorderStyle.FixedSingle,
            .BackColor = Color.WhiteSmoke
        }
        Dim lblLegendTitle As New Label With {
            .Location = New Point(0, 0),
            .Size = New Size(RightW, 20),
            .Text = "Group Colors",
            .Font = New Font("Segoe UI", 8, FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleCenter,
            .BackColor = Color.SlateGray,
            .ForeColor = Color.White
        }
        pnlLegend.Controls.Add(lblLegendTitle)

        Dim groupNames As String() = {"G1 ◆ Blue", "G2 ◆ Green", "G3 ◆ Salmon", "G4 ◆ Plum", "G5 ◆ Yellow"}
        For i = 1 To 5
            pnlLegend.Controls.Add(New Label With {
                .Size = New Size(16, 22),
                .Location = New Point(8, 22 + (i - 1) * 28),
                .BackColor = GroupColors(i),
                .BorderStyle = BorderStyle.FixedSingle
            })
            pnlLegend.Controls.Add(New Label With {
                .Size = New Size(170, 22),
                .Location = New Point(30, 22 + (i - 1) * 28),
                .Text = groupNames(i - 1),
                .Font = New Font("Segoe UI", 7.5F),
                .TextAlign = ContentAlignment.MiddleLeft
            })
        Next
        pnlLegend.Controls.Add(New Label With {
            .Size = New Size(16, 22),
            .Location = New Point(8, 162),
            .BackColor = Color.Goldenrod,
            .BorderStyle = BorderStyle.FixedSingle
        })
        pnlLegend.Controls.Add(New Label With {
            .Size = New Size(170, 22),
            .Location = New Point(30, 162),
            .Text = "♛ King (added to all sub-tickets)",
            .Font = New Font("Segoe UI", 7.5F),
            .TextAlign = ContentAlignment.MiddleLeft
        })

        ' Sub-ticket preview header  (y=442..463)
        Dim lblPreviewHeader As New Label With {
            .Location = New Point(RightX, 442),
            .Size = New Size(RightW, 22),
            .Text = "Sub-ticket Preview",
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleCenter,
            .BackColor = Color.DarkSlateGray,
            .ForeColor = Color.White
        }

        LvSubTickets.Location = New Point(RightX, 466)
        LvSubTickets.Size = New Size(RightW, 152)
        LvSubTickets.View = View.Details
        LvSubTickets.FullRowSelect = True
        LvSubTickets.GridLines = True
        LvSubTickets.MultiSelect = False
        LvSubTickets.HeaderStyle = ColumnHeaderStyle.Nonclickable
        LvSubTickets.Columns.Add("Sub-ticket", 72)
        LvSubTickets.Columns.Add("Numbers", 132)

        ' King Ticket (left side, y=366..430)
        ChkKingTicket.Location = New Point(8, 366)
        ChkKingTicket.Size = New Size(306, 26)
        ChkKingTicket.Text = "♛  King Ticket — designate one number as King"
        ChkKingTicket.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        ChkKingTicket.BackColor = Color.WhiteSmoke

        LblKingStatus.Location = New Point(8, 398)
        LblKingStatus.Size = New Size(306, 26)
        LblKingStatus.TextAlign = ContentAlignment.MiddleCenter
        LblKingStatus.Font = New Font("Segoe UI", 8.5F)
        LblKingStatus.BackColor = Color.Transparent
        LblKingStatus.Text = String.Empty

        ' ?? OK / Cancel buttons  (y=490) ?????????????????????
        BtnOk.Location = New Point(RightX, 630)
        BtnOk.Size = New Size(100, 34)
        BtnOk.Text = "Play Ways"
        BtnOk.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        BtnOk.Enabled = False
        BtnOk.BackColor = Color.ForestGreen
        BtnOk.ForeColor = Color.White
        BtnOk.FlatStyle = FlatStyle.Flat

        Dim btnCancel As New Button With {
            .Location = New Point(RightX + 108, 630),
            .Size = New Size(100, 34),
            .Text = "Cancel",
            .Font = New Font("Segoe UI", 10),
            .DialogResult = DialogResult.Cancel
        }
        Me.CancelButton = btnCancel

        ' ?? Summary bar  (y=530) ?????????????????????????????
        LblSummary.Location = New Point(0, 668)
        LblSummary.Size = New Size(FrmW, 32)
        LblSummary.TextAlign = ContentAlignment.MiddleCenter
        LblSummary.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        LblSummary.BackColor = Color.DarkSlateGray
        LblSummary.ForeColor = Color.White
        LblSummary.Text = "Assign all numbers to equal-sized groups to continue."

        Me.Controls.Add(lblInstr)
        Me.Controls.Add(FlpNumbers)
        Me.Controls.Add(ChkKingTicket)
        Me.Controls.Add(LblKingStatus)
        Me.Controls.Add(lblGrpHeader)
        Me.Controls.Add(LvGroups)
        Me.Controls.Add(pnlLegend)
        Me.Controls.Add(lblPreviewHeader)
        Me.Controls.Add(LvSubTickets)
        Me.Controls.Add(BtnOk)
        Me.Controls.Add(btnCancel)
        Me.Controls.Add(LblSummary)
    End Sub

    ' -------------------------------------------------------------------------
    ' Number button click
    ' -------------------------------------------------------------------------
    Private Sub NumberButton_Click(sender As Object, e As EventArgs)
        Dim btn = TryCast(sender, Button)
        If btn Is Nothing Then Return

        Dim number = CInt(btn.Tag)

        ' Clicking the current King always unsets it
        If number = _kingNumber Then
            ClearKing()
            Return
        End If

        ' King designation mode: clicking any number makes it King
        If ChkKingTicket.Checked Then
            SetKing(number, btn)
            Return
        End If

        ' Normal group cycling (King number cannot be re-cycled)
        Dim current = _assignments(number)
        Dim usedGroups = _assignments _
            .Where(Function(kvp) kvp.Key <> _kingNumber AndAlso kvp.Value > 0) _
            .Select(Function(kvp) kvp.Value).Distinct().Count()
        Dim maxGroup = Math.Min(usedGroups + 1, 5)

        Dim nextGroup = current + 1
        If nextGroup > maxGroup Then nextGroup = 0

        _assignments(number) = nextGroup
        btn.BackColor = GroupColors(nextGroup)

        If nextGroup = 0 Then
            btn.Text = number.ToString()
            btn.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        Else
            btn.Text = number.ToString() & Environment.NewLine & GroupLabels(nextGroup)
            btn.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        End If

        RefreshGroupList()
        RefreshSummary()
    End Sub

    ' -------------------------------------------------------------------------
    ' Refresh group list view
    ' -------------------------------------------------------------------------
    Private Sub RefreshGroupList()
        LvGroups.BeginUpdate()
        LvGroups.Items.Clear()

        If _kingNumber > 0 Then
            Dim kingItem As New ListViewItem("♛ King")
            kingItem.SubItems.Add(_kingNumber.ToString())
            kingItem.BackColor = Color.Goldenrod
            LvGroups.Items.Add(kingItem)
        End If

        Dim grps = _assignments _
            .Where(Function(kvp) kvp.Key <> _kingNumber AndAlso kvp.Value > 0) _
            .GroupBy(Function(kvp) kvp.Value) _
            .OrderBy(Function(g) g.Key)
        For Each grp In grps
            Dim nums = grp.Select(Function(kvp) kvp.Key).OrderBy(Function(n) n)
            Dim item As New ListViewItem($"G{grp.Key}")
            item.SubItems.Add(String.Join(", ", nums))
            item.BackColor = GroupColors(grp.Key)
            LvGroups.Items.Add(item)
        Next

        LvGroups.EndUpdate()
        RefreshSubTicketPreview()
    End Sub

    ' -------------------------------------------------------------------------
    ' Refresh sub-ticket preview
    ' -------------------------------------------------------------------------
    Private Sub RefreshSubTicketPreview()
        LvSubTickets.BeginUpdate()
        LvSubTickets.Items.Clear()

        Dim grps = _assignments _
            .Where(Function(kvp) kvp.Key <> _kingNumber AndAlso kvp.Value > 0) _
            .GroupBy(Function(kvp) kvp.Value) _
            .OrderBy(Function(g) g.Key)

        Dim subIndex = 1
        For Each grp In grps
            Dim nums = grp.Select(Function(kvp) kvp.Key).OrderBy(Function(n) n).ToList()
            Dim item As New ListViewItem($"ST {subIndex}")
            Dim numsText = String.Join(", ", nums)
            If _kingNumber > 0 Then
                numsText &= $"  ♕{_kingNumber}"
            End If
            item.SubItems.Add(numsText)
            item.BackColor = GroupColors(grp.Key)
            LvSubTickets.Items.Add(item)
            subIndex += 1
        Next

        LvSubTickets.EndUpdate()
    End Sub

    ' -------------------------------------------------------------------------
    ' Validate and refresh the summary bar / OK button
    ' -------------------------------------------------------------------------
    Private Sub RefreshSummary()
        If ChkKingTicket.Checked AndAlso _kingNumber = 0 Then
            LblSummary.Text = "Click a number to designate it King ♛."
            LblSummary.BackColor = Color.DarkSlateGray
            BtnOk.Enabled = False
            Return
        End If

        Dim unassigned = _assignments _
            .Where(Function(kvp) kvp.Key <> _kingNumber AndAlso kvp.Value = 0).Count()
        Dim grps = _assignments _
            .Where(Function(kvp) kvp.Key <> _kingNumber AndAlso kvp.Value > 0) _
            .GroupBy(Function(kvp) kvp.Value) _
            .ToList()

        If unassigned > 0 Then
            LblSummary.Text = $"{unassigned} number(s) still unassigned."
            LblSummary.BackColor = Color.DarkSlateGray
            BtnOk.Enabled = False
            Return
        End If

        If grps.Count < 2 Then
            LblSummary.Text = "Need at least 2 groups."
            LblSummary.BackColor = Color.Firebrick
            BtnOk.Enabled = False
            Return
        End If

        Dim targetSize = grps(0).Count()
        Dim allEqual = grps.All(Function(g) g.Count() = targetSize)

        If Not allEqual Then
            LblSummary.Text = "All groups must be the same size."
            LblSummary.BackColor = Color.Firebrick
            BtnOk.Enabled = False
            Return
        End If

        Dim subTickets = grps.Count
        Dim totalCost = subTickets * _betAmount
        If _kingNumber > 0 Then
            Dim spotCount = targetSize + 1
            LblSummary.Text = $"{subTickets} sub-tickets  ({spotCount}-spot w/ King ♛)  -  Total: {totalCost:C2}"
        Else
            LblSummary.Text = $"{subTickets} sub-tickets  ({targetSize}-spot each)  -  Total: {totalCost:C2}"
        End If
        LblSummary.BackColor = Color.ForestGreen
        BtnOk.Enabled = True
    End Sub

    ' -------------------------------------------------------------------------
    ' OK
    ' -------------------------------------------------------------------------
    Private Sub BtnOk_Click(sender As Object, e As EventArgs) Handles BtnOk.Click
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    ' -------------------------------------------------------------------------
    ' King Ticket: designate / clear king
    ' -------------------------------------------------------------------------
    Private Sub ChkKingTicket_CheckedChanged(sender As Object, e As EventArgs) Handles ChkKingTicket.CheckedChanged
        UpdateKingStatus()
        RefreshSummary()
    End Sub

    Private Sub SetKing(number As Integer, btn As Button)
        If _kingNumber > 0 Then
            Dim oldKing = _kingNumber
            _kingNumber = 0
            _assignments(oldKing) = 0
            Dim oldBtn = _numberButtons(oldKing)
            oldBtn.BackColor = GroupColors(0)
            oldBtn.Text = oldKing.ToString()
            oldBtn.Font = New Font("Segoe UI", 10, FontStyle.Bold)
            oldBtn.FlatAppearance.BorderColor = Color.Empty
        End If

        _kingNumber = number
        _assignments(number) = 0
        btn.BackColor = Color.Goldenrod
        btn.Text = $"{number}{Environment.NewLine}♛"
        btn.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        btn.FlatAppearance.BorderColor = Color.DarkGoldenrod

        RefreshGroupList()
        RefreshSummary()
        UpdateKingStatus()
    End Sub

    Private Sub ClearKing()
        If _kingNumber = 0 Then Return

        Dim number = _kingNumber
        _kingNumber = 0
        _assignments(number) = 0

        Dim btn = _numberButtons(number)
        btn.BackColor = GroupColors(0)
        btn.Text = number.ToString()
        btn.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        btn.FlatAppearance.BorderColor = Color.Empty

        RefreshGroupList()
        RefreshSummary()
        UpdateKingStatus()
    End Sub

    Private Sub UpdateKingStatus()
        If _kingNumber > 0 Then
            LblKingStatus.Text = $"King: {_kingNumber} ♛  — click to unset"
            LblKingStatus.BackColor = Color.Goldenrod
            LblKingStatus.ForeColor = Color.Black
        ElseIf ChkKingTicket.Checked Then
            LblKingStatus.Text = "Click a number to designate it King ♛"
            LblKingStatus.BackColor = Color.DarkGoldenrod
            LblKingStatus.ForeColor = Color.White
        Else
            LblKingStatus.Text = String.Empty
            LblKingStatus.BackColor = Color.Transparent
            LblKingStatus.ForeColor = SystemColors.ControlText
        End If
    End Sub

End Class