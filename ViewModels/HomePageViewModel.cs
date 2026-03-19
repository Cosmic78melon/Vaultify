using System;
using System.IO;
using Python.Runtime;
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


namespace Password_Manager.ViewModels
{
    public partial class HomePageViewModel : PageViewModel
    {
        [ObservableProperty]
        private string _passwordGenerator = string.Empty;

        [ObservableProperty] private bool _hasLetterCase = true;
        [ObservableProperty] private bool _hasNumCase = true;
        [ObservableProperty] private bool _hasPuncCase = true;
        [ObservableProperty] private int _lenght;


        [ObservableProperty] 
        private string _passwordHaveToCheck = string.Empty;

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
        PythonAPI python_helper = new PythonAPI();


        [RelayCommand]
        public async Task GeneratePassword()
        {
            object result = await Task.Run(() => python_helper.CustomeGen(has_Letter: HasLetterCase, hasNum: HasNumCase, hasPunc: HasPuncCase, Length: Lenght));
            PasswordGenerator = result?.ToString() ?? string.Empty;
        }

        [RelayCommand]
        public async Task PasswordChecker()
        {
            var details = await Task.Run(() => python_helper.PassswordCheck(PasswordHaveToCheck));
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
                FontC = "White";
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
    }
}