using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Net.Sockets;

namespace ModServer
{
    [Serializable()]
    public class ConnectionException : Exception
    {
        public ConnectionException() : base() { }
        public ConnectionException(string message) : base(message) { }
        public ConnectionException(string message, Exception inner) : base(message,inner) { }
    }
    /// <summary>
    /// This class is uses Microsoft's implementation of SSL. Most of the code is copied 
    /// from: http://msdn.microsoft.com/en-us/library/system.net.security.sslstream%28v=vs.110%29.aspx
    /// </summary>
    public class HttpConnection
    {
        private string address;
        public string Address
        {
            get
            {
                return address;
            }
            set
            {
                address = value;
            }
        }

        private int port = 80;

        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        private string servername;

        private Dictionary<String, String> cookies;

        public Dictionary<String, String> Cookies
        {
            get { return cookies; }
            set { cookies = value; }
        }

        public static string CHUNKED_TERMINATOR = "0" + Environment.NewLine + Environment.NewLine;

        public Stream stream;
        public delegate void AwaitMessageCallback(HttpResponse r);

        public HttpConnection(string address, int port, string servername, Stream stream)
        {
            this.address = address;
            this.port = port;
            this.servername = servername;
            this.stream = stream;

            cookies = new Dictionary<string, string>();
        }

        public HttpResponse sendRequest(HttpRequest request)
        {
            request.addHeader("Host", servername);
            string thisstring = request.ToString();
            string sendme = request.ToString();
            byte[] outBytes = Encoding.UTF8.GetBytes(sendme);
            stream.Write(outBytes, 0, outBytes.Length);
            stream.Flush();

            return awaitMessage();
        }

        public Thread sendRequestAsync(HttpRequest request, AwaitMessageCallback callBack)
        {
            request.addHeader("Host", servername);

            string thisstring = request.ToString();
            string sendme = request.ToString();
            byte[] outBytes = Encoding.UTF8.GetBytes(sendme);

            //try catch here with a fail callback and timeout timer
            stream.Write(outBytes, 0, outBytes.Length);
            stream.Flush();

            Thread listenThread;
            listenThread = new Thread(() => awaitMessageAsync(callBack));
            listenThread.Start();
            return listenThread;
        }

        public HttpResponse awaitMessage()
        {
            byte[] buffer = new byte[4048];
            string message;
            int bytes = 0;

            HttpResponse response = null;

            bool Cont = true;
            bool ResponseReceived = false;

            do
            {
                int read = stream.Read(buffer, 0, buffer.Length);
                if (read == 0)
                {
                    Cont = false;
                }
                message = Encoding.UTF8.GetString(buffer, 0, read);
                bytes += read;
                byte[] tmp = new byte[bytes];
                try
                {
                    for (int i = 0; i < response.MessageRAW.Length; i++)
                    {
                        tmp[i] = response.MessageRAW[i];
                    }
                }
                catch (NullReferenceException)
                {

                }
                int j = 0;
                for (int i = bytes - read; i < bytes; i++)
                {
                    tmp[i] = buffer[j];
                    j++;
                }

                if (!ResponseReceived)
                {
                    response = new HttpResponse(message.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
                    message = "";
                }
                response.MessageRAW = tmp;

                if (response.headers.ContainsKey("transfer-encoding") && response.headers["transfer-encoding"].Equals("chunked"))
                {
                    if (message.Contains(CHUNKED_TERMINATOR))
                    {
                        message = message.Remove(message.IndexOf(CHUNKED_TERMINATOR));
                        Cont = false;
                    }
                    ResponseReceived = true;
                }
                else
                {
                    ResponseReceived = response.Status != 0;
                    if (ResponseReceived)
                    {
                        if (read == buffer.Length)
                        {
                            if (!message.EndsWith(Environment.NewLine))
                            {
                                Cont = false;
                            }
                        }
                        else
                        {
                            Cont = false;
                        }
                    }
                }
                if (ResponseReceived)
                {
                    response.Body += message;
                }
            } while (Cont);

            // may want to check if return code is > 200 for clearing referer
            return processResponse(response);
        }

        public bool continueDownload(Stream output, HttpResponse response, Action<int> setup, Action<int> step)
        {
            if ((int)response.Status < 300)
            {
                try
                {
                    if (!response.headers.ContainsKey("content-length"))
                    {
                        throw new ConnectionException("Content Length needed");
                    }
                    int contentLen = Convert.ToInt32(response.headers["content-length"]);
                    int filepartlen = response.MessageRAW.Length - response.HeadLength;
                    int restLen = contentLen - filepartlen;

                    setup(contentLen);

                    output.Write(response.MessageRAW, response.HeadLength, filepartlen);
                    step(filepartlen);

                    byte[] rest = new byte[1048576]; // 1mb of buffer space
                    int read = 0;
                    while (1 < restLen)
                    {
                        read = stream.Read(rest, 0, rest.Length);
                        output.Write(rest, 0, read);
                        restLen -= read;
                        step(read);
                        output.Flush();
                    }
                }
                catch (FormatException)
                {
                    return false;
                }
                return true;
            }
            else
            {
                throw new ConnectionException("Download failed.");
            }
        }

        private void awaitMessageAsync(AwaitMessageCallback callBack)
        {
            callBack(awaitMessage());
        }

        private HttpResponse processResponse(HttpResponse response)
        {
            foreach (KeyValuePair<String, String> kvp in response.cookies)
            {
                if (cookies.ContainsKey(kvp.Key))
                    cookies.Remove(kvp.Key);
                cookies.Add(kvp.Key, kvp.Value);
            }
            return response;
        }
    }
}
