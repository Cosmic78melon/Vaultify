# Importing Important Libraries
import os 
import secrets 
import argparse
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
        self.TestResult = {0:"Strong\n", 1:"Weak\n" , 2: "Error\n", -1: "No Password\n", 80:"Breached\n", "Cause": {"Breached": None, "hasUppercase": None, "hasLowercase": None, 
                                                                                                          "hasDigits": None, "hasPunc": None, "isLong": None}}
        self.Pure_Random_Ints = self._randomNumGen(700,0, 9999)
        
    def Check_Password(self, Password = None):
        """
        This function Checks is the password is strong or not by looking at how many characters does this have enough letters or char ect.
        and also this function checks is the password brached or not so we can ensure full safety of the password
        
        Status: ✔Complete 
        """
        if Password == None:
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
            return self.TestResult[1]
        
        self.TestResult["Cause"] = {
        "Breached": False,
        "hasUppercase": True,
        "hasLowercase": True,
        "hasDigits": True,
        "hasPunc": True,
        "isLong": True
    }
        return self.TestResult[0]
        
    def GeneratePass(self): 
        if self.shouldGeneratePass == True:
            if (self.Length) < 12:
                return "Invalid Lenght. It must be greater than 12"
            
            random_num = str(secrets.choice(self.Pure_Random_Ints))
            alpha_char = string.ascii_letters + random_num + string.punctuation 
            run = True
            while run:
                password = "".join(secrets.choice(alpha_char) for _ in range(self.Length))
                result = self.Check_Password(password)
                if result == self.TestResult[0]:
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

# Command Line Utility for debugging purposes
def main():
    parser = argparse.ArgumentParser(description="how the password managers CLI work")
    parser.add_argument("-d", "--demo", action="store_true",help="show demo of all features")
    parser.add_argument("--generate", action="store_true", help="Generate new password")
    parser.add_argument("-l", "--length", type=int, default=12,help="password length when generating (default: 12)")
    parser.add_argument("-c", "--check",type=str,help="Check password strength password strenght", nargs=1)
    parser.add_argument("-s", "--site", type=str, default="Unknown", help="Name of the site", nargs="?")
    parser.add_argument("--version", action="version",version="Password Manager -> 1.1.0")
    
    args = parser.parse_args()
    if args.demo:
        print()
        print("-------------------------------------------------")
        print("|| Illustrating how the password manager works ||")
        print("-------------------------------------------------")
        print()
        print("Use --generate or --check")
        print("!!!Make sure you are using the latest Version!!!")
        print("Example:")
        print("  python password_manager.py --generate --length 20 --site netflix")
        print("  python password_manager.py --check 'MyP@ssw0rd123'\n")
        pw = PasswordManager("Password Manager CO.", "password123", True, 17)
        print(f"Site Name: {pw.site_name}")
        print(f"Password: {pw.password} and the length: {pw.Length}")
        print(f"Password Status: {pw.Check_Password()}")
        print("As we can see this password is not strong so what we can do 🤔??😋 We can use the built in password generator")
        print(f"Generated Password: {pw.GeneratePass()}")
        print()
    elif args.generate:
        pw = PasswordManager(args.site, shouldGeneratePass=True, Password_Length=args.length)
        print(f"{pw.site_name}'s password is {pw.GeneratePass()}")
        
    elif args.check:
        password_toCheck = "".join([_ for _ in args.check])
        pw = PasswordManager(args.site, password_toCheck, False, args.length)
        pw.Check_Password()
        if pw.TestResult["Cause"]["Breached"] == True:
            print(f"Password is {pw.Check_Password()}")
        else:
            print(f"Password is {pw.Check_Password()} Details: {pw.TestResult["Cause"]}")
    
    else:
        print()
        print("Use --generate or --check")
        print("!!!Make sure you are using the latest Version!!!")
        print("Example:")
        print("  python password_manager.py --generate --length 20 --site netflix")
        print("  python password_manager.py --check 'MyP@ssw0rd123'\n")
        print("-----------------------------------------------------------------------------------------------------")
        parser.print_help()
        print()
        
        
if __name__ == "__main__":
    main()