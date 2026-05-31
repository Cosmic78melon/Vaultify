using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Input.Platform;
using Avalonia.Controls.ApplicationLifetimes;

namespace Password_Manager.Service;

public class CopyTextsServices(Func<TopLevel?> toplevel)
{
    public static IClipboard Get()
    {
        //Desktop
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
            {
                MainWindow: { } window
            })
        {
            return window.Clipboard!;

        }
        //Android (and iOS?)
        else if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime { MainView: { } mainView })
        {
            var visualRoot = mainView.GetVisualRoot();
            if (visualRoot is TopLevel topLevel)
            {
                return topLevel.Clipboard!;
            }
        }

        return null!;
    }
}