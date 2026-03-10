using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_Manager.ViewModels
{
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

        private ObservableCollection<string> _items;

        public ObservableCollection<string> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public All_EntriesPageViewModel()
        {
            Items = new ObservableCollection<string>
            {
                "Item 1",
                "Item 2",
                "Item 3"
            };
        }
    }
}
