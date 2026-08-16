using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Vaultify.Service;
using System.Collections.ObjectModel;
using System.Linq;


namespace Vaultify.ViewModels
{
    public class HomeCard()
    {
        public required string Title { get; set; }
        public required int Number { get; set; } 
    }
    public class FavDataTitle()
    {
        public required string? FTitle { get; set; }
    }
    public class StatusData()
    {
        public int Total {get; set;}
        public int StrongCount { get; set; }
        public int WeakCount { get; set; }
        public  int BreachCount {get; set; }
    }
    public partial class HomePageViewModel : PageViewModel
    {
        private IAppServices _appServices;
        [ObservableProperty]
        private string _passwordGenerator = string.Empty;

        [ObservableProperty] private bool _hasUpperLetter = true;
        [ObservableProperty] private bool _hasLowerLetter = true;
        [ObservableProperty] private bool _hasNumCase = true;
        [ObservableProperty] private bool _hasPuncCase = true;
        [ObservableProperty] private int _lenght = 12;
        [ObservableProperty] private string? _masterPass;
        


        [ObservableProperty] 
        private string _passwordHaveToCheck;

        [ObservableProperty] private string _passwordResult = "Password Status: Unknown";
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
        [ObservableProperty] private bool _isLoading = true;
        public HomePageViewModel(IAppServices appServices)
        {
            _appServices = appServices;
            _ = GeneratePassword();
        }

        [RelayCommand]
        public async Task GeneratePassword()
        {
            string result = await Task.Run(() => _appServices.CustomeGen(length: Lenght, hasUpperLetters: HasUpperLetter, hasLowerLetters: HasLowerLetter, hasNum: HasNumCase, hasPunc: HasPuncCase));
            PasswordGenerator = result;
        }
        public ObservableCollection<int> Boxes { get; } = new(Enumerable.Range(1, 6));
        partial void OnMasterPassChanged(string? value)
        {
            if (value == null) return;
            IsLoading = false;
            StatusDataLoad();
            load_data_recent();
            FavouriteData();
        }

        [RelayCommand]
        public async Task PasswordChecker()
        {
            if (string.IsNullOrEmpty(PasswordHaveToCheck))
            {
                PasswordResult = "Please Enter your password";
                FontC = "yellow";
                return;
            }
            var details = await _appServices.PassswordCheck(PasswordHaveToCheck);
            
            if (string.Equals(details.Result, "Strong", StringComparison.OrdinalIgnoreCase))
            {
                PasswordResult = "Password Status: " + details.Result;
                FontC = "green";
                CheckChar(details);
            }
            else if (string.Equals(details.Result, "Weak", StringComparison.OrdinalIgnoreCase))
            {
                PasswordResult = "Password Status: " + details.Result;
                FontC = "red";
                CheckChar(details);
            }
            else if (string.Equals(details.Result, "Breached", StringComparison.OrdinalIgnoreCase))
            {
                PasswordResult = "Password Status: " + details.Result;
                CheckChar(details);
                FontC = "yellow";
            }
            else
            {
                PasswordResult = "Something Went Wrong⁉⁉";
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
                HasUpper = HasLower = HasNum = HasPunc = Islong = null!;
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

        public void FavouriteData()
        {
            FavData = new ObservableCollection<FavDataTitle>();
            List<string> rawData = _appServices.favData();
            FavData.Clear();
            foreach(string name in rawData)
            {
                FavData.Add( new FavDataTitle
                {
                    FTitle = name
                });
            }
        }
        
        private ObservableCollection<HomeCard> _items;
        public ObservableCollection<HomeCard> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); } 
        }

        public void load_data_recent()
        {
            Items = new ObservableCollection<HomeCard>();
            Dictionary<string, int> cardData = _appServices.card_Data();
            Items.Clear();
            foreach(var pair in cardData)
            {
                Items.Add( new HomeCard
                    {
                        Title = pair.Key,
                        Number = pair.Value
                    });
            }

        }
        
        private ObservableCollection<StatusData> _statusData;

        public ObservableCollection<StatusData> StatusData
        {
            get { return _statusData; }
            set { SetProperty(ref _statusData, value); }
        }

        public void StatusDataLoad()
        {
            StatusData = new ObservableCollection<StatusData>();
            StatusData.Clear();
            
            List<int> statusdata = _appServices.statusdata();
            StatusData.Add(new StatusData
            {
                Total = statusdata[0],
                StrongCount = statusdata[1],
                WeakCount = statusdata[2],
                BreachCount = statusdata[3]
            });
        }
       [RelayCommand]
       public async Task copyPass()
       {
           if (PasswordGenerator == null) return;
           var clipboard = CopyTextsServices.Get();
           await clipboard.SetTextAsync(PasswordGenerator);
       }
        
    }
}