using System.Net.Http;
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
        public static char[] ascii_puncs = new char[] { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '=', '+', '[', ']', '{', '}', '<', '>', '?', '"'};
        public static char[] ascii_numbers = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        public static int alphaC = 26;
        char[] ascii_lowerLetters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        char[] ascii_upperLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        public class passwordCheckDetails
        {
            public string Result { get; set; } = "Weak";
            public bool? HasUppercase { get; set; } = false;
            public bool? HasLowercase { get; set; } = false;
            public bool? HasDigits { get; set; } = false;
            public bool? HasPunctuation { get; set; } = false;
            public bool? IsLongEnough { get; set; } = false;
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

        private async Task<int> IsPasswordBreached(string password)
        {
            try
            {
                var sha1 = SHA1.Create();
                byte[] inputBytes = Encoding.ASCII.GetBytes(password);
                byte[] hashBytes = sha1.ComputeHash(inputBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "");

                string prefix = hash.Substring(0, 5);
                string suffix = hash.Substring(5);

                using (var client = new HttpClient())
                {
                    using var isActive = await client.GetAsync("https://www.google.com");
                    if (isActive.IsSuccessStatusCode != true) return 101;
                    string response = await client.GetStringAsync($"https://api.pwnedpasswords.com/range/{prefix}");
                    if (response.Contains(suffix))
                    {
                        return 200;
                    }
                    else
                    {
                        return 500;
                    }
                }
            }
            catch (HttpRequestException)
            {
                return 101;
            }
            catch (TimeoutException)
            {
                return 101;
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
            
            int isBreached = this.IsPasswordBreached(password).GetAwaiter().GetResult();
            if (isBreached == 200)
            {
                details.Result = "Breached";
                return details;
            }
            if (isBreached == 101)
            {
                details.Result = "No Internet";
                return details;
            }
            if (isBreached == 500)
            {
                int hasL = 0;
                int hasU = 0;
                int hasP = 0;
                int hasN = 0;
                for (int i = 0; i < password.Length; i++)
                {
                    if (char.IsPunctuation(password[i])) hasP++;
                    if (char.IsAsciiDigit(password[i])) hasN++;
                    if (char.IsAsciiLetterLower(password[i])) hasL++;
                    if (char.IsAsciiLetterUpper(password[i])) hasU++;
                }

                if (hasL > 0) details.HasLowercase = true;
                if (hasU > 0) details.HasUppercase = true;
                if (hasP > 0) details.HasPunctuation = true;
                if (hasN > 0) details.HasDigits = true;
                if (password.Length >= 12) details.IsLongEnough = true;

                if ((hasL + hasU + hasP + hasN) >= 4 && password.Length >= 12) details.Result = "Strong";
            }

            return details;
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
