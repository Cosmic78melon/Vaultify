using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Vaultify.Service;
using System;
using System.Collections.Generic;
using Vaultify.ViewModels.Messages;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Vaultify.ViewModels
{
    public partial class Entry : ObservableObject  
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Password { get; set; }
    public required string Username { get; set; }
    public required string Strength { get; set; }
    
    [ObservableProperty] private bool _isFav;
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

    public class FavUser
    {
        public string Id {get; set;}
        public string SiteName {get; set;}
        public bool isFav {get; set;}
    }
    
    public partial class All_EntriesPageViewModel : PageViewModel
    {  
        private static readonly string DPath =  Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Vaultify");
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

        [ObservableProperty] 
        [NotifyPropertyChangedFor(nameof(FavouriteText))]
        private Entry _selectedEntry;
        
        [ObservableProperty] private string _selectedwebsite;
        [ObservableProperty] private string _selecteduserName;
        [ObservableProperty] private string _selectedpassword;
        [ObservableProperty] private string _selectedcategory;
        
        public string FavouriteText =>
            SelectedEntry?.IsFav == true
                ? "Remove from Favourite"
                : "Add to Favourite";

        [ObservableProperty] public bool _confimationDialog = false;
        [ObservableProperty] public bool _confirmDelete = false;

        [ObservableProperty] public string _searchText = string.Empty;
        private Entry? _pendingDeleteItem; 
        
        public readonly IAppServices _appServices;
        public readonly FilePickerService _filePickerService;
        
        [ObservableProperty] public bool _isChecked = false;

        public All_EntriesPageViewModel(IAppServices appServices, FilePickerService filePickerService)
        {
            _appServices = appServices;
            _filePickerService = filePickerService;
            Items = new ObservableCollection<Entry>();
            FilteredItems = new ObservableCollection<Entry>();
        }
        
        partial void OnSearchTextChanged(string _)
        {
            ApplyFilter();
        }
        
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

        private ObservableCollection<Entry> _items;
        public ObservableCollection<Entry> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }
        private ObservableCollection<Entry> _filteredItems = new();
        public ObservableCollection<Entry> FilteredItems
        {
            get { return _filteredItems; }
            set { SetProperty(ref _filteredItems, value); }
        }

        public ObservableCollection<string> websitesNames { get; } = new();
        public ObservableCollection<string> userName { get; } = new();
        public ObservableCollection<string> category { get; } = new();
        public ObservableCollection<string> autoCompletepassword { get; } = new();
        public async Task LoadData(string password)
{
    try
    {
        Items = new ObservableCollection<Entry>();
        var data = await Task.Run(() => _appServices.show_all_data(password));
        
        websitesNames.Clear();

        string filePath = Path.Combine(DPath, "favData.json");

        HashSet<string> favoriteIds = new();

        if (File.Exists(filePath))
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                var favList = JsonSerializer.Deserialize<List<FavUser>>(jsonString);
                
                if (favList != null)
                {
                    // Get a fast list of IDs that are actually marked as true favorites
                    favoriteIds = favList
                        .Where(u => u.isFav && !string.IsNullOrEmpty(u.Id))
                        .Select(u => u.Id)
                        .ToHashSet();
                }
            }
            catch (JsonException)
            {
                favoriteIds = new HashSet<string>();
            }
        }

        foreach (var item in data)
        {
            if (!string.Equals(item.SiteName, "null", StringComparison.InvariantCultureIgnoreCase))
            {
                if (item.Id == null || item.SiteName == null || item.UserName == null || 
                    item.Password == null || item.Strength == null || item.CateGory == null || 
                    item.CreatedAt == null) continue;

                // Check if this item's ID exists in our favorites hashset
                bool isActuallyFavorite = favoriteIds.Contains(item.Id);

                var entry = new Entry
                {
                    Id = item.Id,
                    Title = item.SiteName,
                    Username = item.UserName,
                    Password = item.Password,
                    Strength = item.Strength,
                    ColorS = string.Equals(item.Strength, "Strong", StringComparison.OrdinalIgnoreCase) ? "ForestGreen" : "red",
                    
                    IsFav = isActuallyFavorite,
                    Category = item.CateGory,
                    Time = item.CreatedAt
                };

                Items.Add(entry);
                websitesNames.Add(entry.Title);
                userName.Add(entry.Username);
                category.Add(entry.Category);
                autoCompletepassword.Add(entry.Password);
            }
        }

        ApplyFilter();
    }
    catch (Exception ex)
    {
        Items.Clear();
        Items.Add(new Entry
        {
            Id = "Nothing",
            Title = "Nothing",
            Username = ex.Message,
            Password = "Nothing",
            Strength = "Nothing",
            ColorS = "white",
            IsFav = false,
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
            var path = await _filePickerService.SaveFile();
            if (path == null) return; 
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
                            IsFav = false,
                            Password = Password,
                            Strength = strength,
                            Time = time
                        });
                        ApplyFilter();
                        Webname = string.Empty;
                        Name = string.Empty;
                        Password = string.Empty;
                        ConfirmationPassword = string.Empty;
                        Catagory = string.Empty;
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
        
        private void ApplyFilter()
        {
            FilteredItems.Clear();

            foreach (var item in Items)
            {
                if (string.IsNullOrWhiteSpace(SearchText) ||
                    item.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    item.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || 
                    item.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                {
                    FilteredItems.Add(item);
                }
            }
        }
        
        [RelayCommand]
        private async Task CopyUsername(Entry? items)
        {
            if (items == null) return;
            var clipboard = CopyTextsServices.Get();
            await clipboard.SetTextAsync(items.Username);

        }
        
        [RelayCommand]
        public void CancelButton()
        {
            AddnewOP = AddnewPOP = ShareOP = SharePOP = ChangePasswordOP = ChangePasswordPOP = false;
            ConfimationDialog = false;
            _pendingDeleteItem = null;
            Webname = string.Empty;
            Name = string.Empty;
            Password = string.Empty;
            ConfirmationPassword = string.Empty;
            Catagory = string.Empty;

        }

        [RelayCommand]
        private async Task CopyPassword(Entry? items)
        {
            if (items == null) return;
            var clipboard = CopyTextsServices.Get();
            await clipboard.SetTextAsync(items.Password);
        }


        [RelayCommand]
        private async Task DeleteItem(Entry? items)
        {
            if (items == null) return;
            ConfimationDialog = true;
            _pendingDeleteItem = items;
            
        }

        [RelayCommand]
        private async Task AddOrRemoveFav(Entry? items)
        {
            if (items == null) return;
            string filePath = Path.Combine(DPath, "favData.json");
            if (!File.Exists(filePath))
            {
                return;
            }
            bool success;
            items.IsFav = !items.IsFav;
            
            if (items.IsFav)
            {
                items.IsFav = false;
                success = _appServices.AddOrRemoveFavourites(
                    items.Id,
                    items.IsFav,
                    HomepagePassword);
                if (success)
                {
                    WeakReferenceMessenger.Default.Send(
                        new FavouritesChangedMessage(items.Title, items.IsFav));
                    string jsonString = File.ReadAllText(filePath);
                    List<FavUser> favList = JsonSerializer.Deserialize<List<FavUser>>(jsonString);
                    favList.RemoveAll(item => item.Id.Equals(items.Id));
                    string updateJson =
                        JsonSerializer.Serialize(favList, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(filePath, updateJson);
                }
                else
                {
                    items.IsFav = true;
                }
            }
            else
            {
                items.IsFav = true;
                success = _appServices.AddOrRemoveFavourites(
                    items.Id,
                    items.IsFav,
                    HomepagePassword);
                if (success)
                {
                    WeakReferenceMessenger.Default.Send(
                        new FavouritesChangedMessage(items.Title, items.IsFav));
                    string jsonString = File.ReadAllText(filePath);
                    List<FavUser> favList = JsonSerializer.Deserialize<List<FavUser>>(jsonString);
                    favList.Add(new FavUser
                    {
                        Id = items.Id,
                        SiteName = items.Title,
                        isFav = items.IsFav
                    });
                    string updateJson =
                        JsonSerializer.Serialize(favList, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(filePath, updateJson);
                }
                else
                {
                    items.IsFav = false;
                }
            }
        }
        
        [RelayCommand]
        private async Task yesButton()
        {
            if (_pendingDeleteItem == null) return;
            
            bool isDeleted = await _appServices.remove_data(_pendingDeleteItem.Id, HomepagePassword);
            if (isDeleted)
            {
                Items.Remove(_pendingDeleteItem);
                ApplyFilter();
            }

            _pendingDeleteItem = null;
            ConfimationDialog = false;            
        }
        [RelayCommand]
        private async Task GeneratePassword()
        {
            string password = await Task.Run(() => _appServices.CustomeGen(true, true, true, true));
            Password = ConfirmationPassword = password;
        }        
    }
}
