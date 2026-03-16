' Last Edit: 2026-03-06 - Auto-close after 7 seconds when mouse is not hovering.
Public Class FrmPayoutSchedule
    Inherits Form

    Private _gameType As String = "Regular"
    Private _pickedCount As Integer
    Private _matchedCount As Integer
    Private _payout As Decimal
    Private _betAmount As Decimal

    Private WithEvents BtnClose As New Button()
    Private ReadOnly LvPayouts As New ListView()
    Private ReadOnly LblWin As New Label()
    Private ReadOnly LblGameInfo As New Label()
    Private WithEvents AutoCloseTimer As New Timer() With {.Interval = 7000}

    Public Shared Sub ShowForWin(owner As Form, gameType As String, pickedCount As Integer, matchedCount As Integer, payout As Decimal, betAmount As Decimal)
        Using frm As New FrmPayoutSchedule()
            frm._gameType = gameType
            frm._pickedCount = pickedCount
            frm._matchedCount = matchedCount
            frm._payout = payout
            frm._betAmount = betAmount
            frm.ShowDialog(owner)
        End Using
    End Sub

    Public Sub New()
        Me.Text = "You Won!"
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.StartPosition = FormStartPosition.CenterParent
        Me.ClientSize = New Size(260, 390)
        Me.Font = New Font("Segoe UI", 9)

        LblWin.Dock = DockStyle.Top
        LblWin.Height = 44
        LblWin.TextAlign = ContentAlignment.MiddleCenter
        LblWin.Font = New Font("Segoe UI", 14, FontStyle.Bold)
        LblWin.BackColor = Color.Gold
        LblWin.ForeColor = Color.Black

        LblGameInfo.Dock = DockStyle.Top
        LblGameInfo.Height = 26
        LblGameInfo.TextAlign = ContentAlignment.MiddleCenter
        LblGameInfo.BackColor = Color.DarkGoldenrod
        LblGameInfo.ForeColor = Color.White
        LblGameInfo.Font = New Font("Segoe UI", 9, FontStyle.Bold)

        LvPayouts.Dock = DockStyle.Fill
        LvPayouts.View = View.Details
        LvPayouts.FullRowSelect = True
        LvPayouts.MultiSelect = False
        LvPayouts.GridLines = True
        LvPayouts.HeaderStyle = ColumnHeaderStyle.Nonclickable
        LvPayouts.Columns.Add("Matched", 110)
        LvPayouts.Columns.Add("Win", 128)
        LvPayouts.Enabled = False

        BtnClose.Dock = DockStyle.Bottom
        BtnClose.Height = 36
        BtnClose.Text = "Close"
        BtnClose.Font = New Font("Segoe UI", 8, FontStyle.Bold)

        Me.AcceptButton = BtnClose

        Me.Controls.Add(LvPayouts)
        Me.Controls.Add(LblGameInfo)
        Me.Controls.Add(LblWin)
        Me.Controls.Add(BtnClose)
    End Sub

    Private Sub FrmPayoutSchedule_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LblWin.Text = $"You Won {_payout:C2}!"

        Dim gameTitle As String
        Select Case _gameType
            Case "Regular"
                gameTitle = $"Pick {_pickedCount} Payout Schedule"
            Case "Bullseye"
                gameTitle = "Bullseye (Corners + Center) Payout Schedule"
            Case "TopBottom"
                gameTitle = "Top / Bottom Payout Schedule"
            Case "LeftRight"
                gameTitle = "Left / Right Payout Schedule"
            Case Else
                gameTitle = "Quadrant Payout Schedule"
        End Select

        LblGameInfo.Text = $"{gameTitle}  -  Matched: {_matchedCount}"

        Dim entries As Dictionary(Of Integer, Decimal)
        If _gameType = "Regular" Then
            entries = GetPayoutScheduleEntries(_pickedCount)
        ElseIf _gameType = "Bullseye" Then
            entries = GetBullseyePayoutScheduleEntries()
        Else
            entries = GetAreaPayoutScheduleEntries(_gameType)
        End If

        Dim rows = entries.Where(Function(kvp) kvp.Value > 0D).OrderByDescending(Function(kvp) kvp.Key)

        LvPayouts.BeginUpdate()
        For Each kvp In rows
            Dim winAmount = kvp.Value * _betAmount
            Dim item As New ListViewItem(kvp.Key.ToString())
            item.SubItems.Add(winAmount.ToString("C2"))

            If kvp.Key = _matchedCount Then
                item.BackColor = Color.Gold
                item.ForeColor = Color.Black
                item.Font = New Font(LvPayouts.Font, FontStyle.Bold)
            End If

            LvPayouts.Items.Add(item)
        Next
        LvPayouts.EndUpdate()

        For Each item As ListViewItem In LvPayouts.Items
            If item.BackColor = Color.Gold Then
                item.EnsureVisible()
                Exit For
            End If
        Next

        AutoCloseTimer.Start()
    End Sub

    Private Sub AutoCloseTimer_Tick(sender As Object, e As EventArgs) Handles AutoCloseTimer.Tick
        AutoCloseTimer.Stop()
        If Not Bounds.Contains(Cursor.Position) Then
            DialogResult = DialogResult.Cancel
            Close()
        End If
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As EventArgs) Handles BtnClose.Click
        AutoCloseTimer.Stop()
        Me.Close()
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            AutoCloseTimer.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class