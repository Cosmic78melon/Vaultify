using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Password_Manager.Service;
using System.Collections.ObjectModel;


namespace Password_Manager.ViewModels
{
    public class HomeCard()
    {
        public string title { get; set; }
        public int number { get; set; } 
    }
    public class FavDataTitle()
    {
        public string title { get; set; }
    }
    public class StatusData()
    {
        public int total {get; set;}
        public int strongCount { get; set; }
        public int weakCount { get; set; }
        public  int breachCount {get; set; }
    }
    public partial class HomePageViewModel : PageViewModel
    {
        public IAppServices _appServices;
        [ObservableProperty]
        private string _passwordGenerator = string.Empty;

        [ObservableProperty] private bool _hasUpperLetter = true;
        [ObservableProperty] private bool _hasLowerLetter = true;
        [ObservableProperty] private bool _hasNumCase = true;
        [ObservableProperty] private bool _hasPuncCase = true;
        [ObservableProperty] private int _lenght;
        [ObservableProperty] private string _master_pass;


        [ObservableProperty] 
        private string _passwordHaveToCheck;

        [ObservableProperty] private string _password_result = "Password Status: Unknown";
        [ObservableProperty] private string _hasUpper = "Include Uppercase Letter";
        [ObservableProperty] private string _hasLower = "Include Lowercase Letter";
        [ObservableProperty] private string _hasNum = "Include Numbers";
        [ObservableProperty] private string _hasPunc = "Include Special Characters Letter";
        [ObservableProperty] private string _islong = "Not Long Enough";
        [ObservableProperty] private string _fontC = "White";
        [ObservableProperty] private string _font1 = "White";
        [ObservableProperty] private string _font2 = "White";
        [ObservableProperty] private string _font3 = "White";
        [ObservableProperty] private string _font4 = "White";
        [ObservableProperty] private string _font5 = "White";
        
        public HomePageViewModel(IAppServices appServices)
        {
            _appServices = appServices;
        }

        [RelayCommand]
        public async Task GeneratePassword()
        {
            string result = await Task.Run(() => _appServices.CustomeGen(length: Lenght, hasUpperLetters: HasUpperLetter, hasLowerLetters: HasLowerLetter, hasNum: HasNumCase, hasPunc: HasPuncCase));
            PasswordGenerator = result;
        }

        [RelayCommand]
        public async Task PasswordChecker()
        {
            var details = await _appServices.PassswordCheck(PasswordHaveToCheck);
            if (string.Equals(details.Result, "Strong", StringComparison.OrdinalIgnoreCase))
            {
                Password_result = "Password Status: " + details.Result;
                FontC = "green";
                CheckChar(details);
            }
            else if (string.Equals(details.Result, "Weak", StringComparison.OrdinalIgnoreCase))
            {
                Password_result = "Password Status: " + details.Result;
                FontC = "red";
                CheckChar(details);
            }
            else if (string.Equals(details.Result, "Breached", StringComparison.OrdinalIgnoreCase))
            {
                Password_result = "Password Status: " + details.Result;
                CheckChar(details);
                FontC = "yellow";
            }
            else
            {
                Password_result = "Something Went Wrong⁉⁉";
                CheckChar(details);
                FontC = "Yellow";
            }
        }
        private void CheckChar(dynamic details)
        {
            if (details.HasUppercase)
            {
                HasUpper = "✔ Has Uppercase Letter";
                Font1 = "green";
            }
            else
            {
                HasUpper = "❌ Missing Uppercase Letter";
                Font1 = "red";
            }

            if (details.HasLowercase)
            {
                Font2 = "green";
                HasLower = "✔ Has Lowercase Letter";
            }
            else
            {
                HasLower = "❌ Missing Lowercase Letter";
                Font2 = "red";
            }

            if (details.HasDigits)
            {
                Font3 = "green";
                HasNum = "✔ Has Numbers";
            }
            else
            {
                HasNum = "❌ Missing Numbers";
                Font3 = "red";
            }

            if (details.HasPunctuation)
            {
                HasPunc = "✔ Has Special Characters Letter";
                Font4 = "green";
            }
            else
            {
                HasPunc = "❌ Missing Special Characters Letter";
                Font4 = "red";

            }

            if (details.IsLongEnough)
            {
                Islong = "✔ Password meets length requirements";
                Font5 = "green";
            }
            else
            {
                Islong = "❌ Not Long Enough";
                Font5 = "red";
            }
            if (string.Equals(details.Result, "Breached", StringComparison.OrdinalIgnoreCase))
            {
                HasUpper = HasLower = HasNum = HasPunc = Islong = null;
                FontC = "White";
            }
            details.HasUppercase = false;
            details.HasLowercase = false;
            details.HasPunctuation = false;
            details.HasDigits = false;
            details.IsLongEnough = false;
        }
        private ObservableCollection<FavDataTitle> _favData;
        public ObservableCollection<FavDataTitle> FavData
        {
            get { return _favData; }
            set { SetProperty(ref _favData, value); } 
        }

        public void favouriteData(string MasterPassword)
        {
            FavData = new ObservableCollection<FavDataTitle>();
            List<string> raw_data = _appServices.favData(MasterPassword);
            FavData.Clear();
            foreach(string name in raw_data)
            {
                FavData.Add( new FavDataTitle
                {
                    title = name
                });
            }
        }
        
        private ObservableCollection<HomeCard> _items;
        public ObservableCollection<HomeCard> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); } 
        }

        public void load_data_recent(string MasterPassword)
        {
            Items = new ObservableCollection<HomeCard>();
            Dictionary<string, int> card_Data = _appServices.card_Data(MasterPassword);
            Items.Clear();
            foreach(var pair in card_Data)
            {
                Items.Add( new HomeCard
                    {
                        title = pair.Key,
                        number = pair.Value
                    });
            }
        }
        
        private ObservableCollection<StatusData> _statusData;

        public ObservableCollection<StatusData> StatusData
        {
            get { return _statusData; }
            set { SetProperty(ref _statusData, value); }
        }

        public void statusDataLoad(string MasterPassword)
        {
            StatusData = new ObservableCollection<StatusData>();
            StatusData.Clear();
            
            List<int> Statusdata = _appServices.statusdata(MasterPassword);
            StatusData.Add(new StatusData
            {
                total = Statusdata[0],
                strongCount = Statusdata[1],
                weakCount = Statusdata[2],
                breachCount = Statusdata[3]
            });
        }
    }
}