using Avalonia.Automation.Peers;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Password_Manager.Service;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

        [RelayCommand]
        public void AddnewPOPButton()
        {
            AddnewPOP = AddnewOP = true;
        }
        [RelayCommand]
        public void SharePOPButton()
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

        public async void LoadData(string password)
        {   
            try
            {
                PythonAPI pythoneHelper = new PythonAPI();
                Items = new ObservableCollection<Entry>();
                dynamic data = await Task.Run(() => pythoneHelper.show_all_data(password));
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
            catch(Exception)
            {
                Items.Add(new Entry
                {
                    Title = "Nothing",
                    Username = "Nothing",
                    Password = "Nothing",
                    catagory = "Nothing"
                });
            }
            
        }

        [RelayCommand]
        public async void save()
        {
            try
            {
                PythonAPI python = new PythonAPI();
                bool isAuthenticated = await Task.Run(() => python.check_Password(HomepagePassword));
                if (string.Equals(Password, ConfirmationPassword, StringComparison.OrdinalIgnoreCase) && isAuthenticated)
                {
                    dynamic data = await Task.Run(() => python.addCredentials(HomepagePassword, Webname, Password, catagory: Catagory, user_Name: Name));
                    if (data == true)
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
            PythonAPI python = new PythonAPI();
            string password = python.Generate_password().ToString();
            Password = ConfirmationPassword = password;
        }
    }
}
