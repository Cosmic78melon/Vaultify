using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Password_Manager.Service;

namespace Password_Manager.ViewModels
{
    public class SecurityNote
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Date { get; set; }
    }

    public partial class SecurityPageViewModel : PageViewModel
    {
        public readonly IAppServices _appservices;
        [ObservableProperty] public string _passwordSecureity;
        [ObservableProperty] public bool _confirmDialougeVisible = false;
        [ObservableProperty] public bool _confirmDialouge = false;
        [ObservableProperty] public bool _securityDialougeVisible = false;
        [ObservableProperty] public string _securityNotesToSave = string.Empty;
        [ObservableProperty] public string _statusMessage = string.Empty;
        [ObservableProperty] public string _colorC = string.Empty;
        private ObservableCollection<SecurityNote> _securityNotes;

        public ObservableCollection<SecurityNote> SecureNotes
        {
            get { return _securityNotes; }
            set { SetProperty(ref _securityNotes, value); }
        }

        public SecurityPageViewModel(IAppServices appServices)
        {
            _appservices = appServices;
        }

        public async Task ItemLoadNotes(string password)
        {
            SecureNotes = new ObservableCollection<SecurityNote>();
            try
            {
                var data = await Task.Run(() => _appservices.show_all_data(password));
                if (data == null) return;
                foreach (var item in data)
                {
                    if (!string.Equals(item.notes, "Nothing", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(item.notes, "null", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(item.notes, "Unknown", StringComparison.OrdinalIgnoreCase))
                    {
                        SecureNotes.Add(new SecurityNote
                        {
                            Id = item.Id,
                            Description = item.notes,
                            Date = item.createdAt
                        });
                    }
                }

            }
            catch (Exception)
            {
                SecureNotes.Add(new SecurityNote
                {
                    Description = "Nothing",
                    Date = "Nothing"
                });
            }
        }

        [RelayCommand]
        private async Task SaveNotes()
        {
            var (data, id, _, time) = await _appservices.addCredentials(PasswordSecureity, "null", "null", "null", SecurityNotesToSave, "null", false);
            if (data)
            {
                StatusMessage = "Successful";
                ColorC = "green";
                SecureNotes.Add(new SecurityNote
                {
                    Id = id,
                    Description = SecurityNotesToSave,
                    Date = time
                });
            }
            else
            {
                StatusMessage = "Failed to Save";
                ColorC = "red";
            }
        }

        [RelayCommand]
        private async Task CopyNotes(SecurityNote item)
        {
            if (item == null) return;
            var clipboard = CopyTextsServices.Get();
            await clipboard.SetTextAsync(item.Description);
        }

        [RelayCommand]
        private async Task DeleteItem(SecurityNote item)
        {
            if (item == null) return;
            ConfirmDialougeVisible = true;
            await Task.Delay(1500);
            if (ConfirmDialouge)
            {
                bool isRemoved = await _appservices.remove_data(item.Id, PasswordSecureity);
                if (isRemoved)
                {
                    SecureNotes.Remove(item);
                }
            }
        }

        [RelayCommand]
        public void NewNotes()
        {
            SecurityDialougeVisible = true;
            ConfirmDialouge = false;
            ConfirmDialougeVisible = false;
        }

        [RelayCommand]
        public void yesbutton()
        {
            ConfirmDialouge = true;
            ConfirmDialougeVisible = false;
        }

        [RelayCommand]
        private void CancelButton()
        {
            ConfirmDialouge = false;
            SecurityDialougeVisible = false;
            ConfirmDialougeVisible = false;
        }
    }
}
