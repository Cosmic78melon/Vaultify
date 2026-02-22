using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Password_Manager.Factory;
using Password_Manager.Services;
using System.Xml.Serialization;

namespace Password_Manager.ViewModels
{
    enum iconSize
    {
        Small = 50, Large = 100
    }
    public partial class MainWindowViewModel : ViewModelBase
    {
        public PageFactory _pageFactory;
        private AuthResult _authResult;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IconSize))]
        public bool _isExpanded = true;

        public int IconSize => (IsExpanded == true) ? (int)iconSize.Large : (int)(iconSize.Small);


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HomePageActive))]
        [NotifyPropertyChangedFor(nameof(All_Entries))]
        [NotifyPropertyChangedFor(nameof(SecurityNotes))]
        [NotifyPropertyChangedFor(nameof(SettingsPageActive))]
        [NotifyPropertyChangedFor(nameof(AccountPageActive))]
        public PageViewModel _currentPage;

        public bool HomePageActive => (CurrentPage.PageNames == Models.PageViewData.Home);
        public bool All_Entries => (CurrentPage.PageNames == Models.PageViewData.All_Entries);
        public bool SecurityNotes => (CurrentPage.PageNames == Models.PageViewData.Security);
        public bool SettingsPageActive => (CurrentPage.PageNames == Models.PageViewData.Settings);
        public bool AccountPageActive => (CurrentPage.PageNames == Models.PageViewData.Accounts);


        public MainWindowViewModel()
        {
            _currentPage = new HomePageViewModel();
        }
        public MainWindowViewModel(PageFactory pageFactory)
        {
            _pageFactory = pageFactory;
            
            GoToHome();
        }

        [RelayCommand]
        public void GoToHome()
        {
            CurrentPage = _pageFactory.GetPageViewModel(Models.PageViewData.Home);
        }  

        [RelayCommand]
        public void GoToAll_Entries()
        {
            CurrentPage = _pageFactory.GetPageViewModel(Models.PageViewData.All_Entries);
        } 
        [RelayCommand]
        public void GoToSecurity()
        {
            CurrentPage = _pageFactory.GetPageViewModel(Models.PageViewData.Security);
        } 
        [RelayCommand]
        public void GoToSettings()
        {
            CurrentPage = _pageFactory.GetPageViewModel(Models.PageViewData.Settings);
        } 
        [RelayCommand]
        public void GoToAccounts()
        {
            CurrentPage = _pageFactory.GetPageViewModel(Models.PageViewData.Accounts);
        }        
        [RelayCommand]
        public void SideMenuResize()
        {
            IsExpanded = !IsExpanded;
        }
    }
}
