' Last Edit: 2026-03-15 - WinBonusGame code-behind: auto-close timer, pick info.
Class WinBonusGame

    Private _secondsLeft As Integer = 7
    Private ReadOnly _timer As New System.Windows.Threading.DispatcherTimer()

    Friend Shared Sub ShowBonus(owner As Window, picks As Integer)
        Dim win As New WinBonusGame()
        win.TbkInfo.Text = $"Pick {picks} with no matches earns{Environment.NewLine}1 free $2 game of any kind."
        win.Owner = owner
        win.ShowDialog()
    End Sub

    Public Sub New()
        InitializeComponent()
        _timer.Interval = TimeSpan.FromSeconds(1)
        AddHandler _timer.Tick, AddressOf Timer_Tick
    End Sub

    Private Sub WinBonusGame_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        UpdateCountdown()
        _timer.Start()
    End Sub

    Private Sub Timer_Tick(sender As Object, e As EventArgs)
        _secondsLeft -= 1

        If _secondsLeft <= 0 Then
            _timer.Stop()
            Close()
            Return
        End If

        UpdateCountdown()
    End Sub

    Private Sub UpdateCountdown()
        TbkCountdown.Text = $"Closing in {_secondsLeft}s…"
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
        _timer.Stop()
        Close()
    End Sub

    Private Sub WinBonusGame_MouseEnter(sender As Object, e As MouseEventArgs) Handles Me.MouseEnter
        _timer.Stop()
        TbkCountdown.Text = String.Empty
    End Sub

    Private Sub WinBonusGame_MouseLeave(sender As Object, e As MouseEventArgs) Handles Me.MouseLeave
        _secondsLeft = 7
        UpdateCountdown()
        _timer.Start()
    End Sub

End Class
