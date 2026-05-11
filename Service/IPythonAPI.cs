namespace Password_Manager.Service
{
    public interface IPythonAPI
    {
        public string CustomeGen(bool hasUpperLetters, bool hasLowerLetters, bool hasNum, bool hasPunc, int length = 12);
        public dynamic PassswordCheck(string password = null!);
        public dynamic show_all_data(string password);
        public bool isAuthenticated(string password);
        public bool isNewUser();
        public bool register(string userName, string password);
        public bool addCredentials(string masterpassword, string siteName, string userName = "Nothing", string password = "Noting", string message = "unknown", string catagory = "unknown", bool favourite = false);
        public dynamic card_Data(string masterpassword);
        public dynamic favData(string masterpassword);
        public dynamic statusdata(string masterpassword);
        public bool ExportVault(string masterPass, string Command, dynamic file_path, bool isDec);
        public bool change_Data(string masterpassword, string id, string json_obj);
    }
}
