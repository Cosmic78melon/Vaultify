using System.Collections.Generic;
using System.Threading.Tasks;

namespace Password_Manager.Service
{
    public interface IAppServices
    {
        public Task<string> CustomeGen(bool hasUpperLetters, bool hasLowerLetters, bool hasNum, bool hasPunc, int length = 12);
        public Task<dynamic> PassswordCheck(string password = null!);
        public List<AppServices.vaultData> show_all_data(string password);
        public bool isAuthenticated(string password);
        public bool isNewUser();
        public Task<bool> register(string userName, string password);
        public Task<bool> addCredentials(string masterpassword, string siteName, string userName = "Nothing", string password = "Noting", string message = "unknown", string catagory = "unknown", bool favourite = false);
        public Dictionary<string, int> card_Data(string masterpassword);
        public List<string> favData(string masterpassword);
        public List<int> statusdata(string masterpassword);
        public bool ExportVault(string masterPass, string Command, dynamic file_path, bool isDec);
        public bool loginAuth(string password);

    }
}
