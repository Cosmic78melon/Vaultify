using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Password_Manager.Factory;
using System;
using System.Threading.Tasks;
using Password_Manager.Models;
using Password_Manager.Service;

namespace Password_Manager.ViewModels
{
    enum IconSizeList
    {
        Small = 40, Large = 100 
    }
    public partial class MainWindowViewModel : ViewModelBase
    {
        private string? _masterPassword = string.Empty;
        [ObservableProperty] private string _userName = null!;
        [ObservableProperty] private string _email = null!;
        [ObservableProperty] private string _password = null!;
        [ObservableProperty] private string _confirmationPassword = null!;
        [ObservableProperty] private string _statusMessage = null!;
        [ObservableProperty] private string _colorsC = null!;            
        [ObservableProperty] private bool _bgOp = true;
        
        [ObservableProperty] private bool _bgPop = true;
        
        [ObservableProperty] private bool _bgOpSignUp = false;
        
        [ObservableProperty] private bool _bgPopSignUp = false;
        [ObservableProperty] private bool _revealPass = false;
        [ObservableProperty] private string _eyeIcon = "EyeOff";
        public required PageFactory _pageFactory;
        public required IAppServices _appServices;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IconSize))]
        [NotifyPropertyChangedFor(nameof(SeacrhbarVisibility))]
        public bool _isExpanded = true;

        public int IconSize => (IsExpanded) ? (int)IconSizeList.Large : (int)IconSizeList.Small;

        public bool SeacrhbarVisibility => (IsExpanded) ? true : false;

        [ObservableProperty]
        public required PageViewModel _currentPage;

        [ObservableProperty] public bool _homePageActive;
        [ObservableProperty] public bool _all_EntriesActive;
        [ObservableProperty] public bool _securityNotesActive;
        [ObservableProperty] public bool _settingsPageActive;
        [ObservableProperty] public bool _accountPageActive;
        

        public MainWindowViewModel(PageFactory pageFactory, IAppServices appServices)
        {
            _pageFactory = pageFactory;
            _appServices = appServices;
            CheckUser();
            GoToHome();
        }
        public MainWindowViewModel()
        {
            if (_appServices != null) CurrentPage = new HomePageViewModel(_appServices);
        }

        public void CheckUser()
        {
            bool isNewUser = _appServices.isNewUser();
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
        public async Task Register()
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
                bool isNewUser = _appServices.isNewUser();
                bool isAuthenticated = _appServices.isAuthenticated(Password);
                if (isNewUser != true && isAuthenticated)
                {
                    StatusMessage = "Welcome back! You are already registered with us.";
                    ColorsC = "yellow";
                    return;
                }

                bool register = await _appServices.register(UserName, Password);
                if (!register)
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
        public void GoToHome()
        {
            CurrentPage = _pageFactory.GetPageViewModel<HomePageViewModel>(item =>
            {
                if (_masterPassword != null)
                {
                    if (string.IsNullOrEmpty(_masterPassword)) return;
                    item.load_data_recent();
                    item.FavouriteData();
                    item.StatusDataLoad();
                }
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
            CurrentPage = _pageFactory.GetPageViewModel<SecurityPageViewModel>((item =>
            {
               if (_masterPassword != null)
               {
                   item.PasswordSecureity = _masterPassword;
                   _ = item.ItemLoadNotes(_masterPassword);
               }
            }));
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
        public void SideMenuResize() => IsExpanded = !IsExpanded;

        [RelayCommand]
        public void RevealPassword()
        {
            RevealPass = !RevealPass;
            EyeIcon = RevealPass ? "Eye" : "EyeOff";
        }

        [RelayCommand]
        public void LoginCommand()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Please enter your password.";
                ColorsC = "red";
                return;
            }
            bool passwordStatus = _appServices.loginAuth(Password);

            if (passwordStatus != true)
            {
                StatusMessage = "Invalid Password.";
                ColorsC = "red";
                return;
            }
            
            _masterPassword = Password;
            BgOp = false;
            BgPop = false;
            Password = string.Empty;
            GoToHome();
        }

    }
}
