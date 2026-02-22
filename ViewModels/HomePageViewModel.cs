using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Python.Runtime;

namespace Password_Manager.ViewModels
{
    public static class PythonHelper
    {
        public static void Python_Script(string script)
        {
            Runtime.PythonDLL = @"C:\Users\Digital Computer\AppData\Local\Programs\Python\Python312\Python312.dll";
            PythonEngine.Initialize();

            using(Py.GIL())
            {
                var pythonScript = Py.Import("Security/" + script);
                var userName = new PyString("Test_1");
                var Password = new PyString("password");
                bool HealthCheck = true;
                bool generate = false;

                dynamic Password_Manager = pythonScript.PasswordManager;
                dynamic manage_1 = Password_Manager(userName,  Password, HealthCheck, generate);
                dynamic result = manage_1.
            }
        }
    }

    public partial class HomePageViewModel : PageViewModel
    {
        [ObservableProperty]
        private string generatedPassword = "Null";

        [RelayCommand]
        public void GeneratePassword()
        {
            // Logic to generate password
            return; // Example
        }
    }
}
