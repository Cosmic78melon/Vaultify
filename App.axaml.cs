using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Password_Manager.Factory;
using Password_Manager.Models;
using Password_Manager.ViewModels;
using Password_Manager.Views;
using Python.Runtime;
using System;
using System.CodeDom;
using System.Diagnostics;
using System.IO;
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


            // Initialize Python
            string baseDir = AppContext.BaseDirectory;
            string pythonHome = Path.Combine(baseDir, "python-3.13.12-embed-amd64");
            string pythonDll = Path.Combine(pythonHome, "python313.dll");

            Runtime.PythonDLL = pythonDll;
            PythonEngine.PythonHome = pythonHome;

            // Append directory, not DLL
            string currentPath = Environment.GetEnvironmentVariable("PATH");
            Environment.SetEnvironmentVariable("PATH", currentPath + ";" + pythonHome);

            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();

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