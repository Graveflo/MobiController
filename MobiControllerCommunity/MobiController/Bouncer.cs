using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using MobiControllerBlackBox;

namespace MobiController
{
    //private Dictionary<String,String> //account registry

    internal static class Bouncer
    {
        private static Dictionary<String, String> userTable = new Dictionary<String, String>(); // encrypt the password with the username as the seed! IMPORTANT!!!
        public static RSACryptoServiceProvider rcsp = new RSACryptoServiceProvider(2048);

        public static Dictionary<String,String> UserTable{
            set
            {
                userTable = value;
            }
        }

        public static bool IsUserTableNull
        {
            get
            {
                return (userTable == null);
            }
        }

        public static String[] UserNames
        {
            get
            {
                return userTable.Keys.ToArray();
            }
        }
        public static int AccountCount
        {
            get
            {
                return userTable.Count;
            }
        }

        //private static RSACryptoServiceProvider serializeRSA()
        //{
        //    DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
        //    RSACryptoServiceProvider returnme = new RSACryptoServiceProvider();
        //    XmlSerializer xml = new XmlSerializer(typeof(RSAParameters));
        //    DES.Key = gk();
        //    DES.IV = ASCIIEncoding.ASCII.GetBytes("francinq");
        //    MemoryStream secret = new MemoryStream(MobiController.Properties.Resources.mysecret);
        //    CryptoStream cryptostream = new CryptoStream(secret, DES.CreateDecryptor(), CryptoStreamMode.Read);
        //    returnme.ImportParameters((RSAParameters)xml.Deserialize(cryptostream));
        //    return returnme;
        //}


        public static bool addAccount(String username, String password)
        {
            if (userTable.ContainsKey(username))
            {
                return false;
            }
            else
            {
                userTable.Add(username, BlackBox.generateUserHash(Encoding.UTF8.GetBytes(username), Encoding.UTF8.GetBytes(password)));
                App.serialize(userTable);
                return true;
            }
        }

        public static bool removeAccount(String username)
        {
            bool a = userTable.Remove(username);
            App.serialize(userTable);
            return a;
        }

        public static bool removeAccount(String username, String password)
        {
            if(validateCredentials(username,password))
            {
                return removeAccount(username);
            }
            else
            {
                return false;
            }
        }

        public static bool validateCredentials(String username, String password64base)
        {
            byte[] pwd;
            try
            {
                pwd = rcsp.Decrypt(Convert.FromBase64String(password64base), false);
            }
            catch (CryptographicException)
            {
                //USER MUST REFRESH
                return false;
            }

            SHA256CryptoServiceProvider hasher = new SHA256CryptoServiceProvider();
            // it's a mess but it works
            if (username.Equals(username) && Encoding.UTF8.GetString(App.Config.password).Trim().Equals(Encoding.UTF8.GetString(hasher.ComputeHash(pwd)).Trim()))
            {
                return true;
            }

            if (userTable.ContainsKey(username) == false)
            {
                return false; //TERMINATING CONDITION
            }

            byte[] usr = Encoding.UTF8.GetBytes(username);
            //the password comes in encoded in base64. This will get the password encrypted with thepublic RSA key
            //byte[] pwd = Convert.FromBase64String(password64base);
            String hash = BlackBox.generateUserHash(usr, pwd);

            //it has been established that the username key exists. Now see if the hashes match
            return userTable[username].Equals(hash);
        }
    }
}
