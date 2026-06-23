using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vaultify.Service
{
    public interface IAppServices
    {
        public Task<string> CustomeGen(bool hasUpperLetters, bool hasLowerLetters, bool hasNum, bool hasPunc, int length = 12);
        public Task<dynamic> PassswordCheck(string password = null!);
        public List<AppServices.vaultData> show_all_data(string password);
        public bool isAuthenticated(string password);
        public bool isNewUser();
        public Task<bool> register(string userName, string password);
        public Task<(bool isAdded, string Id, string strength, string time)> addCredentials(string masterpassword, string siteName, string userName = "Nothing", string password = "Noting", string message = "unknown", string catagory = "unknown", bool favourite = false);
        public Dictionary<string, int> card_Data();
        public List<string> favData();
        public List<int> statusdata();
        public bool ExportVault(string Command, dynamic file_path);
        public Task<bool> remove_data(string Id, string password);

    }
}
