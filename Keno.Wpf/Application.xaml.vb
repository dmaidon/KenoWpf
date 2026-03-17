' Last Edit: 2026-03-17 - MahApps Dark.Steel theme applied in code; eliminates XDG0010 XAML designer errors on Application.xaml.
Class Application

    ' MahApps.Metro theme is loaded here at startup (not in Application.xaml) so the XAML designer
    ' never needs to resolve the MahApps.Metro DLL from pack:// URIs, eliminating XDG0010 errors.
    Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
        ControlzEx.Theming.ThemeManager.Current.ChangeTheme(Current, "Dark.Steel")
    End Sub

End Class
