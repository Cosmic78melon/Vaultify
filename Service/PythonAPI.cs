using System.Net.Http;
using System.Collections.Generic;
using Python.Runtime;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cryptography;
using System.Security.Cryptography;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Data.Sqlite;

namespace Password_Manager.Service
{
    public class PythonAPI: IPythonAPI
    {
        public static char[] AsciiPuncs = new char[] { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '=', '+', '[', ']', '{', '}', '<', '>', '?', '"'};
        public static char[] AsciiNumbers = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        public static int AlphaC = 26;
        char[] _asciiLowerLetters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        char[] _asciiUpperLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private readonly string DPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "DataBase"), "encrypted-data.db");
        private const int KeySize = 32;
        public static bool isAuth = false;
        
        public static List<vaultData> global_Data = new();

        public class vaultData
        {
            public string Id {get; set;}
            public string Salt {get; set;}
            public int Iteration {get; set;}
            public string siteName {get; set;}
            public string userName {get; set;}
            public string password {get; set;}
            public string notes {get; set;}
            public string cateGory {get; set;}
            public string strength {get; set;}
            public bool favourite {get; set;}
            public string createdAt {get; set;}
            public string updatedAt {get; set;}
            
        }
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
        public async Task<string> CustomeGen(bool hasUpperLetters, bool hasLowerLetters, bool hasNum, bool hasPunc, int length = 12)
        {
            bool[] lenResult = { hasLowerLetters, hasUpperLetters, hasNum, hasPunc};
            int boolCount = lenResult.Count(x => x);
            if (boolCount < 2) return "Not Possible";
            if (length < 12) return "Not Possible";
            
            var rawData = new StringBuilder();
            if (hasNum) rawData.Append(AsciiNumbers);
            if (hasLowerLetters) rawData.Append(_asciiLowerLetters);
            if (hasUpperLetters) rawData.Append(_asciiUpperLetters);
            if (hasPunc) rawData.Append(AsciiPuncs);
            
            if (hasLowerLetters && hasNum && hasPunc && hasUpperLetters)
            {
                while (true)
                {
                    string password = new string(RandomNumberGenerator.GetItems(rawData.ToString().AsSpan(), length));
                    var result = await this.PassswordCheck(password);
                    if (string.Equals(result.Result, "Strong", StringComparison.OrdinalIgnoreCase))
                    {
                        return password;
                    }
                    
                }
            }
            return new string(RandomNumberGenerator.GetItems(rawData.ToString().AsSpan(), length));
        }

        private async Task<int> IsPasswordBreached(string password)
        {
            try
            {
                var sha1 = SHA1.Create();
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);
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
        public async Task<dynamic> PassswordCheck(string? password = null)
        {
            var details = new passwordCheckDetails();
            if (string.IsNullOrEmpty(password))
            {
                details.Result = "No password";
                return details;
            }

            int isBreached = await this.IsPasswordBreached(password);
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

                if (hasL > 0 && hasU > 0 && hasP > 0 && hasN > 0 && password.Length >= 12) details.Result = "Strong";
            }

            return details;
        }

        public List<vaultData> show_all_data(string password)
        {
            if (string.IsNullOrEmpty(password) || isAuth != true) return global_Data;

            using (var connection = new SqliteConnection($"Data Source={DPath}"))
            {
                connection.Open();
                
                // Get current database row count
                using var countCommand = new SqliteCommand(
                    "SELECT COUNT(*) FROM Credential_Data WHERE Id != 0;",
                    connection);
                
                int dbCount = Convert.ToInt32(countCommand.ExecuteScalar());
                if (global_Data.Count == dbCount) return global_Data;
                    
                global_Data.Clear();
                var sql = "SELECT * FROM Credential_Data where Id != 0 ORDER BY Updated_at DESC;";
                int id = 0;

                using var command = new SqliteCommand(sql, connection);
                

                using SqliteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        vaultData allData = new vaultData { Id = reader.GetString(0), Salt = reader.GetString(1), Iteration = reader.GetInt32(2), cateGory = reader.GetString(7), 
                            strength = reader.GetString(8), createdAt = reader.GetString(10), updatedAt = reader.GetString(11)};
                        
                        //Derived key 
                        byte[] bsalt = Convert.FromHexString(reader.GetString(1));
                        string fernetKey = this.DeriveKey(bsalt, reader.GetInt32(2), password);
                        allData.siteName = Fernet.Decrypt(fernetKey, reader.GetString(3));
                        allData.userName = Fernet.Decrypt(fernetKey, reader.GetString(4));
                        allData.password = Fernet.Decrypt(fernetKey, reader.GetString(5));
                        allData.notes = Fernet.Decrypt(fernetKey, reader.GetString(6));
                        bool fav = reader.GetInt16(9) == 1;
                        allData.favourite = fav;
                        global_Data.Add(allData);
                    }
                }
                return global_Data;
            }
        }

        public bool isAuthenticated(string password)
        {
            if (string.IsNullOrEmpty(password) || Path.Exists(DPath) == false) return false;
            using var connection = new SqliteConnection($"Data Source={DPath}");
            connection.Open();
            const string sql = "SELECT * FROM Credential_Data WHERE Id = @id";
            int id = 0;

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using SqliteDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    byte[] salt = Convert.FromHexString(reader.GetString(1));
                    int iteration = reader.GetInt32(2);
                    string encPass = reader.GetString(5);

                    try
                    {
                        string decPassword = this.DecryptedData(salt, iteration, encPass, password);
                        if (password != decPassword)
                        {
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }
            }
            isAuth = true;
            show_all_data(password);
            return isAuth;
        }
        public bool isNewUser()
        {
            if (string.IsNullOrEmpty(DPath)) return true;
            if (Path.Exists(DPath)) return false;
            return true;
        }


        public bool addCredentials(string masterPass,string siteName, string userName, string password, string message = "unknown", string category = "unknown", bool favourite = false) 
        {
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                if (isAuth != true)
                {
                    return false;
                }
                dynamic manager = python.PasswordManager(siteName,  masterPass);
                bool data = manager.encryptAndStoredata(userName, password, message, category, favourite);
                return Convert.ToBoolean(data);
            }
        }
        public async Task<bool> register(string userName, string password)
        {
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic details = await this.PassswordCheck(password);
                if (!string.Equals(details.Result, "Strong", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                dynamic manager = python.PasswordManager("Password Manager", password);
                bool data = manager.encryptAndStoredata(userName, password);
                return data;
            }
        }
        public List<int> statusdata(string masterpassword)
        {
            if (isAuth != true) return null;
            
            int total = 0;
            int weak = 0;
            int strong = 0; 
            int breached = 0;
            foreach (var item in global_Data)
            {
                total++;
                if (string.Equals(item.strength, "Strong", StringComparison.OrdinalIgnoreCase))
                {
                    strong++;
                }                    
                else if (string.Equals(item.strength, "Weak", StringComparison.OrdinalIgnoreCase))
                {
                    weak++;
                }                 
                else if (string.Equals(item.strength, "Breached", StringComparison.OrdinalIgnoreCase))
                {
                    breached++;
                }
            }

            return new List<int> { total, strong, weak, breached };
        }
        public List<string> favData(string masterpassword)
        {
            if (isAuth != true) return null;
            
            List<string> card_data = new();
            
            foreach (var item in global_Data)
            {
                if (card_data.Contains(item.siteName) != true) card_data.Add(item.siteName);
            }
            return card_data;
        }

        public Dictionary<string, int> card_Data(string masterpassword)
        {
            if (isAuth != true) return null;
            Dictionary<string, int> card_data = new();
            foreach (var item in global_Data)
            {
                if (card_data.ContainsKey(item.siteName))
                {
                    card_data[item.siteName]++;
                }
                else
                {
                    card_data.Add(item.siteName, 1);
                }
            }
            return card_data;
        }

        public bool ExportVault(string masterPass, string command, dynamic filePath, bool isDec)
        {
            if (isAuth != true) return false;
            dynamic python = SecurityMod();
            using (Py.GIL())
            {
                dynamic manager = python.PasswordManager("null", masterPass);
                bool data = manager.share_data(command, filePath, isDec);
                return data;
            }
        }

        public bool change_Data(string masterpassword, string id, string jsonObj)
        {
            return false;
        }

        private string DecryptedData(byte[] salt, int iteration, string encText, string password)
        {
            string fernetKeyString = DeriveKey(salt, iteration, password); 
            string decryptedText = Fernet.Decrypt(fernetKeyString, encText);
            return decryptedText;
        }
        
        private string DeriveKey(byte[] salt, int iteration, string password)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iteration, HashAlgorithmName.SHA256);
            byte[] dKey = pbkdf2.GetBytes(KeySize);
            string fernetKeyString = Convert.ToBase64String(dKey).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            return fernetKeyString;
        }
    }
}
