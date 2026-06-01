using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Password_Manager.Service;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Password_Manager.ViewModels
{
    public class Entry
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public required string Password { get; set; }
        public required string Username { get; set; }
        public required string Strength { get; set; }
        public required string ColorS { get; set; }
        public required string Category { get; set; }
        public required string Time { get; set; }
    }
    
    public class FileType
    {
        public required string Name { get; set; }
        public required string Extension { get; set; }
        public required string Icons { get; set; }
        
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

        [ObservableProperty] public bool _confimationDialog = false;
        [ObservableProperty] public bool _confirmDelete = false;

        public readonly IAppServices _appServices;
        public readonly FilePickerService _filePickerService;
        public readonly CopyTextsServices _copyTexts;

        [RelayCommand]
        public void AddnewPopButton()
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

        public All_EntriesPageViewModel(IAppServices appServices, FilePickerService filePickerService)
        {
            _appServices = appServices;
            _filePickerService = filePickerService;
        }

        public async Task LoadData(string password)
        {
            try
            {
                Items = new ObservableCollection<Entry>();
                var data = await Task.Run(() => _appServices.show_all_data(password));
                foreach (var item in data)
                {
                    if (!string.Equals(item.siteName, "null", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Items.Add(new Entry
                        {
                            Id =  item.Id,
                            Title = item.siteName,
                            Username = item.userName,
                            Password = item.password,
                            Strength = item.strength,
                            ColorS = ((string.Compare(item.strength, "Strong", StringComparison.OrdinalIgnoreCase) == 0) ? "ForestGreen":"red"),
                            Category = item.cateGory,
                            Time = item.createdAt
                        });
                    }
                }
            }
            catch(Exception ex)
            {
                Items.Add(new Entry
                {
                    Id =  null,
                    Title = "Nothing",
                    Username = ex.Message,
                    Password = "Nothing",
                    Strength = "Nothing",
                    ColorS = "white",
                    Category = "Nothing",
                    Time = "Loading................"
                });
            }
            
        }
        
        public ObservableCollection<FileType> Categories { get; } =
        [
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
            }
        ];
        
        [RelayCommand]
        private async Task SaveFile()
        {
            if (string.IsNullOrEmpty(SelectedItem.Extension))
            {
                SelectedItem.Extension = ".csv";
            }
            var path = await _filePickerService.SaveFile(HomepagePassword);
            bool result = _appServices.ExportVault(SelectedItem.Extension, path);
            if (result)
            {
                SharePOP = false;
                ShareOP = false;
            }
        }


        [RelayCommand]
        private async Task save()
        {
            try
            {
                bool isAuthenticated = _appServices.isAuthenticated(HomepagePassword);
                if (string.Equals(Password, ConfirmationPassword, StringComparison.OrdinalIgnoreCase) && isAuthenticated)
                {
                    var (data, Id, strength, time) = await _appServices.addCredentials(HomepagePassword, Webname, userName: Name, password: Password, "Nothing", catagory:Catagory, false); 
                    if (data)
                    {
                        StatusMessage = "Password Saved Successfully";
                        ColorC = "green";
                        Items.Add(new Entry
                        {
                            Id = Id,
                            Title = Webname,
                            Username =  Name,
                            Category = Catagory,
                            ColorS = ((string.Compare(strength, "Strong", StringComparison.OrdinalIgnoreCase) == 0) ? "ForestGreen":"red"),
                            Password = Password,
                            Strength = strength,
                            Time = time
                        });
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
                ColorC = "yellow";
            }
        }
        
        [RelayCommand]
        private async Task CopyUsername(Entry items)
        {
            if (items == null) return;
            var clipboard = CopyTextsServices.Get();
            await clipboard.SetTextAsync(items.Username);

        }        
        [RelayCommand]
        private async Task CopyPassword(Entry items)
        {
            if (items == null) return;
            var clipboard = CopyTextsServices.Get();
            await clipboard.SetTextAsync(items.Password);
        }        
        [RelayCommand]
        private async Task DeleteItem(Entry items)
        {
            if (items == null) return;
            ConfimationDialog = true;

            await Task.Delay(1555);
            if (ConfirmDelete)
            {
                bool isDeleted = await _appServices.remove_data(items.Id, HomepagePassword);
                if (isDeleted)
                {
                    Items.Remove(items);
                }
            }
        }
        [RelayCommand]
        private void yesButton()
        {
            ConfirmDelete = true;
            ConfimationDialog = false;
        }
        [RelayCommand]
        private async Task GeneratePassword()
        {
            string password = await _appServices.CustomeGen(true, true, true, true);
            Password = ConfirmationPassword = password;
        }        
    }
}
