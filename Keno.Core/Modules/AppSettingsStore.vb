' Last Edit: 2026-03-23 - Added DrawSpeedIndex persistence. Moved to Keno.Core.
Imports System.IO
Imports System.Text.Json

Public Module AppSettingsStore
    Private Const SettingsFileName As String = "app-settings.json"
    Private ReadOnly SettingsFilePath As String = Path.Combine(SetDir, SettingsFileName)
    Private ReadOnly JsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True
    }

    Public Function LoadAppSettings() As AppSettings
        Try
            Directory.CreateDirectory(SetDir)

            If Not File.Exists(SettingsFilePath) Then
                Dim defaults = New AppSettings()
                SaveAppSettings(defaults)
                Return defaults
            End If

            Dim json = File.ReadAllText(SettingsFilePath)
            Dim settings = JsonSerializer.Deserialize(Of AppSettings)(json)

            If settings Is Nothing Then
                settings = New AppSettings()
                SaveAppSettings(settings)
            End If

            Return settings
        Catch ex As Exception
            LogError(ex, NameOf(LoadAppSettings))
            Return New AppSettings()
        End Try
    End Function

    Public Sub SaveAppSettings(settings As AppSettings)
        Try
            Directory.CreateDirectory(SetDir)

            Dim json = JsonSerializer.Serialize(settings, JsonOptions)
            File.WriteAllText(SettingsFilePath, json)
        Catch ex As Exception
            LogError(ex, NameOf(SaveAppSettings))
        End Try
    End Sub

    Public Class AppSettings
        Public Property HasLocation As Boolean
        Public Property LocationX As Integer
        Public Property LocationY As Integer
        Public Property WindowState As String
        ''' <summary>0 = Slow, 1 = Medium, 2 = Fast</summary>
        Public Property DrawSpeedIndex As Integer
    End Class
End Module
