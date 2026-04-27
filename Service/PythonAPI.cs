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
                sys.path.append(Path.Combine(cwd, "Security"));
                return Py.Import("Security");
            }
        }
        public string Generate_password(string? siteName = null, int length = 12)
        {
            dynamic securityModule = SecurityMod();
            using (Py.GIL())
            {
                dynamic pwManager = securityModule.PasswordManager(siteName, "Nothing", length);
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
                using dynamic pwManager = securityModule.PasswordManager(siteName, null, length);
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
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic manager = python.PasswordManager(null, password);
                dynamic data = manager.show_all_data();
                return data;
            }
        }

        public bool isAuthenticated(string password)
        {
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic manager = python.PasswordManager(null, password);
                dynamic data = manager.IsAuthenticated();
                return data; 
            } 
        }
        public bool isNewUser()
        {
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic manager = python.PasswordManager();
                bool data = manager.isNewUser();
                return data;
            }
        }


        public bool addCredentials(string masterPass,string siteName, string userName, string password, string message = "unknown", string category = "unknown", bool favourite = false) 
        {
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                if (this.isAuthenticated(masterPass) != true)
                {
                    return false;
                }
                dynamic manager = python.PasswordManager(siteName,  masterPass);
                bool data = manager.encryptAndStoredata(userName, password, message, category, favourite);
                return data;
            }
        }
        public bool register(string userName, string password)
        {
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic details = this.PassswordCheck(password);
                if (!string.Equals(details.Result, "Strong", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                dynamic manager = python.PasswordManager("Password Manager", password);
                bool data = manager.encryptAndStoredata(userName, password);
                return data;
            }
        }
        public dynamic statusdata(string masterpassword)
        {
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic manager = python.PasswordManager(null, masterpassword);
                dynamic data = manager.status();
                return data;
            }
        }
        public dynamic favData(string masterpassword)
        {
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic manager = python.PasswordManager(null, masterpassword);
                dynamic data = manager.favourite_card_data();
                return data;
            }
        }

        public dynamic card_Data(string masterpassword)
        {
            throw new NotImplementedException();
        }
    }
}
