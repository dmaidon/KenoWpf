' Last Edit: 2026-03-07 - All-time stats store persisted to all-time-summary.json. Moved to Keno.Core.
Imports System.IO
Imports System.Text.Json

Public Module AllTimeSummaryStore

    Private ReadOnly SummaryFilePath As String = Path.Combine(DatDir, "all-time-summary.json")
    Private ReadOnly JsonOptions As New JsonSerializerOptions With {.WriteIndented = True}

    Public Sub EnsureAllTimeSummary()
        Try
            Directory.CreateDirectory(DatDir)
            If Not File.Exists(SummaryFilePath) Then
                SaveSummary(New AllTimeSummary())
            End If
        Catch ex As Exception
            LogError(ex, NameOf(EnsureAllTimeSummary))
        End Try
    End Sub

    Public Sub IncrementSessions()
        Try
            Dim s = LoadSummary()
            s.SessionsPlayed += 1
            SaveSummary(s)
        Catch ex As Exception
            LogError(ex, NameOf(IncrementSessions))
        End Try
    End Sub

    Public Sub RecordGame(betAmount As Decimal, payout As Decimal)
        Try
            Dim s = LoadSummary()
            s.TotalGamesPlayed += 1
            s.TotalWagered += betAmount
            s.TotalPayout += payout
            If payout > 0D Then s.TotalWins += 1
            If payout > s.BestSinglePayout Then s.BestSinglePayout = payout
            SaveSummary(s)
        Catch ex As Exception
            LogError(ex, NameOf(RecordGame))
        End Try
    End Sub

    Public Sub RecordFreeGameEarned()
        Try
            Dim s = LoadSummary()
            s.TotalFreeGamesEarned += 1
            SaveSummary(s)
        Catch ex As Exception
            LogError(ex, NameOf(RecordFreeGameEarned))
        End Try
    End Sub

    Public Sub RecordJackpotWon()
        Try
            Dim s = LoadSummary()
            s.JackpotsWon += 1
            SaveSummary(s)
        Catch ex As Exception
            LogError(ex, NameOf(RecordJackpotWon))
        End Try
    End Sub

    Public Function LoadSummary() As AllTimeSummary
        Try
            If Not File.Exists(SummaryFilePath) Then Return New AllTimeSummary()
            Dim json = File.ReadAllText(SummaryFilePath)
            Return If(JsonSerializer.Deserialize(Of AllTimeSummary)(json), New AllTimeSummary())
        Catch ex As Exception
            LogError(ex, NameOf(LoadSummary))
            Return New AllTimeSummary()
        End Try
    End Function

    Private Sub SaveSummary(summary As AllTimeSummary)
        Try
            Directory.CreateDirectory(DatDir)
            File.WriteAllText(SummaryFilePath, JsonSerializer.Serialize(summary, JsonOptions))
        Catch ex As Exception
            LogError(ex, NameOf(SaveSummary))
        End Try
    End Sub

    Public Class AllTimeSummary
        Public Property SessionsPlayed As Integer
        Public Property TotalGamesPlayed As Integer
        Public Property TotalWagered As Decimal
        Public Property TotalPayout As Decimal
        Public Property TotalWins As Integer
        Public Property BestSinglePayout As Decimal
        Public Property TotalFreeGamesEarned As Integer
        Public Property JackpotsWon As Integer
    End Class

End Module
