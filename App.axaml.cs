using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Password_Manager.Factory;
using Password_Manager.Service;
using Password_Manager.ViewModels;
using Password_Manager.Views;
using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Dialogs;

namespace Password_Manager
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public override void OnFrameworkInitializationCompleted()
        {
            if (Design.IsDesignMode) return;
            
            var collections = new ServiceCollection();
            collections.AddSingleton<MainWindowViewModel>();
            collections.AddSingleton<HomePageViewModel>();
            collections.AddTransient<All_EntriesPageViewModel>();
            collections.AddTransient<SecurityPageViewModel>();
            collections.AddTransient<SettingsPageViewModel>();
            collections.AddTransient<AccountPageViewModel>();

            // service initialization
            collections.AddSingleton<IAppServices, AppServices>();
            
            // Page View Models and Page View
            collections.AddSingleton<Func<Type, PageViewModel>>(x => type => type switch
            {
                _ when type == typeof(HomePageViewModel) => x.GetRequiredService<HomePageViewModel>(),
                _ when type == typeof(All_EntriesPageViewModel) => x.GetRequiredService<All_EntriesPageViewModel>(),
                _ when type == typeof(SecurityPageViewModel) => x.GetRequiredService<SecurityPageViewModel>(),
                _ when type == typeof(SettingsPageViewModel) => x.GetRequiredService<SettingsPageViewModel>(),
                _ when type == typeof(AccountPageViewModel) => x.GetRequiredService<AccountPageViewModel>(),
                _ => throw new NotImplementedException()
            });

            collections.AddSingleton<PageFactory>();
            //File Picker Service
            collections.AddSingleton<FilePickerService>();
            collections.AddSingleton<Func<TopLevel?>>(x => () =>
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime window)
                {
                    return TopLevel.GetTopLevel(window.MainWindow);
                }

                return null;
            });

            collections.AddSingleton<CopyTextsServices>();
            collections.AddSingleton<Func<TopLevel?>>(x => () =>
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime clipboards)
                {
                    return TopLevel.GetTopLevel(clipboards.MainWindow);
                }

                return null;
            });
            
            var service = collections.BuildServiceProvider();
           
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = service.GetRequiredService<MainWindowViewModel>()
                };

            }

            base.OnFrameworkInitializationCompleted();
        }

        private static void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}