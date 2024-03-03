using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.OpenGL;

namespace SurfaceVisualizer;

internal class Program : Application
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder
            .Configure<Program>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions
            {
                RenderingMode = [ Win32RenderingMode.Wgl ],
                WglProfiles = [ new GlVersion(GlProfileType.OpenGL, 4, 5) ],
            })
            .With(new X11PlatformOptions
            {
                GlProfiles = [ new GlVersion(GlProfileType.OpenGL, 4, 5) ],
            });
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
