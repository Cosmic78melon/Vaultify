using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Password_Manager.Service;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Password_Manager.ViewModels
{
    public class Entry
    {
        public required string Title { get; set; }
        public required string Password { get; set; }
        public required string Username { get; set; }
        public required string strength { get; set; }
        public required string Color_S { get; set; }
        public string category { get; set; }
    }
    
    public class FileType
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Icons { get; set; }
        
    }

    public class changebleObj
    {
        public string user_name { get; set; }
        public string password { get; set; }
        public string site_name { get; set; }
        public string category { get; set; }
        public string favourite { get; set; }
    }
    
    public partial class All_EntriesPageViewModel : PageViewModel
    {  
        [ObservableProperty] private bool _addnewOP = false;
        [ObservableProperty] private bool _addnewPOP = false;
        [ObservableProperty] private bool _shareOP = false;
        [ObservableProperty] private bool _sharePOP = false;
        [ObservableProperty] private bool _changePasswordOP = false;
        [ObservableProperty] private bool _changePasswordPOP = false;
        [ObservableProperty] private string _statusMessage = string.Empty;
        [ObservableProperty] private string _colorC = "white";
        [ObservableProperty] private string _name;
        [ObservableProperty] private string _webname;
        [ObservableProperty] public string _password;
        [ObservableProperty] private string _confirmationPassword;
        [ObservableProperty] public string _catagory;
        [ObservableProperty] private string _homepagePassword;
        [ObservableProperty] private FileType _selectedItem;
        [ObservableProperty] private bool _isDecrypted = false;
        

        public readonly IPythonAPI _pythonAPI;
        public readonly FilePickerService _filePickerService;

        [RelayCommand]
        public void AddnewPOPButton()
        {
            AddnewPOP = AddnewOP = true;
        }
        [RelayCommand]
        public void SharePopButton()
        {
            ShareOP = SharePOP = true;
        }
        [RelayCommand]
        public void ChangePasswordPOPButton()
        {
            ChangePasswordOP = ChangePasswordPOP = true;
        }
        [RelayCommand]
        public void CancelButton()
        {
            AddnewOP = AddnewPOP = ShareOP = SharePOP = ChangePasswordOP = ChangePasswordPOP = false;
        }

        private ObservableCollection<Entry> _items;
        public ObservableCollection<Entry> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public All_EntriesPageViewModel(IPythonAPI pythonAPI, FilePickerService filePickerService)
        {
            _pythonAPI = pythonAPI;
            _filePickerService = filePickerService;
        }

        public async Task LoadData(string password)
        {
            try
            {
                Items = new ObservableCollection<Entry>();
                var data = await Task.Run(() => _pythonAPI.show_all_data(password));
                foreach (var item in data)
                    {
                        Items.Add(new Entry
                        {
                            Title = item.siteName,
                            Username = item.userName,
                            Password = item.password,
                            strength = item.strength,
                            Color_S = ((string.Compare(item.strength, "Strong", StringComparison.OrdinalIgnoreCase) == 0) ? "ForestGreen":"red"),
                            category = item.cateGory
                        });
                    }
            }
            catch(Exception ex)
            {
                Items.Add(new Entry
                {
                    Title = "Nothing",
                    Username = ex.Message,
                    Password = "Nothing",
                    strength = "Nothing",
                    Color_S = "white",
                    category = "Nothing"
                });
            }
            
        }
        
        public ObservableCollection<FileType> Categories { get; } = new()
        {
            new FileType
            {
                Name = "CSV Document",
                Extension = ".csv",
                Icons = "DocumentCsv"
            },
            new FileType
            {
                Name = "Excel Document",
                Extension = ".xlsx",
                Icons = "Document"
            },
            new FileType
            {
                Name = "XML Document",
                Extension = ".xml",
                Icons = "DocumentData"
            },
            new FileType
            {
                Name = "Html Document",
                Extension = ".html",
                Icons = "DocumentCss"
            },
            new FileType
            {
                Name = "Json Document",
                Extension = ".json",
                Icons = "JavaScript"
            },
            new FileType
            {
                Name = "Text Document",
                Extension = ".txt",
                Icons = "DocumentText"
            }
        };
        
        [RelayCommand]
        public async Task SaveFile()
        {
            if (string.IsNullOrEmpty(SelectedItem.Extension))
            {
                SelectedItem.Extension = ".csv";
            }
            var path = await _filePickerService.SaveFile(HomepagePassword);
            bool result = _pythonAPI.ExportVault(HomepagePassword, SelectedItem.Extension, path, IsDecrypted);
            if (result == true)
            {
                SharePOP = false;
                ShareOP = false;
            }
        }


        [RelayCommand]
        public async Task save()
        {
            try
            {
                bool isAuthenticated = _pythonAPI.isAuthenticated(HomepagePassword);
                if (string.Equals(Password, ConfirmationPassword, StringComparison.OrdinalIgnoreCase) && isAuthenticated)
                {
                    bool data = await _pythonAPI.addCredentials(HomepagePassword, Webname, userName: Name, password: Password, "Nothing", catagory:Catagory, false); 
                    if (data)
                    {
                        StatusMessage = "Password Saved Successfully";
                        ColorC = "green";
                    }
                    else
                    {
                        StatusMessage = "Password Was Not Saved";
                        ColorC = "red";
                    }
                }
            }
            catch (Exception e)
            {
                StatusMessage = "Something Went Wrong";
                ColorC = "yellow";
            }
        }

        [RelayCommand]
        public async Task GeneratePassword()
        {
            string password = await _pythonAPI.CustomeGen(true, true, true, true);
            Password = ConfirmationPassword = password;
        }        
    }
}
