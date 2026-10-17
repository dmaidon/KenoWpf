namespace Keno.Android;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();

        // Build number comes from the assembly version, which the build bumps on every build.
        var version = typeof(AboutPage).Assembly.GetName().Version?.ToString() ?? "?";
        LblVersion.Text = $"Version {version}";
        LblCopyright.Text = "© 2026 PAROLE Software";
    }

    private async void BtnClose_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
