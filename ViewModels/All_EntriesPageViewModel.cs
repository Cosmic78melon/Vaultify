using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Password_Manager.Service;
using Python.Runtime;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using System.Text.Json;

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
            await Task.Delay(100);
            try
            {
                Items = new ObservableCollection<Entry>();
                dynamic data = await Task.Run(() => _pythonAPI.show_all_data(password));
                using (Py.GIL())
                {
                    foreach (dynamic item in data)
                    {
                        Items.Add(new Entry
                        {
                            Title = item["Site"],
                            Username = item["Username"],
                            Password = item["Password"],
                            strength = item["Strength"],
                            Color_S = ((string.Compare(Convert.ToString(item["Strength"]), "Strong") == 0) ? "ForestGreen":"red"),
                            category = item["Category"]
                        });
                    }
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
        public void save()
        {
            try
            {
                bool isAuthenticated = _pythonAPI.isAuthenticated(HomepagePassword);
                if (string.Equals(Password, ConfirmationPassword, StringComparison.OrdinalIgnoreCase) && isAuthenticated)
                {
                    bool data = Convert.ToBoolean(_pythonAPI.addCredentials(HomepagePassword, Webname, userName: Name, password: Password, "Nothing", catagory:Catagory, false)); 
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
            catch (Exception)
            {
                StatusMessage = "Something Went Wrong";
                ColorC = "red";
            }
        }

        [RelayCommand]
        public void GeneratePassword()
        {
            string password = _pythonAPI.Generate_password();
            Password = ConfirmationPassword = password;
        }        
    }
}
