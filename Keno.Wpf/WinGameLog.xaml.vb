' Last Edit: 2026-03-15 - WinGameLog: log viewer, Clear File with confirmation, Close.
Class WinGameLog

    Friend Shared Sub ShowWindow(owner As Window)
        Dim win As New WinGameLog With {
            .Owner = owner
        }
        win.ShowDialog()
    End Sub

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub WinGameLog_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        LoadLog()
    End Sub

    Private Sub LoadLog()
        Dim content = ReadLog()
        TbxLog.Text = If(String.IsNullOrEmpty(content), "(No log entries yet.)", content)
        TbxLog.CaretIndex = TbxLog.Text.Length
        TbxLog.ScrollToEnd()
    End Sub

    Private Sub BtnClearFile_Click(sender As Object, e As RoutedEventArgs)
        Dim result = MessageBox.Show(
            "Are you sure you want to clear the game log file? This cannot be undone.",
            "Clear Game Log",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning)

        If result = MessageBoxResult.Yes Then
            ClearLog()
            LoadLog()
        End If
    End Sub

    Private Sub BtnExportCsv_Click(sender As Object, e As RoutedEventArgs)
        Dim dlg As New Microsoft.Win32.SaveFileDialog() With {
            .Filter = "CSV files (*.csv)|*.csv",
            .FileName = $"keno-game-log-{DateTime.Now:yyyyMMdd}.csv"
        }
        If dlg.ShowDialog(Me) <> True Then Return
        Try
            Dim count = ExportCsv(dlg.FileName)
            MessageBox.Show(Me, $"Exported {count} games to {dlg.FileName}.", "Export CSV", MessageBoxButton.OK, MessageBoxImage.Information)
        Catch ex As Exception
            LogError(ex, NameOf(BtnExportCsv_Click))
            MessageBox.Show(Me, $"Export failed: {ex.Message}", "Export CSV", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
        Close()
    End Sub

End Class