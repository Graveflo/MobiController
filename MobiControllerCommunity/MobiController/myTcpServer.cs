using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;

using System.Collections.Concurrent;

using ModServer;
using MobiControllerBlackBox.Controllers;

namespace MobiController
{
    public class myTcpServer : TCPserver
    {

        public delegate void ConnectionInit(MyClientContainer loggedEvent);
        public static event ConnectionInit Connected;
        public delegate void ConnectionDisconnect(MyClientContainer loggedEvent);
        public static event ConnectionDisconnect Disconnected;
        public delegate void ConnectionSocketException(SocketException ex);
        public static event ConnectionSocketException SocketException;


        private static HashSet<string> blocked = new HashSet<string>();
        public static HashSet<string> Blocked { set { blocked = value; } get { return blocked; } }

        public myTcpServer(int port)
            : base(port)
        {
            
        }

        protected override void ListenForClients()
        {
            try
            {
                base.ListenForClients();

            }
            catch (SocketException ex)
            {
                if (SocketException != null)
                    SocketException(ex);
            }
        }

        protected override void HandleClientComm(TcpClient client, IProtocolMatcher[] protocols)
        {
            if (protocols == null)
            {
                return;
            }
            //client.Client.RemoteEndPoint
            base.HandleClientComm(client, protocols);
            HttpProtocolMatcher pm = (HttpProtocolMatcher)protocols[0]; //get current running protocol HTTP
            myHttpEngine engine = (myHttpEngine)pm.Engine; // get the engine from it
            Disconnect(engine.Client); // get the client from the engine
        }

        protected override IProtocolMatcher[] generateProtocolEngines(TcpClient client)
        {
            string endpoint = client.Client.RemoteEndPoint.ToString();
            string ip = endpoint.Substring(0, endpoint.IndexOf(':'));
            foreach (string addr in blocked)
            {
                if (ip.Equals(addr))
                {
                    Disconnect(client);
                    return null;
                }
            }
            MyClientContainer Client = new MyClientContainer(client);

            Client.SessionVariables = new SessionInfo() { IsAuthenticated = false };

            if (Connected != null)
                Connected(Client);

            return new IProtocolMatcher[]{ 
                new HttpProtocolMatcher(){ Engine = new myHttpEngine(client.GetStream(),Client)}
            };
        }

        //protected override void proccessHttp(HttpEngine http, String message)
        //{
        //    App.Log.logEvent(message, Event.EVENT_FLAGS.NORMAL | Event.EVENT_FLAGS.DEBUG);
        //    base.proccessHttp(http, message);
        //}

        public virtual void Disconnect(MyClientContainer client)
        {
            //
            if (Disconnected != null)
                Disconnected(client);
            base.Disconnect(client.getClient());
        }
        public static void sDisconnect(MyClientContainer client)
        {
            if (Disconnected != null)
                Disconnected(client);
        }

        public static void tempBanIP(string ip)
        {
            try
            {
                myTcpServer.Blocked.Add(ip);
                var cls = new List<MyClientContainer>();
                foreach (MyClientContainer thisClient in App.ClientView.mylist)
                {
                    string thisendpoint = thisClient.getClient().Client.RemoteEndPoint.ToString();
                    string thisip = thisendpoint.Substring(0, thisendpoint.IndexOf(':'));
                    if (thisip.Equals(ip))
                    {
                        cls.Add(thisClient);
                    }
                }
                foreach (MyClientContainer thisClient in cls)
                {
                    myTcpServer.sDisconnect(thisClient);
                }
            }
            catch (ObjectDisposedException) { }
            try
            {
                App.MainWin.KickIP(ip);
            }
            catch (ObjectDisposedException) { }
        }

        public static void tempBanAgent(string agent)
        {
            try
            {
                MyClientContainer.banAgents.Add(agent.Trim());
                var cls = new List<MyClientContainer>();
                foreach (MyClientContainer thisClient in App.ClientView.mylist)
                {
                    if (thisClient.Agent.Equals(agent))
                    {
                        cls.Add(thisClient);
                    }
                }
                foreach (MyClientContainer thisClient in cls)
                {
                    myTcpServer.sDisconnect(thisClient);
                }
            }
            catch (ObjectDisposedException) { }
        }
    }
}
