' Last Edit: 2026-03-10 - SatDir / LogDir / SetDir marked ReadOnly. Moved to Keno.Core; Application.StartupPath replaced with AppContext.BaseDirectory.
Imports System.IO
Imports System.Text

Public Module Globals

    Public ReadOnly DatDir As String = Path.Combine(AppContext.BaseDirectory, "Data")
    Public ReadOnly LogDir As String = Path.Combine(AppContext.BaseDirectory, "Logs")
    Public ReadOnly SetDir As String = Path.Combine(AppContext.BaseDirectory, "Settings")

    Public ReadOnly cpy As String = "© 2026 Dennis N. Maidon. All rights reserved."

    Public Sub LogError(ex As Exception, context As String)
        Try
            Directory.CreateDirectory(LogDir)
            CleanupOldLogs()

            Dim fileName = $"err_{DateTime.Now:MMMdd}.log"
            Dim logPath = Path.Combine(LogDir, fileName)
            Dim message = New StringBuilder()
            message.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}")
            message.AppendLine(ex.ToString())
            message.AppendLine()

            File.AppendAllText(logPath, message.ToString())
        Catch
        End Try
    End Sub

    Public Function HasErrorLogForToday() As Boolean
        Try
            Dim fileName = $"err_{DateTime.Now:MMMdd}.log"
            Dim logPath = Path.Combine(LogDir, fileName)

            If Not File.Exists(logPath) Then
                Return False
            End If

            Dim info = New FileInfo(logPath)
            Return info.Length > 0
        Catch
            Return False
        End Try
    End Function

    Public Sub CleanupOldLogs()
        Try
            If Not Directory.Exists(LogDir) Then
                Return
            End If

            Dim cutoff = DateTime.Now.AddDays(-10)
            For Each filePath In Directory.GetFiles(LogDir, "err_*.log")
                Dim info = New FileInfo(filePath)
                If info.LastWriteTime < cutoff Then
                    info.Delete()
                End If
            Next
        Catch
        End Try
    End Sub

End Module
