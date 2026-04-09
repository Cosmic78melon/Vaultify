using Avalonia.Automation.Peers;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Password_Manager.Service;
using Python.Runtime;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_Manager.ViewModels
{
    public class Entry
    {
        public string Title { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string catagory { get; set; }
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

        public readonly IPythonAPI _pythonAPI;

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

        public All_EntriesPageViewModel(IPythonAPI pythonAPI)
        {
            _pythonAPI = pythonAPI;
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
                            Title = item["Site Name"],
                            Username = item["User Name"],
                            Password = item["Password"],
                            catagory = item["Catagory"]
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
                    catagory = "Nothing"
                });
            }
            
        }

        [RelayCommand]
        public async Task save()
        {
            try
            {
                bool isAuthenticated = await Task.Run(() => _pythonAPI.isAuthenticated(HomepagePassword));
                if (string.Equals(Password, ConfirmationPassword, StringComparison.OrdinalIgnoreCase) && isAuthenticated)
                {
                    bool data = await Task.Run(() => _pythonAPI.addCredentials(HomepagePassword, Webname, Name, Password, Catagory)); 
                    if (data != false)
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
        public async Task GeneratePassword()
        {
            string password = _pythonAPI.Generate_password();
            Password = ConfirmationPassword = password;
        }
    }
}
