' Last Edit: 2026-03-20 06:43 AM - AppendFreeGameBatch: log free-game sessions as a consecutive-game block identical in shape to AppendBatch.
Imports System.IO

Public Module GameLogStore

    Private ReadOnly LogFilePath As String = Path.Combine(DatDir, "game-log.txt")

    Public Sub AppendGame(gameMode As String, betAmount As Decimal, matched As Integer, payout As Decimal, Optional multiplier As Integer = 1, Optional freeGameAwarded As Boolean = False, Optional firstLastBonus As Decimal = 0D)
        Try
            Directory.CreateDirectory(DatDir)
            Dim matchedText = If(matched < 0, "N/A", matched.ToString())
            Dim flbNote = If(firstLastBonus > 0D, $" | FirstLast: {firstLastBonus:C2}", "")
            Dim fgNote = If(freeGameAwarded, " | Free Game", "")
            Dim line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {gameMode} | Bet: {betAmount:C2} | Match: {matchedText} | Multiplier: {multiplier}x | Payout: {payout:C2}{flbNote}{fgNote}"
            File.AppendAllText(LogFilePath, line & Environment.NewLine)
        Catch ex As Exception
            LogError(ex, NameOf(AppendGame))
        End Try
    End Sub

    Public Sub AppendBatch(gameMode As String, betAmount As Decimal, results As IEnumerable(Of (Matched As Integer, Payout As Decimal, FreeGameAwarded As Boolean, Multiplier As Integer, FirstLastBonus As Decimal)), Optional bonus As Decimal = 1D)
        Try
            Directory.CreateDirectory(DatDir)
            Dim list = results.ToList()
            Dim lines As New List(Of String)()
            Dim timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            lines.Add($"[ {list.Count}-Consecutive Games")
            For Each r In list
                Dim flbNote = If(r.FirstLastBonus > 0D, $" | FirstLast: {r.FirstLastBonus:C2}", "")
                Dim fgNote = If(r.FreeGameAwarded, " | Free Game", "")
                lines.Add($"{timestamp} | {gameMode} | Bet: {betAmount:C2} | Match: {r.Matched} | Multiplier: {r.Multiplier}x | Bonus: {bonus:0.##}x | Payout: {r.Payout:C2}{flbNote}{fgNote}")
            Next
            Dim winList = list.Where(Function(r) r.Payout > 0D).ToList()
            Dim winCount = winList.Count
            lines.Add("Final Payout Calculation:")
            If winCount = 0 Then
                lines.Add("  0 wins")
                lines.Add("  Total Payout: $0.00")
            Else
                Dim winSubtotal = winList.Sum(Function(r) r.Payout)
                Dim flSubtotal = winList.Sum(Function(r) r.FirstLastBonus)
                Dim regularSubtotal = winSubtotal - flSubtotal
                Dim total = regularSubtotal * bonus + flSubtotal
                lines.Add($"  {winCount} win(s) subtotal = {winSubtotal:C2}")
                lines.Add($"  {regularSubtotal:C2} × {bonus:0.##}x bonus = {regularSubtotal * bonus:C2}")
                If flSubtotal > 0D Then lines.Add($"  + {flSubtotal:C2} flat (1st/Last bonus)")
                lines.Add($"  Total Payout: {total:C2}")
            End If
            lines.Add("End Group ]")
            File.AppendAllLines(LogFilePath, lines)
        Catch ex As Exception
            LogError(ex, NameOf(AppendBatch))
        End Try
    End Sub

    Public Sub AppendFreeGameBatch(gameMode As String, betAmount As Decimal, results As IEnumerable(Of (Matched As Integer, Payout As Decimal)))
        Try
            Directory.CreateDirectory(DatDir)
            Dim list = results.ToList()
            Dim lines As New List(Of String)()
            Dim timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            lines.Add($"[ {list.Count}-Consecutive Free Game{If(list.Count = 1, "", "s")}")
            For Each r In list
                lines.Add($"{timestamp} | {gameMode} | Bet: {betAmount:C2} | Match: {r.Matched} | Payout: {r.Payout:C2}")
            Next
            Dim winList = list.Where(Function(r) r.Payout > 0D).ToList()
            Dim winCount = winList.Count
            lines.Add("Final Payout Calculation:")
            If winCount = 0 Then
                lines.Add("  0 wins")
                lines.Add("  Total Payout: $0.00")
            Else
                Dim total = winList.Sum(Function(r) r.Payout)
                lines.Add($"  {winCount} win(s) subtotal = {total:C2}")
                lines.Add($"  Total Payout: {total:C2}")
            End If
            lines.Add("End Group ]")
            File.AppendAllLines(LogFilePath, lines)
        Catch ex As Exception
            LogError(ex, NameOf(AppendFreeGameBatch))
        End Try
    End Sub

    ''' <summary>Returns the full text of the game log, or an empty string if the file does not exist.</summary>
    Public Function ReadLog() As String
        Try
            If File.Exists(LogFilePath) Then
                Return File.ReadAllText(LogFilePath)
            End If
        Catch ex As Exception
            LogError(ex, NameOf(ReadLog))
        End Try
        Return String.Empty
    End Function

    ''' <summary>Clears all content from the game log file.</summary>
    Public Sub ClearLog()
        Try
            If File.Exists(LogFilePath) Then
                File.WriteAllText(LogFilePath, String.Empty)
            End If
        Catch ex As Exception
            LogError(ex, NameOf(ClearLog))
        End Try
    End Sub

End Module
