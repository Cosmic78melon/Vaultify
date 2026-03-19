using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Password_Manager.Factory;
using System.Diagnostics;
using System;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Password_Manager.Service;

namespace Password_Manager.ViewModels
{
    enum IconSizeList
    {
        Small = 10, Large = 22
    }
    public partial class MainWindowViewModel : ViewModelBase
    {
        private string? MasterPassword = string.Empty;
        [ObservableProperty] private string _userName;
        [ObservableProperty] private string _email;
        [ObservableProperty] private string _password;
        [ObservableProperty] private string _confirmationPassword;
        [ObservableProperty] private string _statusMessage;
        [ObservableProperty] private string _colorsC;
        [ObservableProperty] private bool _bg_OP = true;
        [ObservableProperty] private bool _bg_POP = true;

        [ObservableProperty] private bool _bg_OPSignUp = false;
        [ObservableProperty] private bool _bg_POPSignUp = false;


        [RelayCommand]
        public void GotRegister()
        {
            Bg_OP = Bg_POP = false;
            Bg_POPSignUp = Bg_OPSignUp = true;
        }
        [RelayCommand]
        public void GotLogin()
        {
            Bg_OP = Bg_POP = true;
            Bg_POPSignUp = Bg_OPSignUp = false;
        }

        [RelayCommand]
        public void Register()
        {
            StatusMessage = string.Empty;
            
            if (string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password) && string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(ConfirmationPassword))
            {
                StatusMessage = "Please Enter your Credentials";
                ColorsC = "red";
                return;
            }
        }

        [RelayCommand]
        public async void LoginCommand()
        {
            PythonAPI pythonhelper = new PythonAPI();
            bool password_status = await Task.Run(() => pythonhelper.check_Password(Password));
            if (string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Please enter your password.";
                ColorsC = "red";
                return;
            }

            // Dummy login logic
            if (password_status)
            {
                MasterPassword = Password;
                Bg_OP = false;
                Bg_POP = false;
                Password = string.Empty;
                // You can add navigation or other logic here upon successful login
            }
            StatusMessage = "Invalid Password.";
            ColorsC = "red";
        }

        public PageFactory _pageFactory;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IconSize))]
        [NotifyPropertyChangedFor(nameof(SeacrhbarSize))]
        public bool _isExpanded = true;

        public int IconSize => (IsExpanded) ? (int)IconSizeList.Large : (int)IconSizeList.Small;

        public int SeacrhbarSize => (IsExpanded) ? 220 : 70;

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
            CurrentPage = new HomePageViewModel();
        }
        public MainWindowViewModel(PageFactory pageFactory)
        {
            _pageFactory = pageFactory;
            
            GoToHome();
        }

        [RelayCommand]
        public void GoToHome()
        {
            CurrentPage = _pageFactory.GetPageViewModel<HomePageViewModel>();
        }  

        [RelayCommand]
        public void GoToAll_Entries()
        {
            CurrentPage = _pageFactory.GetPageViewModel<All_EntriesPageViewModel>(item => item.LoadData(MasterPassword));
        } 
        [RelayCommand]
        public void GoToSecurity()
        {
            CurrentPage = _pageFactory.GetPageViewModel<SecurityPageViewModel>();
        } 
        [RelayCommand]
        public void GoToSettings()
        {
            CurrentPage = _pageFactory.GetPageViewModel<SettingsPageViewModel>();
        } 
        [RelayCommand]
        public void GoToAccounts()
        {
            CurrentPage = _pageFactory.GetPageViewModel<AccountPageViewModel>();
        }        
        [RelayCommand]
        public void SideMenuResize()
        {
            IsExpanded = !IsExpanded;
        }
    }
}
