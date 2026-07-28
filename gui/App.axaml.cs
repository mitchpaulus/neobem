using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using gui.ViewModels;
using gui.Views;

namespace gui;

public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewModel = new MainWindowViewModel();
            desktop.MainWindow = new MainWindow { DataContext = viewModel };

            string? initialFile = desktop.Args?.FirstOrDefault(File.Exists);
            if (initialFile is not null) viewModel.LoadFile(Path.GetFullPath(initialFile));
        }

        base.OnFrameworkInitializationCompleted();
    }
}
