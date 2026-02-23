using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Password_Manager.Factory;
using Password_Manager.Models;
using Password_Manager.ViewModels;
using Password_Manager.Views;
using Python.Runtime;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

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
            var collections = new ServiceCollection();
            collections.AddSingleton<MainWindowViewModel>();
            collections.AddTransient<HomePageViewModel>();
            collections.AddTransient<All_EntriesPageViewModel>();
            collections.AddTransient<SecurityPageViewModel>();
            collections.AddTransient<SettingsPageViewModel>();
            collections.AddTransient<AccountPageViewModel>();

            //Initialize Python
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();

            // Register auth service
            collections.AddSingleton<Services.IAuthServices, Services.AuthServices>();

            collections.AddSingleton<Func<PageViewData, PageViewModel>>(x => Name => Name switch
            {
                PageViewData.Home => x.GetRequiredService<HomePageViewModel>(),
                PageViewData.All_Entries => x.GetRequiredService<All_EntriesPageViewModel>(),
                PageViewData.Security => x.GetRequiredService<SecurityPageViewModel>(),
                PageViewData.Settings => x.GetRequiredService<SettingsPageViewModel>(),
                PageViewData.Accounts => x.GetRequiredService<AccountPageViewModel>()
            });

            collections.AddSingleton<PageFactory>();
            var service = collections.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = service.GetRequiredService<MainWindowViewModel>()
                };

            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
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