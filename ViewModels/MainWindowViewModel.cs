using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vaultify.Factory;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;
using System.Reflection;
using Vaultify.Models;
using Vaultify.Service;

namespace Vaultify.ViewModels
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
        [ObservableProperty] private bool _progressBarVisible;
        [ObservableProperty] private string _version;
        public required PageFactory _pageFactory;
        public required IAppServices _appServices;
        public required IUpdateService _updateServices;
        public required IToastService _toastService;
        

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
        public ToastNotificationViewModel Toast =>
                _toastService.Notification;

        public MainWindowViewModel(PageFactory pageFactory, IAppServices appServices, IUpdateService updateServices, IToastService toastService)
        {
            _pageFactory = pageFactory;
            _appServices = appServices;
            _updateServices = updateServices;
            _toastService = toastService;
            
            CheckUser();
            GoToHome();
            CheckVersion();
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

        public async Task CheckVersion()
        {
            var updateInfo = await _updateServices.CheckUpdateInfoAsync();
            string? rawNewVersion = updateInfo?.TagName;
            string? newVersionText = rawNewVersion?.TrimStart('v', 'V');
            
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            
            if (currentVersion != null && System.Version.TryParse(newVersionText, out var newVersion))
            {
                Version = currentVersion.ToString();

                if (newVersion > currentVersion)
                {
                    await _toastService.ShowMessageAsync("New Version is Available",
                        $"New {newVersion} is Available Download Link: ", true, "Info", "#0F52BA", "#ADD8E6", 25000, true, updateInfo?.HtmlUrl);
                }
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
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(ConfirmationPassword))
            {
                StatusMessage = "Please Enter your Credentials";
                ColorsC = "red";
                return;
            }

            if (string.Equals(Password, ConfirmationPassword, StringComparison.Ordinal))
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
                BgOpSignUp = false;
                BgPopSignUp = false;
                BgOp = true;
                BgPop = true;
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
                    item.MasterPass = _masterPassword;
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
        public async Task LoginCommand()
        {
            ProgressBarVisible = true;
            ColorsC = "White";
            StatusMessage = "This may take few seconds...";
            if (string.IsNullOrWhiteSpace(Password))
            {
                ProgressBarVisible = false;
                StatusMessage = "Please enter your password";
                ColorsC = "red";
                return;
            }

            bool passwordStatus = await Task.Run(() => _appServices.isAuthenticated(Password));
            if (!passwordStatus)
            {
                ProgressBarVisible = false;
                StatusMessage = "Invalid Password";
                ColorsC = "red";
                return;
            }

            _masterPassword = Password;
            await Task.Run(() => _appServices.show_all_data(Password));
            Password = string.Empty;
            BgPop = false;
            BgOp = false;
            GoToHome();
        }

    }
}
