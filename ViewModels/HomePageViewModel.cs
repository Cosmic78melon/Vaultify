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


namespace Password_Manager.ViewModels
{
    public class PythonHelper
    {
        public class PasswordCheckDetails
        {
            public string Result { get; set; } = "Unknown";
            public bool? HasUppercase { get; set; }
            public bool? HasLowercase { get; set; }
            public bool? HasDigits { get; set; }
            public bool? HasPunctuation { get; set; }
            public bool? IsLongEnough { get; set; }
        }
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
        public static object Generate_password(string site_name = null, int Length = 12)
        {
            using (Py.GIL())
            {
                dynamic SecurityModule = SecurityMod();
                try
                {
                    dynamic PwManager = SecurityModule.PasswordManager(site_name, null, true, Length);
                    dynamic result = PwManager.GeneratePass();
                    return result;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                return null;
            }
        }
        public object CustomeGen(string site_name = null, int Length = 12, bool has_Letter = true, bool hasNum = true, bool hasPunc = true)
        {
            using (Py.GIL())
            {
                dynamic SecurityModule = SecurityMod();
                if ((has_Letter && hasNum && hasPunc) == true)
                {
                    dynamic data = Generate_password(site_name, Length);
                    return data;
                }
                using dynamic PwManager = SecurityModule.PasswordManager(site_name, null, true, Length);
                dynamic result = PwManager.Custom_GeneratePass(has_Letter, hasNum, hasPunc);
                return result;
            }
        }
        public dynamic PassswordCheck(string Password = null)
        {
            var details = new PasswordCheckDetails();
            if (string.IsNullOrEmpty(Password))
            {
                details.Result = "No Password";
                return details;
            }
            using (Py.GIL())
            {
                dynamic SecurityModule = SecurityMod();
                try
                {
                    using dynamic PwManger = SecurityModule.PasswordManager("Uknown", Password, false);
                    details.Result = PwManger.Check_Password();
                    PyObject testResultPy = PwManger.TestResult;
                    if (testResultPy != null && !testResultPy.IsNone())
                    {
                        var dict = testResultPy["Cause"];
                        if (dict != null && !dict.IsNone())
                        {
                            details.HasUppercase = GetBoolOrNull(dict["hasUppercase"]);
                            details.HasLowercase = GetBoolOrNull(dict["hasLowercase"]);
                            details.HasDigits = GetBoolOrNull(dict["hasDigits"]);
                            details.HasPunctuation = GetBoolOrNull(dict["hasPunc"]);
                            details.IsLongEnough = GetBoolOrNull(dict["isLong"]);
                        }
                    }
                    return details;
                }
                catch (Exception ex)
                {
                    details.Result = ex.Message;
                }
                return details;
            }
        }
        private static bool? GetBoolOrNull(PyObject dict)
        {
            if (dict == null || dict.IsNone()) return null;
            var value = dict;
            if (value == null || value.IsNone()) return null;
            try
            {
                if (value.IsTrue()) return false;
                else return true;
            }
            catch
            {
                return null;
            }
        }
    }

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
        dynamic python_helper = new PythonHelper();


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
                FontC = "White";
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