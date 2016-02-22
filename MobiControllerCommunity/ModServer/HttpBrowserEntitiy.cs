using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace ModServer
{
    public class HttpBrowserEntitiy
    {

        public enum PORTS : int { HTTP = 80, SSL = 443 };

        private Dictionary<String, String> cookies;
        public Dictionary<String, String> SessionCookies
        {
            get
            {
                return cookies;
            }
            set
            {
                cookies = value;
            }
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/system.net.security.sslstream%28v=vs.110%29.aspx
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private static bool ValidateServerCertificate(
      object sender,
      X509Certificate certificate,
      X509Chain chain,
      SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // Do not allow this client to communicate with unauthenticated servers. 
            return false;
        }

        public HttpBrowserEntitiy()
        {
            cookies = new Dictionary<string, string>();
        }

        // in the fure HttpConnections can have some form of callback or events that will register with the entity
        public HttpConnection openSecureConnection(string address, int port, string servername)
        {
            TcpClient client = new TcpClient(address, port);
            SslStream sslStream = new SslStream(
                client.GetStream(),
                false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                null
                );
            // The server name must match the name on the server certificate. 
            sslStream.AuthenticateAsClient(servername); // may throw AuthenticationException
            HttpConnection thisConnection = new HttpConnection(address, port, servername, sslStream);

            return thisConnection;
        }

        public HttpConnection openSecureConnection(string address, PORTS port, string servername)
        { return openSecureConnection(address, (int)port, servername); }


        public HttpConnection openConnection(string address, int port)
        {
            TcpClient client = new TcpClient();
            client.Connect(address, port);

            HttpConnection thisConnection = new HttpConnection(address, port, address, client.GetStream());

            return thisConnection;
        }
        public HttpConnection openConnection(string address, PORTS port)
        {
            return openConnection(address, (int)port);
        }

        public void processResponse(HttpResponse r)
        {
            foreach (KeyValuePair<string, string> kvp in r.cookies)
            {
                if (cookies.ContainsKey(kvp.Key))
                {
                    cookies.Remove(kvp.Key);
                }
                cookies.Add(kvp.Key, kvp.Value);
            }
        }

    }
}
