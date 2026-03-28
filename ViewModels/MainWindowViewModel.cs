using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Password_Manager.Factory;
using System.Diagnostics;
using System;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Password_Manager.Service;
using Password_Manager.Models;

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
            Debug.WriteLine(password_status);
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

        public int SeacrhbarSize => (IsExpanded) ? 220 : 1;

        [ObservableProperty]
        public PageViewModel _currentPage;

        [ObservableProperty] public bool _homePageActive;
        [ObservableProperty] public bool _all_EntriesActive;
        [ObservableProperty] public bool _securityNotesActive;
        [ObservableProperty] public bool _settingsPageActive;
        [ObservableProperty] public bool _accountPageActive;


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
            CurrentPage = _pageFactory.GetPageViewModel<HomePageViewModel>(item => item.load_data_recent(MasterPassword));
            UpdateActiveState(PageViewData.Home);
        }  

        [RelayCommand]
        public async void GoToAll_Entries()
        {
            CurrentPage = _pageFactory.GetPageViewModel<All_EntriesPageViewModel>(async item =>
            {
                item.HomepagePassword = MasterPassword;
                await Task.Run(() => item.LoadData(MasterPassword));
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
