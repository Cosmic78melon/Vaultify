using Python.Runtime;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_Manager.Service
{
    public class PythonAPI
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
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                dynamic os = Py.Import("os");
                string cwd = os.getcwd();
                string parent = os.path.abspath(os.path.join(cwd, os.pardir, os.pardir, os.pardir));
                sys.path.append(Path.Combine(parent, "Security"));
                dynamic SecurityModule = Py.Import("Security");
                return SecurityModule;
            }
        }
        public object Generate_password(string site_name = null, int Length = 12)
        {
            dynamic SecurityModule = SecurityMod();
            using (Py.GIL())
            {
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
                    dynamic PwManger = SecurityModule.PasswordManager("Uknown", Password);
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

        public dynamic show_all_data(string Password)
        {
            dynamic SecurityModule = SecurityMod();
            using (Py.GIL())
            {
                dynamic pw = SecurityModule.PasswordManager("Uknown", Password);
                dynamic data = pw.show_all_data();
                return data;
            }
        }

        public dynamic check_Password(string Password)
        {
            dynamic SecurityModule = SecurityMod();
            using (Py.GIL())
            {
                dynamic pw = SecurityModule.PasswordManager("password", Password);
                dynamic data = pw.check_password();
                return data;
            }
        }

        public dynamic addCredentials(string MasterPass,string site_name, string Password, string message = "unknown", string catagory = "unknown", string user_Name = "Nothing")
        {
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic manager = python.PasswordManager(site_name, MasterPass);
                dynamic data = manager.encryptAndStoredata(message, Password, catagory, user_Name);
                return data;
            }
        }
    }
}
