using Python.Runtime;
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;
using System.Security.Cryptography;
using CSnakes.Runtime.Python;
using PyObject = Python.Runtime.PyObject;

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
        public string CustomeGen(bool hasUpperLetters, bool hasLowerLetters, bool hasNum, bool hasPunc, int length = 12)
        {
            bool[] lenResult = { hasLowerLetters, hasUpperLetters, hasNum, hasPunc};
            int boolCount = lenResult.Count(x => x);
            if (boolCount < 2) return "Not Possible";
            if (length < 12) return "Not Possible";

            int alphaC = 26;
            char[] ascii_lowerLetters = new char[alphaC];
            char[] ascii_upperLetters = new char[alphaC];
            for (int i = 0; i < (alphaC); i++)
            {
                ascii_lowerLetters[i] = (char)('a' + i);
            }
            for (int i = 0; i < (alphaC); i++)
            {
                ascii_upperLetters[i] = (char)('A' + i);
            }

            char[] ascii_numbers = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
            char[] ascii_puncs = new char[] { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '=', '+', '[', ']', '{', '}', '<', '>', '?', '"'};
 
            var raw_data = new StringBuilder();
            if (hasNum) raw_data.Append(ascii_numbers);
            if (hasLowerLetters) raw_data.Append(ascii_lowerLetters);
            if (hasUpperLetters) raw_data.Append(ascii_upperLetters);
            if (hasPunc) raw_data.Append(ascii_puncs);

            string password = new string(RandomNumberGenerator.GetItems<char>(raw_data.ToString().AsSpan(), length));
            if (hasLowerLetters && hasNum && hasPunc && hasUpperLetters)
            {
                while (true)
                {
                    var result = this.PassswordCheck(password);
                    if (string.Equals(result.Result, "Strong", StringComparison.OrdinalIgnoreCase))
                    {
                        return password;
                    }
                }
            }
            return password;
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
                    dynamic pwManger = securityModule.PasswordManager("null", password);
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
                dynamic manager = python.PasswordManager("null", password);
                dynamic data = manager.show_all_data();
                return data;
            }
        }

        public bool isAuthenticated(string password)
        {
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic manager = python.PasswordManager("null", password);
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
            bool result = this.isAuthenticated(masterpassword);
            if (result != true) return null;
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic manager = python.PasswordManager("null", masterpassword);
                dynamic data = manager.status();
                return data;
            }

            return null;
        }
        public dynamic favData(string masterpassword)
        {
            bool result = this.isAuthenticated(masterpassword);
            if (result != true) return null;
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic manager = python.PasswordManager("null", masterpassword);
                dynamic data = manager.favourite_card_data();
                return data;
            }
            
            return null;
        }

        public dynamic card_Data(string masterpassword)
        {
            bool result = this.isAuthenticated(masterpassword);
            if (result != true) return null;
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic manager = python.PasswordManager("null", masterpassword);
                dynamic data = manager.card_data();
                return data;
            }

            return null;
        }

        public bool ExportVault(string masterPass, string Command, dynamic file_path, bool isDec)
        {
            bool result = this.isAuthenticated(masterPass);
            if (result != true) return false;
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic manager = python.PasswordManager("null", masterPass);
                bool data = manager.share_data(Command, file_path, isDec);
                return data;
            }
        }

        public bool change_Data(string masterpassword, string id, string json_obj)
        {
            return false;
        }
    }
}
