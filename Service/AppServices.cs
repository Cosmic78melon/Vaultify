using ClosedXML.Excel;
using System.Net.Http;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cryptography;
using CsvHelper;
using CsvHelper.Excel.EPPlus;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

namespace Password_Manager.Service
{
    public class AppServices: IAppServices
    {
        public static char[] AsciiPuncs = new char[] { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '=', '+', '[', ']', '{', '}', '<', '>', '?', '"'};
        public static char[] AsciiNumbers = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        char[] _asciiLowerLetters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        char[] _asciiUpperLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private const string Filename = "encrypted-data";
        private readonly string DPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "DataBase"), $"{Filename}.db");
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
    // 1. Reject empty password immediately
    if (string.IsNullOrEmpty(password)) return global_Data;

    // 2. Return cache if user is not authenticated
    if (isAuth != true) return global_Data;

    using (var connection = new SqliteConnection($"Data Source={DPath}"))
    {
        connection.Open();

        using var countCommand = new SqliteCommand(
            "SELECT COUNT(*) FROM Credential_Data WHERE Id != 0;", connection);

        int dbCount = Convert.ToInt32(countCommand.ExecuteScalar());

        // 3. Return cache if row count hasn't changed
        if (global_Data != null && global_Data.Count == dbCount) return global_Data;

        // 4. Safe initialization — THIS is what fixes your crash
        global_Data ??= new List<vaultData>();
        global_Data.Clear();

        var sql = "SELECT * FROM Credential_Data WHERE Id != 0 ORDER BY Updated_at DESC;";
        using var command = new SqliteCommand(sql, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            byte[] bsalt = Convert.FromHexString(reader.GetString(1));
            string fernetKey = this.DeriveKey(bsalt, reader.GetInt32(2), password);

            var allData = new vaultData
            {
                Id        = reader.GetString(0),
                Salt      = reader.GetString(1),
                Iteration = reader.GetInt32(2),
                siteName  = Fernet.Decrypt(fernetKey, reader.GetString(3)),
                userName  = Fernet.Decrypt(fernetKey, reader.GetString(4)),
                password  = Fernet.Decrypt(fernetKey, reader.GetString(5)),
                notes     = Fernet.Decrypt(fernetKey, reader.GetString(6)),
                cateGory  = reader.GetString(7),
                strength  = reader.GetString(8),
                favourite = reader.GetInt16(9) == 1,
                createdAt = reader.GetString(10),
                updatedAt = reader.GetString(11)
            };

            global_Data.Add(allData);
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
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", "0");
            
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
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            isAuth = true;
            return isAuth;
        }
        public bool loginAuth(string password)
        {
            if (isAuthenticated(password))
            {
                show_all_data(password);
                return true;
            }

            return false;
        }
        public bool isNewUser()
        {
            if (string.IsNullOrEmpty(DPath)) return true;
            if (Path.Exists(DPath)) return false;
            return true;
        }


        public async Task<(bool isAdded, string? Id, string? strength, string? time)> addCredentials(string masterPass,string siteName, string userName, string password, string message = "unknown", string category = "unknown", bool favourite = false) 
        {
            int iteration = 299990;
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            var result = await PassswordCheck(password);
            if (!isAuthenticated(masterPass) && isNewUser())
                return (false, null, null, null);
            
            try
            {
                using var connection = new SqliteConnection($"Data Source={DPath}");
                connection.Open();
                
                string fernetKey = DeriveKey(salt, iteration, masterPass);
                Guid myguid = Guid.NewGuid();
                string id = myguid.ToString();
                string encSitename = Fernet.Encrypt(fernetKey, siteName);
                string encUsername = Fernet.Encrypt(fernetKey, userName);
                string encPass = Fernet.Encrypt(fernetKey, password);
                string encMsg = Fernet.Encrypt(fernetKey, message);
                string localNow = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                using var commandLate = new SqliteCommand("INSERT INTO Credential_Data (Id, Salt, Iteration, Site_name, User_name, Password, Notes, Category, Strength, Favourite, Created_at, Updated_at) VALUES (@id, @salt, @iteration, @site, @user, @pass, @notes, @category, @strength, @fav, @created, @updated)", connection)
                {
                    Parameters = 
                    {
                        new SqliteParameter("@id", id),
                        new SqliteParameter("@salt", Convert.ToHexString(salt)), 
                        new SqliteParameter("@iteration", iteration), 
                        new SqliteParameter("@site", encSitename), 
                        new SqliteParameter("@user", encUsername), 
                        new SqliteParameter("@pass", encPass), 
                        new SqliteParameter("@notes", encMsg), 
                        new SqliteParameter("@category", category),
                        new SqliteParameter("@strength", result.Result),
                        new SqliteParameter("@fav", favourite), 
                        new SqliteParameter("@created", localNow),
                        new SqliteParameter("@updated", localNow)
                    }
                };
                await commandLate.ExecuteNonQueryAsync();
                show_all_data(masterPass);
                return (true, id, result.Result, localNow);
            }
            catch (Exception)
            {
                return (false, null, null, null);
            }
        }
        
        public async Task<bool> register(string userName, string password)
        {
            int iteration = 299990;
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            var result = await PassswordCheck(password).Result;
            if (!string.Equals(result.Result, "Strong", StringComparison.OrdinalIgnoreCase) || !isNewUser() || isAuthenticated(password)) return false;
            try
            {
                using var connection = new SqliteConnection($"Data Source={DPath}");
                connection.Open();
                const string sql = @"CREATE TABLE IF NOT EXISTS Credential_Data(Id text, Salt text, Iteration int, Site_name text, User_name text, Password, Notes text, Category text, Strength text, Favourite Boolean, Created_at text, Updated_at text)";
                
                using var command = new SqliteCommand(sql, connection);
                await command.ExecuteNonQueryAsync();
                
                string fernetKey = DeriveKey(salt, iteration, password);
                string encUsername = Fernet.Encrypt(fernetKey, userName);
                string encPass = Fernet.Encrypt(fernetKey, password);
                string localNow = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                using var commandLate = new SqliteCommand("INSERT INTO Credential_Data (Id, Salt, Iteration, Site_name, User_name, Password, Notes, Category, Strength, Favourite, Created_at, Updated_at) VALUES (@id, @salt, @iteration, @site, @user, @pass, @notes, @category, @strength, @fav, @created, @updated)", connection)
                {
                    Parameters = 
                    {
                        new SqliteParameter("@id", "0"),
                        new SqliteParameter("@salt", Convert.ToHexString(salt)), 
                        new SqliteParameter("@iteration", iteration), 
                        new SqliteParameter("@site", "Password Manager"), 
                        new SqliteParameter("@user", encUsername), 
                        new SqliteParameter("@pass", encPass), 
                        new SqliteParameter("@notes", "Null"), 
                        new SqliteParameter("@category", "Security"),
                        new SqliteParameter("@strength", result),
                        new SqliteParameter("@fav", false), 
                        new SqliteParameter("@created", localNow),
                        new SqliteParameter("@updated", localNow)
                    }
                };
                await commandLate.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> remove_data(string Id, string password)
        {
            if (string.IsNullOrEmpty(Id)) return false;
            if (string.IsNullOrEmpty(password)) return false;
            
            if (isAuth)
            {
                using var connection = new SqliteConnection($"Data Source={DPath}");
                connection.Open(); 
                const string sql = "DELETE FROM Credential_Data WHERE id = @id";
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@id", Id);

                int rowAffected = await command.ExecuteNonQueryAsync();
                if (rowAffected > 0)
                {
                    show_all_data(password);
                    return true;
                }
                return false;
            }

            return false;
        }
        
        public List<int> statusdata()
        {
            if (isAuth != true) return new List<int>{0,0,0,0};
            
            int total = 0;
            int weak = 0;
            int strong = 0; 
            int breached = 0;
            foreach (var item in global_Data)
            {
                if (!string.Equals(item.siteName, "null", StringComparison.InvariantCultureIgnoreCase))
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
            }

            return new List<int> { total, strong, weak, breached };
        }
        public List<string> favData()
        {
            if (isAuth != true) return null;
            
            List<string> card_data = new();
            
            foreach (var item in global_Data)
            {
                if (!string.Equals(item.siteName, "null", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (card_data.Contains(item.siteName) != true) card_data.Add(item.siteName);
                }
            }
            return card_data;
        }

        public Dictionary<string, int> card_Data()
        {
            if (isAuth != true) return null;
            Dictionary<string, int> card_data = new();
            foreach (var item in global_Data)
            {
                if (!string.Equals(item.siteName, "null", StringComparison.InvariantCultureIgnoreCase))
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
            }
            return card_data;
        }


        public bool ExportVault(string command, dynamic filePath)
        {
            if (!isAuth)
            {
                return false;
            }


            string fileFormat = Filename + command;
            string path = Path.Combine(filePath, fileFormat);
            if (string.Equals(command, ".csv", StringComparison.InvariantCultureIgnoreCase))
            {
                using (var writer = new StreamWriter(path))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(global_Data);
                }

                return true;
            }
            else if (string.Equals(command, ".xlsx", StringComparison.InvariantCultureIgnoreCase))
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Vault");

                    worksheet.Cell(1, 1).InsertTable(global_Data);

                    workbook.SaveAs(path);
                }

                return true;
            }

            return false;
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
