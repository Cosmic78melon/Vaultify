# Importing Important Libraries
import os
import json
import uuid
import sqlite3
import base64
import random
import string
import secrets
import binascii
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
                 Password_Length: int = 12):

        self.password = password
        self.site_name = site_name
        self.Length = Password_Length
        self.TestResult = {0:"Strong", 1:"Weak" , 2: "Error", -1: "No Password", 80:"Breached", "Cause": {"Breached": None, "hasUppercase": None, "hasLowercase": None,
                                                                                                          "hasDigits": None, "hasPunc": None, "isLong": None, "Errors": None}}

        # This is are Constants that are not supposed to control by users
        self.minimum = 0
        self.maximum = 9999
        self.iteration = 299990
        self.Pure_Random_Ints = self._randomNumGen(100000,self.minimum, self.maximum)

        base_dir = os.path.dirname(os.path.abspath(__file__))
        project_root = os.path.abspath(os.path.join(base_dir, os.pardir))
        self.path = os.path.join(project_root, "DataBase", "encrypted-data.db")

    def Check_Password(self, Password = None) -> str | dict[str, None]:
        """
        This function Checks is the password is strong or not by looking at how many characters does this have enough letters or char ect.
        and also this function checks is the password branched or not so we can ensure full safety of the password

        Status: ✔Complete
        """
        if Password is None:
            Password = self.password

        if not Password:
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
        has_lowercase_letters = any(i in string.ascii_lowercase for i in Password)
        has_uppercase_letters = any(i in string.ascii_uppercase for i in Password)
        has_digits = any(i in string.digits for i in Password)
        has_special_Character = any(i in string.punctuation for i in Password)

        try:
            if pwend.check(Password, timeout=22):
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
        if len(Password) < 12:
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

    def GeneratePass(self) -> str | None:
        minimum = self.minimum
        maximum = self.maximum
        if self.Length < 12:
            return "Invalid Length. It must be greater than 12"

        if self.Pure_Random_Ints is None:
            rand = secrets.SystemRandom(10)
            randoms = [rand.randrange(minimum, maximum) for _ in range(10)]
        else:
            randoms = self.Pure_Random_Ints

        random_num = str(secrets.choice(randoms))
        alpha_char = string.ascii_letters + random_num + string.punctuation
        run = True
        while run:
            password = "".join(secrets.choice(alpha_char) for _ in range(self.Length))
            result = self.Check_Password(password)
            if result == self.TestResult[0]:
                return password
        return None

    def Custom_GeneratePass(self, hasLetters, hasNumber, hasPunc) -> str:
        length = self.Length
        randint = self.Pure_Random_Ints
        minimum = self.minimum
        maximum = self.maximum
        if length < 12:
            return "Invalid Length. It must be greater than 12"

        if randint is None:
            rand = secrets.SystemRandom(10)
            randoms = [rand.randrange(minimum, maximum) for _ in range(144)]
        else:
            randoms = self.Pure_Random_Ints

        random_num = ''.join(str(x) for x in random.sample(randoms, k=min(length, len(randoms))))
        if hasLetters and hasNumber and hasPunc:
            result = self.GeneratePass()
            return result

        if hasLetters != True and hasNumber and hasPunc:
            alpha_char = random_num + string.punctuation
        elif hasLetters and hasNumber != True and hasPunc:
            alpha_char = string.ascii_letters + string.punctuation
        elif hasLetters and hasNumber and hasPunc != True:
            alpha_char = string.ascii_letters + random_num
        else:
            return "Invalid request!"

        password = "".join(secrets.choice(alpha_char) for _ in range(self.Length))
        return password

    @staticmethod
    def _randomNumGen(num: int, minimum: int, maximum: int) -> list[int] | None:
        rand = secrets.SystemRandom()
        data = []
        for _ in range(num):
            data.append(rand.randrange(minimum, maximum +1))
        return data

    def encryptAndStoredata(self, user_name = "Unknown", Password = "", SecureNote = "Nothing", Category ="Unknown", favourite = False):
        site_name = self.site_name
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
            isAuth = self.IsAuthenticated()
            ic(isAuth)
            if isAuth == False:
                return isAuth
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
            return isAuth


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
        cursor.execute("SELECT * FROM Credential_Data where Id != 0")
        raw_data = cursor.fetchall()
        
        for item in raw_data:
            temp = {"Site": None, "Username": None, "Password": None, "Notes": None,"Strength": None, "Category": None, "Favourite": None,"Created_at": None, "Updated_at": None}
            temp["Site"] = self.decryptdata(item[1], item[2], item[3])
            temp["Username"] = self.decryptdata(item[1], item[2], item[4])
            temp["Password"] = self.decryptdata(item[1], item[2], item[5])
            temp["Notes"] = self.decryptdata(item[1], item[2], item[6])
            temp["Strength"] = item[8]
            temp["Favourite"] = True if item[9] == 1 else False
            temp["Category"] = item[7]
            temp["Created_at"] = item[10]
            temp["Updated_at"] = item[11]
            
            decrypted_list.append(temp)
        connection.close()
        return decrypted_list

    def decryptdata(self, salt, iteration,plaintext):
        if isinstance(salt, str):
            salt = binascii.unhexlify(salt)

        dKey = self._derived_key(salt, iteration)
        key = Fernet(dKey)
        decoded_text = key.decrypt(plaintext).decode()
        return decoded_text

    def IsAuthenticated(self):
        Password = self.password
        path = self.path

        if Password is None or not os.path.exists(path):
            return False

        connection = sqlite3.connect(path)
        cursor = connection.cursor()

        row = cursor.execute("""
            SELECT Password, Salt, Iteration
            FROM Credential_Data
            WHERE Id = 0
        """).fetchone()

        connection.close()

        if not row:
            return False

        enc_pass = row[0]  # KEEP AS BYTES
        salt = binascii.unhexlify(row[1])
        iteration = int(row[2])

        try:
            decoded_pass = self.decryptdata(
                salt,
                iteration,
                enc_pass
            )
            return Password == decoded_pass

        except Exception as e:
            print("Authentication Error:", e)
            return False

    def change_password(self, id = None, new_Password = ""):
        path = self.path
        if not os.path.exists(path) or os.path.getsize(path) < 50:
            return "Invalid Path"

        if id is None:
            return "Please enter a Id"

        with open(path) as file:
            file_data = json.load(file)
            salt = base64.urlsafe_b64decode(file_data["Salt"])
            for detail in file_data["Credentials"]:
                if id == detail["Id"]:
                    vault = base64.urlsafe_b64decode(detail["Vault"])
                    iteration = detail["iteration"]
                    key, _ = self._derived_key(salt=salt, iteration = iteration)
                    if key is None:
                        return "Invalid Password"
                    f = Fernet(key=key)

                    try:
                        decrypt_data = f.decrypt(vault).decode()
                        data = json.loads(decrypt_data)
                        data["Password"] = new_Password
                        data["updated_at"] = dt.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
                        # use the Fernet instance 'f' to encrypt the updated data
                        enc_data = self._EncJson(f, data['Site Name'], data['Secure Note'], data['Password'])
                        detail["Vault"] = base64.urlsafe_b64encode(enc_data).decode()
                        # write the full file_data back to disk to avoid corruption
                        with open(path, "w", encoding="utf-8") as fl:
                            json.dump(file_data, fl, indent=10)
                        return f"Password Changed Successfully"
                    except Exception as e:
                        return e
            return "Invalid Id"
        
    def isNewUser(self):
        path = self.path
        if os.path.exists(path) == False:
            return True
        return False

    def status(self):
        data = self.show_all_data()
        count = 0
        strong = 0
        weak = 0
        breached = 0
        for i in data:
            count += 1
            if i["Strength"] == self.TestResult[0]:
                strong += 1
            elif i["Strength"] == self.TestResult[1]:
                weak += 1
            elif i["Strength"] == self.TestResult[80]:
                breached += 1
        
        return (count, strong, weak, breached)

    def favourite_card_data(self):
        connection = sqlite3.connect(self.path)
        cursor = connection.cursor()
        cursor.execute("SELECT * FROM Credential_Data where Id != 0 AND Favourite == 1")
        raw_data = cursor.fetchall()
        data = []
        for item in raw_data:
            fav = self.decryptdata(item[1], item[2], item[3])
            data.append(fav)
        connection.close()
        return data
        

if __name__ == "__main__":
    pw_1 = PasswordManager("Netflix", "@@Adol2280@@",  32)
    # ic(pw_1.IsAuthenticated())
    # Status = pw_1.encryptAndStoredata("Cosmic78melon", pw_1.GeneratePass(),
    #                                   "Important Message", "edu", True)
    # print(Status)
    # print(pw_1.status())
    # print(pw_1.favourite_card_data())
    ic(pw_1.show_all_data())




