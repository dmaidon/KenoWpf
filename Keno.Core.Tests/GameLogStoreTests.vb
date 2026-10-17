Imports System.IO
Imports Xunit

Public Class GameLogStoreTests

    <Fact>
    Public Sub ExportCsv_WritesParsedRows()
        ClearLog()
        AppendGame("Regular", 2D, 3, 50D, 2, False, 0D)
        AppendGame("Bullseye", 1D, 0, 0D, 1, True, 5D)
        Dim csvFile = Path.Combine(Path.GetTempPath(), $"keno-test-{Guid.NewGuid():N}.csv")
        Try
            Assert.Equal(2, ExportCsv(csvFile))
            Dim lines = File.ReadAllLines(csvFile)
            Assert.Equal("Timestamp,Mode,Bet,Match,Multiplier,Payout,FirstLastBonus,FreeGame", lines(0))
            Assert.Contains(",""Regular"",2.00,3,2,50.00,,N", lines(1))
            Assert.EndsWith(",Y", lines(2))
        Finally
            File.Delete(csvFile)
            ClearLog()
        End Try
    End Sub

End Class
