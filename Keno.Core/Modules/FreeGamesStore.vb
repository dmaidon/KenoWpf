' Last Edit: 2026-03-20 05:25 AM - Added UseFreeGames(count) for atomic batch deduction.
Imports System.IO
Imports System.Text.Json

Public Module FreeGamesStore

    Private ReadOnly SettingsFilePath As String = Path.Combine(DatDir, "free-games.json")
    Private ReadOnly JsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True
    }

    Public Function GetFreeGames() As Integer
        Try
            Return Math.Max(0, LoadSettings().Count)
        Catch ex As Exception
            LogError(ex, NameOf(GetFreeGames))
            Return 0
        End Try
    End Function

    Public Sub AddFreeGame()
        Try
            SaveFreeGames(GetFreeGames() + 1)
        Catch ex As Exception
            LogError(ex, NameOf(AddFreeGame))
        End Try
    End Sub

    Public Sub UseFreeGame()
        Try
            Dim count = GetFreeGames()
            If count > 0 Then
                SaveFreeGames(count - 1)
            End If
        Catch ex As Exception
            LogError(ex, NameOf(UseFreeGame))
        End Try
    End Sub

    ''' <summary>Deducts <paramref name="count"/> free games in a single atomic disk write.</summary>
    Public Sub UseFreeGames(count As Integer)
        Try
            Dim current = GetFreeGames()
            SaveFreeGames(Math.Max(0, current - count))
        Catch ex As Exception
            LogError(ex, NameOf(UseFreeGames))
        End Try
    End Sub

    Private Sub SaveFreeGames(count As Integer)
        Try
            Directory.CreateDirectory(DatDir)
            Dim settings = New FreeGamesSettings With {.Count = Math.Max(0, count)}
            Dim json = JsonSerializer.Serialize(settings, JsonOptions)
            File.WriteAllText(SettingsFilePath, json)
        Catch ex As Exception
            LogError(ex, NameOf(SaveFreeGames))
        End Try
    End Sub

    Private Function LoadSettings() As FreeGamesSettings
        Try
            Directory.CreateDirectory(DatDir)

            If Not File.Exists(SettingsFilePath) Then
                Return New FreeGamesSettings With {.Count = 0}
            End If

            Dim json = File.ReadAllText(SettingsFilePath)
            Dim settings = JsonSerializer.Deserialize(Of FreeGamesSettings)(json)
            Return If(settings, New FreeGamesSettings With {.Count = 0})
        Catch ex As Exception
            LogError(ex, NameOf(LoadSettings))
            Return New FreeGamesSettings With {.Count = 0}
        End Try
    End Function

    Private Class FreeGamesSettings
        Public Property Count As Integer
    End Class

End Module
