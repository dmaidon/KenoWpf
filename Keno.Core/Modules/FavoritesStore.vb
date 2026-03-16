' Last Edit: 2026-03-23 - Three named favorites slots with backward-compat migration from single-slot format. Moved to Keno.Core.
Imports System.IO
Imports System.Text.Json

Public Module FavoritesStore

    Public Const SlotCount As Integer = 3

    Private ReadOnly FavoritesFilePath As String = Path.Combine(DatDir, "favorites.json")

    Private ReadOnly JsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True
    }

    Public Sub SaveFavorites(slotIndex As Integer, numbers As IEnumerable(Of Integer))
        Try
            Directory.CreateDirectory(DatDir)
            Dim data = LoadData()
            data.Slots(slotIndex).Numbers = numbers.OrderBy(Function(n) n).ToArray()
            File.WriteAllText(FavoritesFilePath, JsonSerializer.Serialize(data, JsonOptions))
        Catch ex As Exception
            LogError(ex, NameOf(SaveFavorites))
        End Try
    End Sub

    Public Function LoadFavorites(slotIndex As Integer) As Integer()
        Try
            Dim nums = LoadData().Slots(slotIndex).Numbers
            Return If(nums, Array.Empty(Of Integer)())
        Catch ex As Exception
            LogError(ex, NameOf(LoadFavorites))
            Return Array.Empty(Of Integer)()
        End Try
    End Function

    Public Function GetSlotCount(slotIndex As Integer) As Integer
        Try
            Dim nums = LoadData().Slots(slotIndex).Numbers
            Return If(nums IsNot Nothing, nums.Length, 0)
        Catch
            Return 0
        End Try
    End Function

    Public Function HasFavorites() As Boolean
        Try
            Return LoadData().Slots.Any(Function(s) s.Numbers IsNot Nothing AndAlso s.Numbers.Length > 0)
        Catch
            Return False
        End Try
    End Function

    Public Function GetAllSlots() As FavoritesSlot()
        Try
            Return LoadData().Slots
        Catch ex As Exception
            LogError(ex, NameOf(GetAllSlots))
            Return MakeEmptySlots()
        End Try
    End Function

    Private Function LoadData() As FavoritesData
        Try
            Directory.CreateDirectory(DatDir)

            If Not File.Exists(FavoritesFilePath) Then
                Return New FavoritesData With {.Slots = MakeEmptySlots()}
            End If

            Dim json = File.ReadAllText(FavoritesFilePath)

            ' Try new multi-slot format
            Dim data = JsonSerializer.Deserialize(Of FavoritesData)(json)
            If data IsNot Nothing AndAlso data.Slots IsNot Nothing AndAlso data.Slots.Length = SlotCount Then
                Return data
            End If

            ' Backward compat: migrate from old single-slot format
            Dim legacy = JsonSerializer.Deserialize(Of LegacyFavoritesData)(json)
            Dim migrated = New FavoritesData With {.Slots = MakeEmptySlots()}
            If legacy IsNot Nothing AndAlso legacy.Numbers IsNot Nothing AndAlso legacy.Numbers.Length > 0 Then
                migrated.Slots(0).Numbers = legacy.Numbers
            End If
            File.WriteAllText(FavoritesFilePath, JsonSerializer.Serialize(migrated, JsonOptions))
            Return migrated
        Catch ex As Exception
            LogError(ex, NameOf(LoadData))
            Return New FavoritesData With {.Slots = MakeEmptySlots()}
        End Try
    End Function

    Private Function MakeEmptySlots() As FavoritesSlot()
        Dim slots(SlotCount - 1) As FavoritesSlot
        For i = 0 To SlotCount - 1
            slots(i) = New FavoritesSlot()
        Next
        Return slots
    End Function

    Public Class FavoritesSlot
        Public Property Name As String
        Public Property Numbers As Integer()
    End Class

    Private Class FavoritesData
        Public Property Slots As FavoritesSlot()
    End Class

    Private Class LegacyFavoritesData
        Public Property Numbers As Integer()
    End Class

End Module
