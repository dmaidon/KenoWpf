' Last Edit: 2026-03-17 05:04 PM - Removed automatic $0 reset; repopulation is now manual via WPF Settings flyout.
Imports System.IO
Imports System.Text.Json

Public Module BankSettingsStore
    Private Const DefaultBalance As Decimal = 10000D
    Private ReadOnly SettingsFilePath As String = Path.Combine(DatDir, "bank-settings.json")
    Private ReadOnly JsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True
    }

    Public Sub EnsureBankSettings()
        Try
            Directory.CreateDirectory(DatDir)

            If File.Exists(SettingsFilePath) Then
                Return
            End If

            SaveBankBalance(DefaultBalance)
        Catch ex As Exception
            LogError(ex, NameOf(EnsureBankSettings))
        End Try
    End Sub

    Public Function GetBankBalance() As Decimal
        Try
            Dim settings = LoadSettings()
            Return settings.Balance
        Catch ex As Exception
            LogError(ex, NameOf(GetBankBalance))
            Return DefaultBalance
        End Try
    End Function

    Public Sub SaveBankBalance(balance As Decimal)
        Try
            Dim settings = New BankSettings With {
                .Balance = balance
            }

            SaveSettings(settings)
        Catch ex As Exception
            LogError(ex, NameOf(SaveBankBalance))
        End Try
    End Sub

    Private Function LoadSettings() As BankSettings
        Try
            Directory.CreateDirectory(DatDir)

            If Not File.Exists(SettingsFilePath) Then
                Dim defaults = New BankSettings With {
                    .Balance = DefaultBalance
                }

                SaveSettings(defaults)
                Return defaults
            End If

            Dim json = File.ReadAllText(SettingsFilePath)
            Dim settings = JsonSerializer.Deserialize(Of BankSettings)(json)

            If settings Is Nothing Then
                settings = New BankSettings With {
                    .Balance = DefaultBalance
                }

                SaveSettings(settings)
            End If

            Return settings
        Catch ex As Exception
            LogError(ex, NameOf(LoadSettings))
            Return New BankSettings With {
                .Balance = DefaultBalance
            }
        End Try
    End Function

    Private Sub SaveSettings(settings As BankSettings)
        Try
            Directory.CreateDirectory(DatDir)

            Dim json = JsonSerializer.Serialize(settings, JsonOptions)

            File.WriteAllText(SettingsFilePath, json)
        Catch ex As Exception
            LogError(ex, NameOf(SaveSettings))
        End Try
    End Sub

    Private Class BankSettings
        Public Property Balance As Decimal
    End Class
End Module
