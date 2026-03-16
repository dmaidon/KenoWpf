' Last Edit: 2026-03-06 - Added streak tracking (CurrentWinStreak, CurrentLossStreak, BestWinStreak). Moved to Keno.Core.
Imports System.IO
Imports System.Text.Json

Public Module DrawStatsStore
    Private Const StatsFileName As String = "draw-stats.json"
    Private ReadOnly StatsFilePath As String = Path.Combine(DatDir, StatsFileName)
    Private Const RecentGameLimit As Integer = 15

    Private ReadOnly JsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True
    }

    Public Sub EnsureDrawStats()
        Try
            Directory.CreateDirectory(DatDir)

            If File.Exists(StatsFilePath) Then
                Return
            End If

            Dim stats = New DrawStats()
            SaveStats(stats)
        Catch ex As Exception
            LogError(ex, NameOf(EnsureDrawStats))
        End Try
    End Sub

    Public Function RecordDraw(picks As IEnumerable(Of Integer), Optional won As Boolean = False) As DrawStats
        Try
            Dim stats = LoadStats()
            stats.GamesPlayed += 1

            For Each pick In picks
                If pick < 1 OrElse pick > 80 Then
                    Continue For
                End If

                stats.Counts(pick) += 1
            Next

            stats.RecentDraws.Add(picks.ToList())
            While stats.RecentDraws.Count > RecentGameLimit
                stats.RecentDraws.RemoveAt(0)
            End While

            If won Then
                stats.CurrentWinStreak += 1
                stats.CurrentLossStreak = 0
                If stats.CurrentWinStreak > stats.BestWinStreak Then
                    stats.BestWinStreak = stats.CurrentWinStreak
                End If
            Else
                stats.CurrentLossStreak += 1
                stats.CurrentWinStreak = 0
            End If

            SaveStats(stats)
            Return stats
        Catch ex As Exception
            LogError(ex, NameOf(RecordDraw))
            Return New DrawStats()
        End Try
    End Function

    Public Function LoadStats() As DrawStats
        Try
            Directory.CreateDirectory(DatDir)

            If Not File.Exists(StatsFilePath) Then
                Dim defaults = New DrawStats()
                SaveStats(defaults)
                Return defaults
            End If

            Dim json = File.ReadAllText(StatsFilePath)
            Dim stats = JsonSerializer.Deserialize(Of DrawStats)(json)

            If stats Is Nothing Then
                stats = New DrawStats()
                SaveStats(stats)
            End If

            If stats.RecentDraws Is Nothing Then
                stats.RecentDraws = New List(Of List(Of Integer))()
            End If

            If stats.Counts Is Nothing Then
                stats.Counts = New Dictionary(Of Integer, Integer)()
            End If

            For number = 1 To 80
                If Not stats.Counts.ContainsKey(number) Then
                    stats.Counts(number) = 0
                End If
            Next

            Return stats
        Catch ex As Exception
            LogError(ex, NameOf(LoadStats))
            Return New DrawStats()
        End Try
    End Function

    Public Sub SaveStats(stats As DrawStats)
        Try
            Directory.CreateDirectory(DatDir)

            Dim json = JsonSerializer.Serialize(stats, JsonOptions)

            File.WriteAllText(StatsFilePath, json)
        Catch ex As Exception
            LogError(ex, NameOf(SaveStats))
        End Try
    End Sub

    Public Class DrawStats
        Public Property GamesPlayed As Integer
        Public Property Counts As Dictionary(Of Integer, Integer)
        Public Property RecentDraws As List(Of List(Of Integer))
        Public Property CurrentWinStreak As Integer
        Public Property CurrentLossStreak As Integer
        Public Property BestWinStreak As Integer

        Public Sub New()
            GamesPlayed = 0
            Counts = New Dictionary(Of Integer, Integer)()
            RecentDraws = New List(Of List(Of Integer))()
            CurrentWinStreak = 0
            CurrentLossStreak = 0
            BestWinStreak = 0
            For number = 1 To 80
                Counts(number) = 0
            Next
        End Sub

    End Class

End Module
