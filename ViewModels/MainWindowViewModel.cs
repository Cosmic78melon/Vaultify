using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Password_Manager.Factory;
using System.Diagnostics;
using System;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Password_Manager.Models;
using Password_Manager.Service;
using ZstdSharp.Unsafe;

namespace Password_Manager.ViewModels
{
    enum IconSizeList
    {
        Small = 10, Large = 22
    }
    public partial class MainWindowViewModel : ViewModelBase
    {
        private string? _masterPassword = string.Empty;
        [ObservableProperty] private string _userName;
        [ObservableProperty] private string _email;
        [ObservableProperty] private string _password;
        [ObservableProperty] private string _confirmationPassword;
        [ObservableProperty] private string _statusMessage;
        [ObservableProperty] private string _colorsC;            
        [ObservableProperty] private bool _bgOp = true;
        
        [ObservableProperty] private bool _bgPop = true;
        
        [ObservableProperty] private bool _bgOpSignUp = false;
        
        [ObservableProperty] private bool _bgPopSignUp = false;

        public required PageFactory _pageFactory;
        public required IPythonAPI _pythonAPI;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IconSize))]
        [NotifyPropertyChangedFor(nameof(SeacrhbarSize))]
        public bool _isExpanded = true;

        public int IconSize => (IsExpanded) ? (int)IconSizeList.Large : (int)IconSizeList.Small;

        public int SeacrhbarSize => (IsExpanded) ? 220 : 5;

        [ObservableProperty]
        public PageViewModel _currentPage;

        [ObservableProperty] public bool _homePageActive;
        [ObservableProperty] public bool _all_EntriesActive;
        [ObservableProperty] public bool _securityNotesActive;
        [ObservableProperty] public bool _settingsPageActive;
        [ObservableProperty] public bool _accountPageActive;
        

        public MainWindowViewModel(PageFactory pageFactory, IPythonAPI pythonAPI)
        {
            _pageFactory = pageFactory;
            _pythonAPI = pythonAPI;
            CheckUser();
            GoToHome();
        }
        public MainWindowViewModel()
        {
            if (_pythonAPI != null) CurrentPage = new HomePageViewModel(_pythonAPI);
        }

        public void CheckUser()
        {
            bool isNewUser = _pythonAPI.isNewUser();
            if (isNewUser)
            {
                // Show SIGNUP
                BgOp = false;
                BgPop = false;

                BgOpSignUp = true;
                BgPopSignUp = true;
            }
            else
            {
                // Show LOGIN
                BgOp = true;
                BgPop = true;

                BgOpSignUp = false;
                BgPopSignUp = false;
            }
        }
        
        [RelayCommand]
        public void GotRegister()
        {
            BgOp = BgPop = false;
            BgPopSignUp = BgOpSignUp = true;
        }
        [RelayCommand]
        public void GotLogin()
        {
            BgOp = BgPop = true;
            BgPopSignUp = BgOpSignUp = false;
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

            if (string.Equals(Password, ConfirmationPassword, StringComparison.OrdinalIgnoreCase))
            {
                bool isNewUser = _pythonAPI.isNewUser();
                bool isAuthenticated = _pythonAPI.isAuthenticated(Password);
                if (isNewUser != true && isAuthenticated)
                {
                    StatusMessage = "Welcome back! You are already registered with us.";
                    ColorsC = "yellow";
                    return;
                }

                bool register = _pythonAPI.register(UserName, Password);
                if (register == false)
                {
                    StatusMessage = "Your Password is Weak. please enter strong password";
                    ColorsC = "red";
                    return;
                }
                StatusMessage = "Account created successfully! Welcome to our platform.";
                ColorsC = "green";
            }
        }

        [RelayCommand]
        public void LoginCommand()
        {
            bool passwordStatus = _pythonAPI.isAuthenticated(Password);
            if (string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Please enter your password.";
                ColorsC = "red";
                return;
            }

            if (passwordStatus)
            {
                _masterPassword = Password;
                BgOp = false;
                BgPop = false;
                Password = string.Empty;
            }
            StatusMessage = "Invalid Password.";
            ColorsC = "red";
        }


        [RelayCommand]
        public void GoToHome()
        {
            CurrentPage = _pageFactory.GetPageViewModel<HomePageViewModel>(item =>
            {
                if (_masterPassword != null) item.load_data_recent(_masterPassword);
            });
            UpdateActiveState(PageViewData.Home);
        }  

        [RelayCommand]
        public void GoToAll_Entries()
        {
            CurrentPage = _pageFactory.GetPageViewModel<All_EntriesPageViewModel>((item) =>
            {
                if (_masterPassword != null)
                {
                    item.HomepagePassword = _masterPassword;
                    _ = item.LoadData(_masterPassword);
                }
            });
            UpdateActiveState(PageViewData.All_Entries);
        } 
        [RelayCommand]
        public void GoToSecurity()
        {
            CurrentPage = _pageFactory.GetPageViewModel<SecurityPageViewModel>();
            UpdateActiveState(PageViewData.Security);
        } 
        [RelayCommand]
        public void GoToSettings()
        {
            CurrentPage = _pageFactory.GetPageViewModel<SettingsPageViewModel>();
            UpdateActiveState(PageViewData.Settings);
        } 
        [RelayCommand]
        public void GoToAccounts()
        {
            CurrentPage = _pageFactory.GetPageViewModel<AccountPageViewModel>();
            UpdateActiveState(PageViewData.Accounts);
        }        

        public void UpdateActiveState(PageViewData activepage)
        {
            HomePageActive = activepage == PageViewData.Home;
            All_EntriesActive = activepage == PageViewData.All_Entries;
            SecurityNotesActive = activepage == PageViewData.Security;
            AccountPageActive = activepage == PageViewData.Accounts;
            SettingsPageActive = activepage == PageViewData.Settings;
        }
        [RelayCommand]
        public void SideMenuResize()
        {
            IsExpanded = !IsExpanded;
        }
    }
}
