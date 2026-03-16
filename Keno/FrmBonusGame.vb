' Last Edit: 2026-03-06 - Auto-close after 7 seconds when mouse is not hovering.
Public Class FrmBonusGame
    Inherits Form

    Private _picks As Integer

    Private WithEvents BtnClose As New Button()
    Private ReadOnly LblHeader As New Label()
    Private ReadOnly LblInfo As New Label()
    Private ReadOnly LblCredited As New Label()
    Private WithEvents AutoCloseTimer As New Timer() With {.Interval = 7000}

    Public Shared Sub ShowBonus(owner As Form, picks As Integer)
        Using frm As New FrmBonusGame()
            frm._picks = picks
            frm.ShowDialog(owner)
        End Using
    End Sub

    Public Sub New()
        Me.Text = "Bonus!"
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.StartPosition = FormStartPosition.CenterParent
        Me.ClientSize = New Size(260, 176)
        Me.Font = New Font("Segoe UI", 9)

        LblHeader.Dock = DockStyle.Top
        LblHeader.Height = 44
        LblHeader.TextAlign = ContentAlignment.MiddleCenter
        LblHeader.Font = New Font("Segoe UI", 14, FontStyle.Bold)
        LblHeader.BackColor = Color.SeaGreen
        LblHeader.ForeColor = Color.White
        LblHeader.Text = "Bonus Free Game!"

        LblInfo.Dock = DockStyle.Top
        LblInfo.Height = 52
        LblInfo.TextAlign = ContentAlignment.MiddleCenter
        LblInfo.Font = New Font("Segoe UI", 9)

        LblCredited.Dock = DockStyle.Top
        LblCredited.Height = 28
        LblCredited.TextAlign = ContentAlignment.MiddleCenter
        LblCredited.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        LblCredited.ForeColor = Color.SeaGreen
        LblCredited.Text = "1 free $2 game added!"

        BtnClose.Dock = DockStyle.Bottom
        BtnClose.Height = 36
        BtnClose.Text = "Close"
        BtnClose.Font = New Font("Segoe UI", 10, FontStyle.Bold)

        Me.AcceptButton = BtnClose

        ' Add in reverse visual order — last added DockStyle.Top lands topmost
        Me.Controls.Add(LblCredited)
        Me.Controls.Add(LblInfo)
        Me.Controls.Add(LblHeader)
        Me.Controls.Add(BtnClose)
    End Sub

    Private Sub FrmBonusGame_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LblInfo.Text = $"Pick {_picks} with no matches earns" &
                       Environment.NewLine &
                       "1 free $2 game of any kind."
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