using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_Manager.Services
{
    public interface IAuthServices
    {
        public Task<AuthResult> Authenticate(string username, string password);
        public Task<AuthResult> LogoutUsers(string username, string password);
        public Task<AuthResult> Register(string username, string password);
    }
    public class AuthResult
    {
        public  bool Succces {get; set;} = false;
        public  string ErrorMessage {get; set;} = string.Empty;
        public string Username {get; set;} = string.Empty;

    }

}
