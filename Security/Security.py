import os
import uuid
import sqlite3
import base64
import string
import binascii
import pandas as pd
import datetime as dt
from icecream import ic
import pwnedpasswords as pwend
from cryptography.fernet import Fernet
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.kdf.pbkdf2 import PBKDF2HMAC
ic.disable()

class PasswordManager:
    def __init__(self,
                 site_name: str = "Password Manager",
                 password = None,
                 password_length: int = 12):

        self.password = password
        self.site_name = site_name.lower()
        self.Length = password_length
        self.TestResult = {0:"Strong", 1:"Weak" , 2: "Error", -1: "No Password", 80:"Breached", "Cause": {"Breached": None, "hasUppercase": None, "hasLowercase": None,
                                                                                                          "hasDigits": None, "hasPunc": None, "isLong": None, "Errors": None}}

        # This is are Constants that are not supposed to control by users
        self.iteration = 299990

        base_dir = os.path.dirname(os.path.abspath(__file__))
        project_root = os.path.abspath(os.path.join(base_dir, os.pardir))
        self.path = os.path.join(project_root, "DataBase", "encrypted-data.db")

    def Check_Password(self, password = None) -> str | dict[str, None]:
        if password is None:
            password = self.password

        if not password :
            return self.TestResult[-1]

        self.TestResult["Cause"] = {
            "Breached": None,
            "hasUppercase": None,
            "hasLowercase": None,
            "hasDigits": None,
            "hasPunc": None,
            "isLong": None
        }


        weak = False
        has_lowercase_letters = any(i in string.ascii_lowercase for i in password)
        has_uppercase_letters = any(i in string.ascii_uppercase for i in password)
        has_digits = any(i in string.digits for i in password)
        has_special_Character = any(i in string.punctuation for i in password)

        try:
            if pwend.check(password, timeout=22):
                self.TestResult["Cause"]["Breached"] = True #type: ignore
                return self.TestResult[80]
        except Exception as e:
            self.TestResult["Cause"]["Errors"] = e #type: ignore

        if not has_uppercase_letters:
            self.TestResult["Cause"]["hasUppercase"] = False#type: ignore
            weak = True
        if not has_lowercase_letters:
            self.TestResult["Cause"]["hasLowercase"] = False#type: ignore
            weak = True
        if not has_digits:
            self.TestResult["Cause"]["hasDigits"] = False#type: ignore
            weak = True
        if not has_special_Character:
            self.TestResult["Cause"]["hasPunc"] = False#type: ignore
            weak = True
        if len(password) < 12:
            self.TestResult["Cause"]["isLong"] = False#type: ignore
            weak = True

        if weak:
            return self.TestResult[1]

        self.TestResult["Cause"] = {
            "Breached": False,
            "hasUppercase": True,
            "hasLowercase": True,
            "hasDigits": True,
            "hasPunc": True,
            "isLong": True,
            "Errors": None
        }
        return self.TestResult[0]


    def encryptAndStoredata(self, user_name = "Unknown", Password = "", SecureNote = "Nothing", Category ="Unknown", favourite = False):
        site_name = self.site_name
        user_name = user_name.lower()
        salt = os.urandom(16)
        iteration = self.iteration
        if not os.path.exists(self.path):
            Password = self.password
            strength_check = self.Check_Password(Password)
            if strength_check == self.TestResult[1] or strength_check == self.TestResult[2] or strength_check == self.TestResult[80]:
                return False 

            connection = sqlite3.connect(self.path)
            cursor = connection.cursor()
            cursor.execute("""
                            CREATE TABLE Credential_Data(
                            Id text,
                            Salt text,
                            Iteration int,
                            Site_name text,
                            User_name text,
                            Password,
                            Notes text,
                            Category text,
                            Strength text,
                            Favourite Boolean,
                            Created_at text,
                            Updated_at text
            )
            """)

            derived_key = self._derived_key(salt, iteration)
            key = Fernet(derived_key)
            enc_pass = self._EncJson(key, self.password)
            enc_user_name = self._EncJson(key, user_name)
            Created_at = dt.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            data = ["0", salt.hex(), iteration, "Password Manager", enc_user_name, enc_pass, SecureNote, Category,
                    strength_check, int(favourite),Created_at, Created_at]
            cursor.execute("""INSERT INTO Credential_Data VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)""", data)
            result = cursor.execute("SELECT Salt FROM Credential_Data WHERE Id= 0").fetchall()
            connection.commit()
            cursor.close()
            return True
        else:
            connection = sqlite3.connect(self.path)
            cursor = connection.cursor()

            Id_text = str(uuid.uuid4())
            strength_check = self.Check_Password(Password)
            Created_at = dt.datetime.now().strftime("%Y-%m-%d %H:%M:%S")

            derived_key= self._derived_key(salt, iteration)
            key = Fernet(derived_key)
            enc_pass = self._EncJson(key, Password)
            enc_site_name = self._EncJson(key, site_name)
            enc_user_name = self._EncJson(key, user_name)
            enc_message = self._EncJson(key, SecureNote)
            data = [Id_text, salt.hex(), iteration,enc_site_name, enc_user_name, enc_pass, enc_message, Category, strength_check, int(favourite), Created_at, Created_at]
            cursor.execute("""INSERT INTO Credential_Data VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)""", data)
            connection.commit()
            connection.close()
            return True
    
    def share_data(self, Command,location = "C:/Users/Digital Computer/Downloads", isDecrypted = False):
        if self.IsAuthenticated() != True:
            return False
        connection = sqlite3.connect(self.path)
        df = pd.read_sql_query("SELECT * FROM Credential_Data", connection)

        if os.path.exists(location) != True:
            os.mkdir(location)
            
        if isDecrypted != True:
            match Command:
                case ".csv":
                    df.to_csv(os.path.join(location, "Credential_Data.csv"), index = False)
                case ".xlsx":
                    df.to_excel(os.path.join(location, "Credential_Data.xlsx"), index = False)
                case ".json":
                    df.to_json(os.path.join(location, "Credential_Data.json"), orient = "records")
                case ".xml":
                    df.to_xml(os.path.join(location, "Credential_Data.xml"))
                case ".html":
                    df.to_html(os.path.join(location, "Credential_Data.html"))
                case ".txt":
                    df.to_csv(os.path.join(location, "Credential_Data.txt"), index = False)
                case _:
                    return False
            return True
        elif isDecrypted == True:
            data = {"Id": [], "Salt": [],"Iteration": [],"Site": [],"Username": [], "Password": [], "Notes": [],"Strength": [], "Category": [], "Favourite": [],"Created_at": [], "Updated_at": []}
            for item in self.show_all_data():
                data["Id"].append(item["Id"])
                data["Site"].append(item["Site"])
                data["Salt"].append(item["Salt"])
                data["Iteration"].append(item["Iteration"])
                data["Username"].append(item["Username"])
                data["Password"].append(item["Password"])
                data["Notes"].append(item["Notes"])
                data["Strength"].append(item["Strength"])
                data["Category"].append(item["Category"])
                data["Favourite"].append(item["Favourite"])
                data["Created_at"].append(item["Created_at"])
                data["Updated_at"].append(item['Updated_at'])
            
            df = pd.DataFrame(data)
            match Command:
                case ".csv":
                    df.to_csv(os.path.join(location, "Credential_Data.csv"), index=False)
                case ".xlsx":
                    df.to_excel(os.path.join(location, "Credential_Data.xlsx"), index=False)
                case ".json":
                    df.to_json(os.path.join(location, "Credential_Data.json"), orient="records")
                case ".xml":
                    df.to_xml(os.path.join(location, "Credential_Data.xml"))
                case ".html":
                    df.to_html(os.path.join(location, "Credential_Data.html"))
                case ".txt":
                    df.to_csv(os.path.join(location, "Credential_Data.txt"), index=False)
                case _:
                    return False
            return True
        else:
            return False


            
        

    def _EncJson(self, fernet, plainText):
        """
        Encrypt a text using a provided Fernet instance.
        """
        if plainText is None:
            raise ValueError("Invalid value")
        try:
            from cryptography.fernet import Fernet as _FernetType
            is_fernet = isinstance(fernet, _FernetType)
        except Exception:
            is_fernet = hasattr(fernet, "encrypt")

        if not is_fernet:
            raise ValueError("Invalid Fernet instance")


        encrypted_text = fernet.encrypt(plainText.encode())
        return encrypted_text

    def _derived_key(self, salt, iteration):
        password = self.password
        if password is None:
            return "Invalid Password"

        if salt is None:
            return "Invalid Salt"

        kdf = PBKDF2HMAC(
            hashes.SHA256(),
            32,
            salt = salt,
            iterations=iteration
        )
        return base64.urlsafe_b64encode(kdf.derive(password.encode()))

    def show_all_data(self):
        decrypted_list = []
        path = self.path

        if not os.path.exists(path):
            return decrypted_list

        if self.IsAuthenticated() == False:
            return decrypted_list

        connection = sqlite3.connect(path)
        cursor = connection.cursor()
        cursor.execute("SELECT * FROM Credential_Data where Id != 0 ORDER BY Updated_at DESC;")
        raw_data = cursor.fetchall()
        
        for item in raw_data:
            temp = {"Id": None,"Site": None, "Salt": None,"Iteration": None,"Username": None, "Password": None,
                    "Notes": None,"Strength": None, "Category": None, "Favourite": None,"Created_at": None,
                    "Updated_at": None}
            temp["Id"] = item[0]
            temp["Salt"] = item[1]
            temp["Iteration"] = item[2]
            salt = binascii.unhexlify(item[1])
            temp["Site"] = self.decryptdata(salt, item[2], item[3])
            temp["Username"] = self.decryptdata(salt, item[2], item[4])
            temp["Password"] = self.decryptdata(salt, item[2], item[5])
            temp["Notes"] = self.decryptdata(salt, item[2], item[6])
            temp["Category"] = item[7]
            temp["Strength"] = item[8]
            temp["Favourite"] = True if item[9] == 1 else False
            temp["Created_at"] = item[10]
            temp["Updated_at"] = item[11]
            
            decrypted_list.append(temp)
        connection.commit()
        connection.close()
        return decrypted_list

    def decryptdata(self, salt, iteration,plaintext):
        if isinstance(salt, str):
            salt = binascii.unhexlify(salt)

        dKey = self._derived_key(salt, iteration)
        key = Fernet(dKey)
        decoded_text = key.decrypt(plaintext).decode()
        return decoded_text



if __name__ == "__main__":
    pw_1 = PasswordManager("shikho", "@@Adol2280@@")
    print(pw_1.show_all_data())
    print(pw_1.card_data())





