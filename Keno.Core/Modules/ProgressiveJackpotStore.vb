' Last Edit: 2026-03-16 - Jackpot seed lowered to $1,000 per spec (initialises at $1,000, resets to $1,000 on win).
Imports System.IO
Imports System.Text.Json

Public Module ProgressiveJackpotStore

    Private Const JackpotSeed As Decimal = 25000D

    Private ReadOnly JackpotFilePath As String = Path.Combine(DatDir, "jackpot.json")
    Private ReadOnly JsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True
    }

    Public Function GetJackpotBalance() As Decimal
        Try
            If Not File.Exists(JackpotFilePath) Then
                Return JackpotSeed
            End If

            Dim json = File.ReadAllText(JackpotFilePath)
            Dim data = JsonSerializer.Deserialize(Of JackpotData)(json)

            Return If(data IsNot Nothing, Math.Max(data.Balance, JackpotSeed), JackpotSeed)
        Catch ex As Exception
            LogError(ex, NameOf(GetJackpotBalance))
            Return JackpotSeed
        End Try
    End Function

    Public Sub EnsureJackpot()
        Try
            If Not File.Exists(JackpotFilePath) Then
                ResetJackpot()
            End If
        Catch ex As Exception
            LogError(ex, NameOf(EnsureJackpot))
        End Try
    End Sub

    Public Sub AddToJackpot(amount As Decimal)
        Try
            Dim balance = GetJackpotBalance() + amount
            SaveJackpotBalance(balance)
        Catch ex As Exception
            LogError(ex, NameOf(AddToJackpot))
        End Try
    End Sub

    Public Sub ResetJackpot()
        Try
            SaveJackpotBalance(JackpotSeed)
        Catch ex As Exception
            LogError(ex, NameOf(ResetJackpot))
        End Try
    End Sub

    Private Sub SaveJackpotBalance(balance As Decimal)
        Try
            Directory.CreateDirectory(DatDir)
            Dim data = New JackpotData With {.Balance = balance}
            Dim json = JsonSerializer.Serialize(data, JsonOptions)
            File.WriteAllText(JackpotFilePath, json)
        Catch ex As Exception
            LogError(ex, NameOf(SaveJackpotBalance))
        End Try
    End Sub

    Private Class JackpotData
        Public Property Balance As Decimal
    End Class

End Module
