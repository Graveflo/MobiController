using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

using System.Collections.Concurrent;

using ModServer;
using MobiControllerBlackBox.Controllers;

namespace MobiController
{
    public class MyClientContainer : ClientContainer
    {
        private SessionInfo sessionInfo;
        private System.Net.Sockets.TcpClient client;

        public MyClientContainer(TcpClient client, String language = "UNKNOWN", String os = "UNKNOWN") : base(client, language, os) {  }
        public static HashSet<string> banAgents = new HashSet<string>();
        //private static bool isBanAgentsLock = false;

        //private static List<Action> afterBanAgentIterate = new List<Action>();

        //public MyClientContainer(TcpClient client) : base(client) { banAgents = new ConcurrentBag<string>(); }

        public override bool parseOS(string os)
        {
            if (os != null && banAgents.Contains(os.Trim()))
            {
                myTcpServer.sDisconnect(this);
                System.Threading.Thread.CurrentThread.Abort();
                return false;
            }
            return base.parseOS(os);
        }

        //public static void banAgent(string agent)
        //{
        //    if (isBanAgentsLock)
        //    {
        //        afterBanAgentIterate.Add(() => banAgent(agent));
        //        return;
        //    }
        //    banAgents.Add(agent);
        //}
        //public static void unBanAgent(string agent)
        //{
        //    if (isBanAgentsLock)
        //    {
        //        afterBanAgentIterate.Add(() => unBanAgent(agent));
        //        return;
        //    }
        //    banAgents.Remove(agent);
        //}

        public SessionInfo SessionVariables
        {
            get
            {
                return sessionInfo;
            }
            set
            {
                sessionInfo = value;
                NotifyPropertyChanged("Auth");
            }
        }
        public String ImageURI
        {
            get
            {
                if (isMobile)
                    return "/MobiController;component/Resources/mobile.ico";
                else
                {
                    if (Agent.Equals("UNKNOWN") || Agent == null)
                    {
                        return "Null";
                    }
                    else
                    {
                        return "/MobiController;component/Resources/monitor.ico";
                    }
                }
            }
        }

        //private string auth;
        public String Auth
        {
            set
            {
                sessionInfo.Auth = value;
                NotifyPropertyChanged("Auth");
            }
            get
            {
                if(sessionInfo!=null){
                    return sessionInfo.Auth;
                }else{
                    return "None";
                }
            }
        }

    }
}
