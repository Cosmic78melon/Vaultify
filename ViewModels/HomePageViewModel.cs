using System;
using System.IO;
using Meilisearch;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Reflection;
using Password_Manager.Service;
using CSnakes.Runtime;
using CSnakes.Runtime.Python;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Policy;
using System.ComponentModel;
using System.Collections.ObjectModel;
using CommunityToolkit.HighPerformance.Enumerables;
using Python.Runtime;
using PyObject = Python.Runtime.PyObject;


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
        public IPythonAPI _pythonAPI;
        [ObservableProperty]
        private string _passwordGenerator = string.Empty;

        [ObservableProperty] private bool _hasLetterCase = true;
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
        
        public HomePageViewModel(IPythonAPI pythonAPI)
        {
            _pythonAPI = pythonAPI;
        }

        [RelayCommand]
        public async Task GeneratePassword()
        {
            string result = await Task.Run(() => _pythonAPI.CustomeGen(has_Letter: HasLetterCase, hasNum: HasNumCase, hasPunc: HasPuncCase, length: Lenght));
            PasswordGenerator = result;
        }

        [RelayCommand]
        public async Task PasswordChecker()
        {
            var details = await Task.Run(() => _pythonAPI.PassswordCheck(PasswordHaveToCheck));
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
            if (details.HasUppercase != true)
            {
                HasUpper = "✔ Has Uppercase Letter";
                Font1 = "green";
            }
            else
            {
                HasUpper = "❌ Missing Uppercase Letter";
                Font1 = "red";
            }

            if (details.HasLowercase != true)
            {
                Font2 = "green";
                HasLower = "✔ Has Lowercase Letter";
            }
            else
            {
                HasLower = "❌ Missing Lowercase Letter";
                Font2 = "red";
            }

            if (details.HasDigits != true)
            {
                Font3 = "green";
                HasNum = "✔ Has Numbers";
            }
            else
            {
                HasNum = "❌ Missing Numbers";
                Font3 = "red";
            }

            if (details.HasPunctuation != true)
            {
                HasPunc = "✔ Has Special Characters Letter";
                Font4 = "green";
            }
            else
            {
                HasPunc = "❌ Missing Special Characters Letter";
                Font4 = "red";

            }

            if (details.IsLongEnough != true)
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
            details.HasUppercase = null;
            details.HasLowercase = null;
            details.HasPunctuation = null;
            details.HasDigits = null;
            details.IsLongEnough = null;
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
            using (Py.GIL())
            {
                dynamic raw_data = _pythonAPI.favData(MasterPassword);
                FavData.Clear();
                foreach(var name in raw_data)
                {
                    FavData.Add( new FavDataTitle
                    {
                        title = Convert.ToString(name)
                    });
                }
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
            using (Py.GIL())
            {
                PyDict pyDict = _pythonAPI.card_Data(MasterPassword);
                Items.Clear();
                foreach(var key in pyDict)
                {
                    var value = pyDict[key];
                    string key_str = key.ToString();
                    int value_int = Convert.ToInt32(value);
                    Items.Add( new HomeCard
                        {
                            title = key_str,
                            number = value_int
                        });
                }
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
            using (Py.GIL())
            {
                dynamic pyStatusdata = _pythonAPI.statusdata(MasterPassword);
                StatusData.Add(new StatusData
                {
                    total = pyStatusdata[0],
                    strongCount = pyStatusdata[1],
                    weakCount = pyStatusdata[2],
                    breachCount = pyStatusdata[3]
                });
            }
        }
    }
}