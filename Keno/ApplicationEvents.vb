' Last Edit: 2026-03-10 - Removed SystemColorMode.System; app uses hardcoded colors not yet DarkMode-aware.
Imports System.IO

Imports Microsoft.VisualBasic.ApplicationServices

Namespace My

    Partial Friend Class MyApplication

        ''' <summary>Sets app-wide defaults before the startup form is created.</summary>
        Private Sub MyApplication_ApplyApplicationDefaults(sender As Object, e As ApplyApplicationDefaultsEventArgs) Handles Me.ApplyApplicationDefaults
            e.HighDpiMode = HighDpiMode.SystemAware
        End Sub

        ''' <summary>Ensures data directories exist and seeds the bank on first run.</summary>
        Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
            Directory.CreateDirectory(DatDir)
            Directory.CreateDirectory(LogDir)
            Directory.CreateDirectory(SetDir)
            EnsureBankSettings()
        End Sub

        ''' <summary>Last-resort handler: logs any unhandled UI-thread exception and keeps the app alive.</summary>
        Private Sub MyApplication_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs) Handles Me.UnhandledException
            LogError(e.Exception, "UnhandledException")
            e.ExitApplication = False
        End Sub

    End Class

End Namespace