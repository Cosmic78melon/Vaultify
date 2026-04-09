using Python.Runtime;
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace Password_Manager.Service
{
    public class PythonAPI: IPythonAPI
    {
        public class passwordCheckDetails
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
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                dynamic os = Py.Import("os");
                string cwd = os.getcwd();
                string parent = os.path.abspath(os.path.join(cwd, os.pardir, os.pardir, os.pardir));
                sys.path.append(Path.Combine(parent, "Security"));
                return Py.Import("Security");
            }
        }
        public string Generate_password(string? siteName = null, int length = 12)
        {
            dynamic securityModule = SecurityMod();
            using (Py.GIL())
            {
                dynamic pwManager = securityModule.PasswordManager(siteName, "Nothing", true, length);
                dynamic result = pwManager.GeneratePass();
                return result.ToString();
            }
        }
        public string CustomeGen(string? siteName = null, int length = 12, bool hasLetter = true, bool hasNum = true, bool hasPunc = true)
        {
            using (Py.GIL())
            {
                dynamic securityModule = SecurityMod();
                if (hasLetter && hasNum && hasPunc)
                {
                    dynamic data = Generate_password(siteName, length);
                    return data;
                }
                using dynamic pwManager = securityModule.PasswordManager(siteName, null, true, length);
                dynamic result = pwManager.Custom_GeneratePass(hasLetter, hasNum, hasPunc);
                return result.ToString();
            }
        }
        public dynamic PassswordCheck(string? password = null)
        {
            var details = new passwordCheckDetails();
            if (string.IsNullOrEmpty(password))
            {
                details.Result = "No password";
                return details;
            }
            using (Py.GIL())
            {
                dynamic securityModule = SecurityMod();
                try
                {
                    dynamic pwManger = securityModule.PasswordManager("Unknown", password);
                    details.Result = pwManger.Check_Password();
                    PyObject testResultPy = pwManger.TestResult;
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
            // I have no fucking Idea how the code works, but it works as intended so don't touch anything
            if (dict.IsNone()) return null;
            try
            {
                if (dict.IsTrue()) return false; // why?? it works
                else return true;
            }
            catch
            {
                return null;
            }
        }

        public dynamic show_all_data(string password)
        {
            dynamic securityModule = SecurityMod();
            using (Py.GIL())
            {
                dynamic pw = securityModule.PasswordManager("Unknown", password);
                dynamic data = pw.show_all_data();
                return data;
            }
        }

        public bool isAuthenticated(string password)
        {
            dynamic securityModule = SecurityMod();
            using (Py.GIL())
            {
                dynamic pw = securityModule.PasswordManager("password", password);
                bool data = Convert.ToBoolean(pw.IsAuthenticated());
                return data;
            }
        }
        public bool isNewUser()
        {
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic manager = python.PasswordManager("Password Manager");
                bool data = Convert.ToBoolean(manager.new_user());
                return data; 
            }
        }

        public bool addCredentials(string masterPass,string siteName = "password Manager", string user_Name = "Nothing", string password = "Noting", string message = "unknown", string catagory = "unknown", bool favourite = false) 
            // TODO: add a password strength checker
        {
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic manager = python.PasswordManager(siteName, masterPass);
                dynamic data = Convert.ToBoolean(manager.encryptAndStoredata(user_Name, password, message, catagory, favourite));
                return data;
            }
        }

        public bool register(string userName, string password)
        {
            bool isNewUser = this.isNewUser();
            bool isAuthenticated = this.isAuthenticated(password);
            if (isAuthenticated && !isNewUser)
            {
                return false;
            }
            bool isRegistered = this.addCredentials(masterPass:password, user_Name:userName);
            return true;
        }
        public dynamic card_Data(string masterpassword)
        {
            dynamic securityModule = SecurityMod();
            using (Py.GIL())
            {
                dynamic manager = securityModule.PasswordManager("Password Manager", masterpassword);
                dynamic data = manager.vaultStatusCheck();
                return data;
            }
        }
    }
}
