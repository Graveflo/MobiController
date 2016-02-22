using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiController
{
    public static class HttpAuth
    {
        // uses the username as the encryption key for the password in the cookie
        // Do a action query event
        private static Dictionary<String, MyClientContainer> cookieTable = new Dictionary<String, MyClientContainer>();
        private static Dictionary<String, MyClientContainer> Sessions = new Dictionary<String, MyClientContainer>();
       // private static HashSet<String> yummy = new HashSet<String>();

        private static HashSet<string> Kix = new HashSet<string>();

        public static void allowClient(MyClientContainer client)
        {
            client.SessionVariables.IsAuthenticated = true;
        }
        public static void allowClient(MyClientContainer client, string username)
        {
            client.SessionVariables.Auth = username;
            allowClient(client);
        }

        public static void checkInClient(MyClientContainer client, String cookieValue)
        {
            string cookie = cookieValue.Trim();
            client.SessionVariables.Cookie = cookieValue;
            cookieTable.Add(cookie, client); //handle permissions with enum member of ClientContainer
        }

        //Check client username is the right seed for password cookie
        public static bool setSession(MyClientContainer client)
        {
            try
            {
                MyClientContainer matchingClient = cookieTable[client.Cookies[myHttpEngine.SESSIONID_COOKIE_PASSWORD].Trim()];
                client.SessionVariables = matchingClient.SessionVariables; //Syncs the two sessions
                //client.Auth = ""; // MyClientContainer needs this value for the GUI
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        public static bool hasKey(String key)
        {
            return cookieTable.ContainsKey(key);
        }

        public static bool isServerMaster(MyClientContainer client)
        {
            return true;
        }
    }
}
