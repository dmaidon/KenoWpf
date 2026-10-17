' Last Edit: 2026-03-10 - SatDir / LogDir / SetDir marked ReadOnly. Moved to Keno.Core; Application.StartupPath replaced with AppContext.BaseDirectory.
Imports System.IO
Imports System.Text

Public Module Globals

    Public ReadOnly DatDir As String = Path.Combine(AppContext.BaseDirectory, "Data")
    Public ReadOnly LogDir As String = Path.Combine(AppContext.BaseDirectory, "Logs")
    Public ReadOnly SetDir As String = Path.Combine(AppContext.BaseDirectory, "Settings")

    Public ReadOnly cpy As String = "© 2026 PAROLE Software"

    ''' <summary>Writes via a temp file then replaces the target, so a crash mid-write cannot truncate it.</summary>
    Public Sub WriteAllTextAtomic(path As String, contents As String)
        Dim tmp = path & ".tmp"
        File.WriteAllText(tmp, contents)
        If File.Exists(path) Then
            File.Replace(tmp, path, Nothing)
        Else
            File.Move(tmp, path)
        End If
    End Sub

    Private _logsCleaned As Boolean

    ''' <summary>Moves an unreadable data file aside so the next save does not destroy it.</summary>
    Public Sub BackupCorruptFile(path As String)
        Try
            If File.Exists(path) Then File.Copy(path, path & ".bad", True)
        Catch
        End Try
    End Sub

    Public Sub LogError(ex As Exception, context As String)
        Try
            Directory.CreateDirectory(LogDir)
            If Not _logsCleaned Then
                _logsCleaned = True
                CleanupOldLogs()
            End If

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
