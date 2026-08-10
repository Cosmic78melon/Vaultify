using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vaultify.ViewModels;

public partial class ToastNotificationViewModel: PageViewModel
{
    [ObservableProperty] public string _colorBG = "red";
    [ObservableProperty] public bool  _isVisible = false;
    [ObservableProperty] public string _colorSFG = "red";
    [ObservableProperty] public string _title = "red";
    [ObservableProperty] public string _message = "red";
    [ObservableProperty] public string _iconName = string.Empty;

    [RelayCommand]
    public void isVisible()
    {
        IsVisible = false;
    }
}