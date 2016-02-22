using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml;

namespace MobiController
{
    [Serializable()]
        public class SerializableConfig
    {
        public static int VERSION = 3;
        public int version = VERSION;

        public string ServerName = "MyMobiControllerServer";
        public int MaxConnections = 1; //Max number of connections
        public string ipAddress = "";  //used to detect a change in IP
        public bool isAutoListen = true;
        public bool isAutoStart = true;
        public bool isStartInTray = true;
        public bool isHideDonate = false;
        public bool isUPnPonStart = false;
        public bool isShowMessageOnMinimize = true;
        public int SERVER_PORT = 80;
        public int UPnPPort = 6349;
        public string username = "";
        public byte[] password;
        public Dictionary<string, DateTime> updateTimeStamps = new Dictionary<string, DateTime>();
        public Dictionary<string, string> bannedIPs = new Dictionary<string, string>();
        public Dictionary<string, string> bannedMACs = new Dictionary<string, string>();

        public void convert(SerializableConfig config)
        {
            if (config.ServerName != null)
                ServerName = config.ServerName;
            if (config.MaxConnections != null)
                MaxConnections = config.MaxConnections;
            if (config.ipAddress != null)
                ipAddress = config.ipAddress;
            if (config.isAutoListen != null)
                isAutoListen = config.isAutoListen;
            if (config.isStartInTray != null)
                isStartInTray = config.isStartInTray;
            if (config.isHideDonate != null)
                isHideDonate = config.isHideDonate;
            if (config.isUPnPonStart != null)
                isUPnPonStart = config.isUPnPonStart;
            if (config.isShowMessageOnMinimize != null)
                isShowMessageOnMinimize = config.isShowMessageOnMinimize;
            if (config.SERVER_PORT != null)
                SERVER_PORT = config.SERVER_PORT;
            if (config.username != null)
                username = config.username;
            if (config.password != null)
                password = config.password;
            if (config.updateTimeStamps != null)
                updateTimeStamps = config.updateTimeStamps;
            if (config.bannedIPs != null)
                bannedIPs = config.bannedIPs;
            if (config.bannedMACs != null)
                bannedMACs = config.bannedMACs;
        }
    }
}
