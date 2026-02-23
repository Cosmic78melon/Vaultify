using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Password_Manager.ViewModels
{
    public static class PythonHelper
    {
        public static object Python_Script(string site_name = "Unknown", string Password = "", bool shouldGenerate = false, int Length = 12)
        { 
            using(Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                sys.path.append(Path.Combine(@"C:\Users\Digital Computer\Documents\Passord Manager\Password_Manager\Password Manager\Security\"));

                using dynamic SecurityModule = Py.Import("Security");
                using dynamic PwManager = SecurityModule.PasswordManager(site_name, Password, shouldGenerate, Length);
                //Debug.WriteLine(Thread.CurrentThread.ManagedThreadId);
                if (shouldGenerate)
                {
                    object result = PwManager.GeneratePass();
                    return result;
                }
                else if (!shouldGenerate && string.IsNullOrWhiteSpace(Password))
                {
                    var result = PwManager.Check_Password();
                    return result.As<Dictionary<string, object>>();
                }
                return null;

            }
        }
    }


    public partial class HomePageViewModel : PageViewModel
    {
        [ObservableProperty]
        private string _passwordGenerator;

        [RelayCommand]
        public async void GeneratePassword()
        {
            object result = PythonHelper.Python_Script(Password:"1234", shouldGenerate: true, Length: 22);
            PasswordGenerator = result?.ToString() ?? string.Empty;
        }

    }
}
