# Importing Important Libraries
import os 
import secrets 
import math
import requests 
import random, string 
from dotenv import load_dotenv 
import pwnedpasswords as pwend
load_dotenv() 

class PasswordManager: 
    def __init__(self, site_name: str = "Unknown", password = None, shouldGeneratePass: bool = False, Password_Length: int = 12): 
        self.password = password 
        self.site_name = site_name 
        self.Length = Password_Length
        self.shouldGeneratePass = shouldGeneratePass 
        self.TestResult = {0:"Strong", 1:"Weak" , 2: "Error", -1: "No Password", 80:"Breached", "Cause": {"Breached": False, "hasUppercase": True, "hasLowercase": True, 
                                                                                                          "hasDigits": True, "hasPunc": True, "isLong": True}}
        self.Pure_Random_Ints = self._randomNumGen(700,0, 9999)
        
    def Check_Password(self, Password = None):
        """
        This function Checks is the password is strong or not by looking at how many characters does this have enough letters or char ect.
        and also this function checks is the password brached or not so we can ensure full safety of the password
        
        Status: ✔Complete 
        """
        if Password == None:
            Password = self.password
            
        if not self.password:
            return self.TestResult[-1]
        
        self.TestResult["Cause"] = {
        "Breached": False,
        "hasUppercase": True,
        "hasLowercase": True,
        "hasDigits": True,
        "hasPunc": True,
        "isLong": True
    }


        weak = False
        has_lowercase_letters = any(i in string.ascii_lowercase for i in Password)
        has_uppercase_letters = any(i in string.ascii_uppercase for i in Password)
        has_digits = any(i in string.digits for i in Password)
        has_special_Character = any(i in string.punctuation for i in Password)

        if pwend.check(Password):
            self.TestResult["Cause"]["Breached"] = True
            return self.TestResult[80]

        if not has_uppercase_letters:
            self.TestResult["Cause"]["hasUppercase"] = False
            weak = True
        if not has_lowercase_letters:
            self.TestResult["Cause"]["hasLowercase"] = False
            weak = True
        if not has_digits:
            self.TestResult["Cause"]["hasDigits"] = False
            weak = True
        if not has_special_Character:
            self.TestResult["Cause"]["hasPunc"] = False
            weak = True
        if len(Password) < 12:
            self.TestResult["Cause"]["isLong"] = False
            weak = True
        
        if weak == True:
            print(weak)
            return self.TestResult[1]
        
        return self.TestResult[0]
        
    def GeneratePass(self): 
        if self.shouldGeneratePass == True:
            if (self.Length) < 12:
                return "Invalid Lenght. It must be greater than 12"
            
            random_num = str(random.sample(self.Pure_Random_Ints, self.Length))
            alpha_char = string.ascii_letters + random_num + string.punctuation 
            run = True
            while run:
                password = "".join(secrets.choice(alpha_char) for _ in range(self.Length))
                result = self.Check_Password(password)
                if result is self.TestResult[0]:
                    run = False
                    return password
                    
            
    def _randomNumGen(self, num: int, min: int, max: int) -> list: 
        """Generates Random numbers purely because the random numbers are genrated by the atmospheric noise 
            Even if the the atmospheric the noise api don't work it will still give pure noise because than it will generate 
            number beacause it will generate number by looking the system noise which is also purely random
            
            Status: ✔Complete
        """
        keys = os.getenv("API_KEY") 
        url = "https://api.random.org/json-rpc/2/invoke" 
        payload = { "jsonrpc": "2.0", "method": "generateIntegers", "params": { "apiKey": keys, "n": num, "min": min, "max": max, "replacement": True }, "id": 1 } 
        response = requests.post(url, json=payload)
        data = response.json() 
        
        if response.status_code == 200 and "error" not in data: 
            return data["result"]["random"]["data"]
        else: 
            rand = secrets.SystemRandom(num) 
            data = [rand.randrange(min, max) for _ in range(num)]
            return data 

