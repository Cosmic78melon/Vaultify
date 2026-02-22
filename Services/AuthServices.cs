using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Password_Manager.Database;
using System.Threading.Tasks;

namespace Password_Manager.Services
{
    public partial class AuthServices: IAuthServices
    {
        private readonly Int16 DelayTime = 500;
        public async Task<AuthResult> Authenticate(string username, string password)
        {
            await Task.Delay(DelayTime);
            AuthResult authResult = new AuthResult();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                authResult.Succces = false;
                authResult.ErrorMessage = "Invalid Username or Password";
                return authResult;
            }

            var stored = Database.DataManager.GetCredentials(username);
            if (stored is null)
            {
                authResult.Succces = false;
                authResult.ErrorMessage = "User not found";
                return authResult;
            }

            if (stored != password)
            {
                authResult.Succces = false;
                authResult.ErrorMessage = "Invalid Username or Password";
                return authResult;
            }

            authResult.Succces = true;
            authResult.Username = username;
            return authResult;

        }
        
        public async Task<AuthResult> LogoutUsers(string username, string password)
        {
            await Task.Delay(DelayTime);

            AuthResult authResult = new AuthResult();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                authResult.Succces = false;
                authResult.ErrorMessage = "Invalid Username or Password";
                return authResult;
            }
            authResult.Succces = true;
            authResult.Username = username;
            Database.DataManager.DeleteCredentials(username);
            return authResult;
        }

        public async Task<AuthResult> Register(string username, string password)
        {
            await Task.Delay(DelayTime);
            AuthResult authResult = new AuthResult();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                authResult.Succces = false;
                authResult.ErrorMessage = "Invalid Username or Password";
                return authResult;
            }

            var existing = Database.DataManager.GetCredentials(username);
            if (existing is not null)
            {
                authResult.Succces = false;
                authResult.ErrorMessage = "User already exists";
                return authResult;
            }

            bool saved = Database.DataManager.SaveCredentials(username, password);
            if (!saved)
            {
                authResult.Succces = false;
                authResult.Username = username;
                authResult.ErrorMessage = "Unable to save credentials";
                return authResult;
            }

            authResult.Succces = true;
            authResult.Username = username;
            return authResult;
        }
    }
}
