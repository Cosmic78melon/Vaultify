using System;
using System.Collections.Generic;
using System.Linq;
using CredentialManagement;

namespace Password_Manager.Database
{
    public partial class DataManager
    {
        public static bool SaveCredentials(string username, string password, string target = "Master_")
        {
            using (var cred = new Credential())
            {
                cred.Target = target + username;
                cred.Username = username;
                cred.Password = password;
                cred.Type = CredentialType.Generic;
                return cred.Save();
            }
        }

        public static string GetCredentials(string username, string target = "Master_")
        {
            using (var cred = new Credential())
            {
                cred.Target = target + username;
                if (cred.Load())
                {
                    return cred.Password;
                }
                return null;
            }
        }

        public static bool DeleteCredentials(string username, string target = "Master_")
        { 
            using (var cred = new Credential())
            {
                cred.Target = target + username;
                return cred.Delete();
            }
        }
    }
}
