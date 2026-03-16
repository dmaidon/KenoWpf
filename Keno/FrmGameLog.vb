' Last Edit: 2026-03-10 - FrmGameLog implemented; displays game-log.txt with Close and Clear File buttons.
Public Class FrmGameLog

    Private Sub FrmGameLog_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadLog()
    End Sub

    Private Sub LoadLog()
        Dim content = ReadLog()
        RtbContent.Text = If(String.IsNullOrEmpty(content), "(No log entries yet.)", content)
        RtbContent.SelectionStart = RtbContent.Text.Length
        RtbContent.ScrollToCaret()
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As EventArgs) Handles BtnClose.Click
        Close()
    End Sub

    Private Sub BtnClearFile_Click(sender As Object, e As EventArgs) Handles BtnClearFile.Click
        Dim result = MessageBox.Show(
            "Are you sure you want to clear the game log file? This cannot be undone.",
            "Clear Game Log",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2)

        If result = DialogResult.Yes Then
            ClearLog()
            LoadLog()
        End If
    End Sub

End Class