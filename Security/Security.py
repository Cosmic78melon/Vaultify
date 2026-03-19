# Importing Important Libraries
import os 
import re
import json
import uuid
import base64
import random 
import string 
import secrets
import requests 
from dotenv import load_dotenv 
import pwnedpasswords as pwend
from cryptography.fernet import Fernet
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.kdf.pbkdf2 import PBKDF2HMAC

load_dotenv() 

class PasswordManager:
    def __init__(self,
                 site_name: str = "Unknown",
                 password: str = None,
                 shouldGeneratePass: bool = False,
                 Password_Length: int = 12):

        self.password = password 
        self.site_name = site_name
        self.Length = Password_Length
        self.shouldGeneratePass = shouldGeneratePass 
        self.TestResult = {0:"Strong", 1:"Weak" , 2: "Error", -1: "No Password", 80:"Breached", "Cause": {"Breached": None, "hasUppercase": None, "hasLowercase": None, 
                                                                                                  "hasDigits": None, "hasPunc": None, "isLong": None, "Errors": None}}

        # This is are Constants that are not supposed to control by users
        self.min = 0
        self.max = 9999
        self.iteration = 299990
        self.Pure_Random_Ints = self._randomNumGen(10,self.min, self.max)

        base_dir = os.path.dirname(os.path.abspath(__file__))
        project_root = os.path.abspath(os.path.join(base_dir, os.pardir))
        self.path = os.path.join(project_root, "DataBase", "encrypted-data.json")
        
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
        if self.shouldGeneratePass and self.Length < 12:
            return "Invalid Length. It must be greater than 12"

        if self.Pure_Random_Ints is None:
            rand = secrets.SystemRandom(10)
            randoms = [rand.randrange(self.min, self.max) for _ in range(10)]
        else:
            randoms = self.Pure_Random_Ints

        random_num = str(secrets.choice(randoms))
        alpha_char = string.ascii_letters + random_num + string.punctuation
        run = True
        while run:
            password = "".join(secrets.choice(alpha_char) for _ in range(self.Length))
            result = self.Check_Password(password)
            if result == self.TestResult[0]:
                run = False
                return password
        return None

    def Custom_GeneratePass(self, hasLetters, hasNumber, hasPunc) -> str | None:
        if self.shouldGeneratePass:
            if self.Length < 12:
                return "Invalid Length. It must be greater than 12"

            if self.Pure_Random_Ints is None:
                rand = secrets.SystemRandom(10) 
                randoms = [rand.randrange(self.min, self.max) for _ in range(10)]
            else:
                randoms = self.Pure_Random_Ints
            
            random_num = ''.join(str(x) for x in random.sample(randoms, k=min(self.Length, len(randoms))))
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
        return None

    @staticmethod
    def _randomNumGen(num: int, minimum: int, maximum: int) -> list[int] | None:
        """Generates Random numbers purely because the random numbers are generated by the atmospheric noise
            Even if the atmospheric the noise api don't work it will still give pure noise because than it will generate
            number because it will generate number by looking the system noise which is also purely random

            Status: ✔Complete
        """
        try:
            keys = os.getenv("API_KEY")
        except Exception:#type: ignore
            rand = secrets.SystemRandom()
            data = []
            for _ in range(num):
                data.append(rand.randrange(minimum, maximum +1))
            return data

        try:
            url = "https://api.random.org/json-rpc/2/invoke"
            payload = { "jsonrpc": "2.0", "method": "generateIntegers", "params": { "apiKey": keys, "n": num, "min": min, "max": max, "replacement": True }, "id": 1}

            response = requests.post(url, json=payload, timeout=22)
            response.raise_for_status()

            data = response.json()

            if response.status_code == 200 and "error" not in data:
                return data["result"]["random"]["data"]
            else:
                rand = secrets.SystemRandom()
                data = [rand.randrange(minimum, maximum +1) for _ in range(num)]
                return data
        except Exception:
            rand = secrets.SystemRandom()
            data = []
            for _ in range(num):
                data.append(rand.randrange(minimum, maximum +1))
            return data

    def encryptAndStoredata(self, SecureNote = "Nothing", Password = ""):
        if self.password is None:
            return self.TestResult[-1]

        if Password is None:
            return self.TestResult[-1]

        if not os.path.exists(self.path) or os.path.getsize(self.path) == 0:
            with open(self.path , "w") as file:
                json.dump({"Salt": None,"Credentials": []}, file,indent=10)
        
        with open(self.path) as file:
            file_data = json.load(file)
            
            if file_data["Salt"] is None:
                file_data["Salt"] = base64.urlsafe_b64encode(os.urandom(16)).decode()

            salt = base64.urlsafe_b64decode(file_data["Salt"])
            kdf_derived_key, kdf = self._derived_key(salt, iteration = self.iteration)
            key = Fernet(kdf_derived_key)
            
            Id = str(uuid.uuid4())
            credentials = self._EncJson(key, self.site_name,SecureNote, Password)
            
            temp_kdf_type = str(type(kdf))
            kdf_type = re.findall( r'\w+|[^\s\w]+', temp_kdf_type)[-2]
            New_entry = {
                "Id": Id,   
                "KDF": str(kdf_type),
                "iteration": self.iteration,
                "Vault": base64.urlsafe_b64encode(credentials).decode()
            }
                            
            file_data["Credentials"].append(New_entry)
            file.seek(0)
        with open(self.path, "w", encoding="utf-8") as f:
            json.dump(file_data, f, indent=10)
            
        return "Password and Secure Note saved", Id


    def _EncJson(self, fernet, site_name, message, Password = None):
        """
        Encrypt a JSON payload using a provided Fernet instance.
        """
        if Password is None:
            raise ValueError("Invalid Password")
        # Expect a Fernet instance with an .encrypt method
        try:
            from cryptography.fernet import Fernet as _FernetType
            is_fernet = isinstance(fernet, _FernetType)
        except Exception:
            is_fernet = hasattr(fernet, "encrypt")

        if not is_fernet:
            raise ValueError("Invalid Fernet instance")

        vault_data = {
            "Site Name": site_name,
            "Secure Note": str(message),
            "Password": Password
        }
        temp_json = json.dumps(vault_data).encode()
        encrypted_json = fernet.encrypt(temp_json)

        return encrypted_json

    def _derived_key(self, salt, iteration):
        if self.password is None:
            return "Invalid Password", None

        if salt is None:
            return "Invalid Salt", None
        
        kdf = PBKDF2HMAC(
            hashes.SHA256(),
            32,
            salt = salt,
            iterations=iteration
        )
        return base64.urlsafe_b64encode(kdf.derive(self.password.encode())), kdf
        
    
    def decryptAndStoredata(self, id = None):
        if self.password is None:
            return self.TestResult[-1], None
        
        if not os.path.exists(self.path) or os.path.getsize(self.path) < 50:
            return "Invalid Path", None

        if id is None:
            return "Please enter a Id", None

        with open(self.path) as file:
            file_data = json.load(file)
            
            salt = base64.urlsafe_b64decode(file_data["Salt"])
            for detail in file_data["Credentials"]:
                if id == detail["Id"]:
                    vault = base64.urlsafe_b64decode(detail["Vault"])
                    iteration = detail["iteration"]
                    key, _ = self._derived_key(salt=salt, iteration = iteration)
                    f = Fernet(key=key)

                    try:
                        decrypt_data = f.decrypt(vault).decode()
                        return detail, decrypt_data
                    except Exception:
                        return "Decryption failed: wrong password or corrupted data", None
            return "Invalid Id", None

    def show_all_data(self):
        decrypted_data = []
        with open(self.path) as file:
            json_data = json.load(file)
            salt = base64.urlsafe_b64decode(json_data["Salt"])
            for detail in json_data["Credentials"]:
                vault = base64.urlsafe_b64decode(detail["Vault"])
                iteration = detail["iteration"]
                key, _ = self._derived_key(salt=salt, iteration = iteration)
                f = Fernet(key=key)
                try:
                    decrypt_data = f.decrypt(vault).decode()
                    data = json.loads(decrypt_data)
                    decrypted_data.append(data)
                except Exception:
                    return [{"success": False, "error": "Decryption failed"}]
        return decrypted_data

    def check_password(self):
        if self.password is None:
            return False

        with open(self.path) as file:
            json_data = json.load(file)
            salt = base64.urlsafe_b64decode(json_data["Salt"])
            for detail in json_data["Credentials"]:
                vault = base64.urlsafe_b64decode(detail["Vault"])
                iteration = detail["iteration"]
                key, _ = self._derived_key(salt=salt, iteration=iteration)
                f = Fernet(key=key)
                try:
                    f.decrypt(vault).decode()
                except Exception:
                    return False
        return True


    def change_password(self, id = None, new_Password = ""):
        if not os.path.exists(self.path) or os.path.getsize(self.path) < 50:
            return "Invalid Path"

        if id is None:
            return "Please enter a Id"

        with open(self.path) as file:
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
                        # use the Fernet instance 'f' to encrypt the updated data
                        enc_data = self._EncJson(f, data['Site Name'], data['Secure Note'], data['Password'])
                        detail["Vault"] = base64.urlsafe_b64encode(enc_data).decode()
                        # write the full file_data back to disk to avoid corruption
                        with open(self.path, "w", encoding="utf-8") as fl:
                            json.dump(file_data, fl, indent=10)
                        return f"Password Changed Successfully"
                    except Exception as e:
                        return e
            return "Invalid Id"

if __name__ == "__main__":
    pw_1 = PasswordManager("Netflix", "Your Password", False, 32)
    # Status, ID = pw_1.encryptAndStoredata("Important security 😤 Message", "adol33454")
    # print(Status)
    # print(pw_1.change_password("d737a2d9-9478-4cbc-800b-aa8a43fae07b", "%^ado54"))
    # details, vault = pw_1.decryptAndStoredata("Id")
    # print(pw_1.show_all_data())
    # print("This is the updated vault "+vault)
    print(pw_1.check_password())
