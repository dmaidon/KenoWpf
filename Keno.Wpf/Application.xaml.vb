' Last Edit: 2026-03-17 - MahApps resources added in code; eliminates XDG0010 XAML designer errors on Application.xaml.
Class Application

    ' MahApps.Metro resources are loaded here at startup (not in Application.xaml) so the XAML designer
    ' never needs to resolve the MahApps.Metro DLL from pack:// URIs, eliminating XDG0010 errors.
    ' NOTE: ThemeManager.ChangeTheme cannot be used here — MahApps' LibraryThemeProvider is not
    ' registered until the first MetroWindow is constructed (after Startup fires), so GetTheme returns
    ' null and ChangeTheme throws ArgumentNullException. Adding the ResourceDictionaries directly is
    ' the correct code-first equivalent of the XAML MergedDictionaries approach.
    Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
        Resources.MergedDictionaries.Add(New ResourceDictionary() With {.Source = New Uri("pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml")})
        Resources.MergedDictionaries.Add(New ResourceDictionary() With {.Source = New Uri("pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml")})
        Resources.MergedDictionaries.Add(New ResourceDictionary() With {.Source = New Uri("pack://application:,,,/MahApps.Metro;component/Styles/Themes/Dark.Steel.xaml")})
    End Sub

End Class
