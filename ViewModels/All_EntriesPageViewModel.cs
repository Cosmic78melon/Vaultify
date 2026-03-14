using Avalonia.Automation.Peers;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public class SecurityPythonCall
    { 
        private static dynamic SecurityMod()
        {
            dynamic sys = Py.Import("sys");
            dynamic os = Py.Import("os");
            string cwd = os.getcwd();
            string parent = os.path.abspath(os.path.join(cwd, os.pardir, os.pardir, os.pardir));
            sys.path.append(Path.Combine(parent, "Security"));
            dynamic SecurityModule = Py.Import("Security");
            return SecurityModule;
        }

        private dynamic EncryptionAndStore(string site_name, string username)
        {
            using (Py.GIL())
            {
                dynamic securityModule = SecurityMod();
                //try
                //{
                //    dynamic pw_manager = securityModule.PasswordManager(site_name, null, true, Length);
                //}
            }
            return null;
        }
    }

    public class Entry
    {
        public string Title { get; set; }
        public string Password { get; set; }
    }
    public partial class All_EntriesPageViewModel : PageViewModel
    {
        [ObservableProperty] private bool _addnewOP = false;
        [ObservableProperty] private bool _addnewPOP = false;
        [ObservableProperty] private bool _shareOP = false;
        [ObservableProperty] private bool _sharePOP = false;
        [ObservableProperty] private bool _changePasswordOP = false;
        [ObservableProperty] private bool _changePasswordPOP = false;
        [ObservableProperty] private string _statusMessage;
        [ObservableProperty] private string _name;
        [ObservableProperty] private string _password;
        [ObservableProperty] private string _confirmationPassword;

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

        public All_EntriesPageViewModel()
        {
            Items = new ObservableCollection<Entry>
            {
                new Entry { Title = "Google", Password = "abc123" },
                new Entry { Title = "GitHub", Password = "xyz456" },
                new Entry { Title = "Email", Password = "mail789" }
            };
        } 
    }
}
