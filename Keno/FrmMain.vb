' Last Edit: 2026-03-10 - StsGameLog_Click opens FrmGameLog viewer.
Public Class FrmMain

    Private Sub StsHelp_Click(sender As Object, e As EventArgs) Handles StsHelp.Click
        Using dlg As New FrmKenoHelp()
            dlg.ShowDialog(Me)
        End Using
    End Sub

    Private Sub StsGameLog_Click(sender As Object, e As EventArgs) Handles StsGameLog.Click
        Using dlg As New FrmGameLog()
            dlg.ShowDialog(Me)
        End Using
    End Sub

End Class