using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Xml;
using System.IO;

using ModServer;

namespace MobiController
{

    public class UPnP
    {
        public const string AUTHOR = "Harold Aptroot, Netherlands";
        static TimeSpan _timeout = new TimeSpan(0, 0, 0, 3);
        public static TimeSpan TimeOut
        {
            get { return _timeout; }
            set { _timeout = value; }
        }
        static string _descUrl, _serviceUrl, _eventUrl, serviceType;
        public static bool Discover()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            string req = "M-SEARCH * HTTP/1.1\r\n" +
            "HOST: 239.255.255.250:1900\r\n" +
            "ST:upnp:rootdevice\r\n" +
            "MAN:\"ssdp:discover\"\r\n" +
            "MX:3\r\n\r\n";
            byte[] data = Encoding.ASCII.GetBytes(req);
            IPEndPoint ipe = new IPEndPoint(IPAddress.Broadcast, 1900);
            byte[] buffer = new byte[0x1000];

            DateTime start = DateTime.Now;

            do
            {
                s.SendTo(data, ipe);
                s.SendTo(data, ipe);
                s.SendTo(data, ipe);

                int length = 0;
                do
                {
                    length = s.Receive(buffer);

                    string resp = Encoding.ASCII.GetString(buffer, 0, length);
                    if (resp.Contains("upnp:rootdevice"))
                    {
                        string tmp = resp.ToLower();
                        resp = resp.Substring(tmp.IndexOf("location:") + 9);
                        resp = resp.Substring(0, resp.IndexOf("\r")).Trim();
                        if (!string.IsNullOrEmpty(_serviceUrl = GetServiceUrl(resp)))
                        {
                            _descUrl = resp;
                            return true;
                        }
                    }
                } while (length > 0);
            } while (start.Subtract(DateTime.Now) < _timeout);
            return false;
        }

        private static string GetServiceUrl(string resp)
        {
#if !DEBUG
            try
            {
#endif
            XmlDocument desc = new XmlDocument();
            desc.Load(WebRequest.Create(resp).GetResponse().GetResponseStream());
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(desc.NameTable);
            nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
            XmlNode typen = desc.SelectSingleNode("//tns:device/tns:deviceType/text()", nsMgr);
            if (!typen.Value.Contains("InternetGatewayDevice"))
                return null;
            XmlNode node = desc.SelectSingleNode("//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANIPConnection:1\"]/tns:serviceType/text()", nsMgr);
            if (node == null)
                return null;
            serviceType = node.Value;
            node = desc.SelectSingleNode("//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANCommonInterfaceConfig:1\"]/tns:controlURL/text()", nsMgr);  //WANCommonInterfaceConfig
            if (node == null) // WANCommonInterfaceConfig
                return null;
            XmlNode eventnode = desc.SelectSingleNode("//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANIPConnection:1\"]/tns:eventSubURL/text()", nsMgr);
            _eventUrl = CombineUrls(resp, eventnode.Value);
            return CombineUrls(resp, node.Value);
#if !DEBUG
            }
            catch { return null; }
#endif
        }

        private static string CombineUrls(string resp, string p)
        {
            int n = resp.IndexOf("://");
            n = resp.IndexOf('/', n + 3);
            return resp.Substring(0, n) + p;
        }


        public static void addPortMapping(int externalPort, int internalPort,
                              String internalClient, String protocol, String description)
        {
            Dictionary<String, String> args = new Dictionary<String, String>();
            args.Add("NewRemoteHost", "");    // wildcard, any remote host matches
            args.Add("NewExternalPort", externalPort.ToString());
            args.Add("NewProtocol", protocol);
            args.Add("NewInternalPort", internalPort.ToString());
            args.Add("NewInternalClient", internalClient);
            args.Add("NewEnabled", "True");
            args.Add("NewPortMappingDescription", description);
            args.Add("NewLeaseDuration", "1000");

            simpleUPnPcommand(_serviceUrl, "AddPortMapping", args);
        }


        public static String getExternalIPAddress()
        {
            XmlDocument xdoc = simpleUPnPcommand(_serviceUrl, "GetExternalIPAddress", null);
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xdoc.NameTable);
            nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
            string IP = xdoc.SelectSingleNode("//NewExternalIPAddress/text()", nsMgr).Value;
            return IP;
        }

        public static void deletePortMapping(int externalPort, String protocol)
        {
            Dictionary<String, String> args = new Dictionary<String, String>();
            args.Add("NewRemoteHost", "");
            args.Add("NewExternalPort", externalPort.ToString());
            args.Add("NewProtocol", protocol);
            simpleUPnPcommand(_serviceUrl, "DeletePortMapping", args);
        }

        public static XmlDocument simpleUPnPcommand(String url, String action, Dictionary<String, String> args)
        {
            serviceType = "urn:schemas-upnp-org:service:WANIPConnection:1";
            String soapAction = "\"" + serviceType + "#" + action + "\"";
            StringBuilder soapBody = new StringBuilder();

            soapBody.Append("<?xml version=\"1.0\"?>\r\n" +
                    "<SOAP-ENV:Envelope " +
                    "xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
                    "SOAP-ENV:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
                    "<SOAP-ENV:Body>" +
                    "<m:" + action + " xmlns:m=\"" + serviceType + "\">");

            if (args != null && args.Count > 0)
            {
                foreach (KeyValuePair<string, string> entry in args)
                {
                    soapBody.Append("<" + entry.Key + ">" + entry.Value +
                            "</" + entry.Key + ">");
                }
            }

            soapBody.Append("</m:" + action + ">");
            soapBody.Append("</SOAP-ENV:Body></SOAP-ENV:Envelope>");

            HttpWebRequest r =(HttpWebRequest)WebRequest.Create(url);

            r.Method = "POST";
            r.ContentType = "text/xml";
            r.Headers.Add("SOAPAction", soapAction);
            //r.ConnectionGroupName =
            //r.
            //r.Connection = "Close";
            //r.Headers.Add("Connection","Close");

            byte[] soapBodyBytes = Encoding.UTF8.GetBytes(soapBody.ToString());

            r.ContentLength = soapBody.Length;

            r.GetRequestStream().Write(soapBodyBytes, 0, soapBodyBytes.Length);

            WebResponse wres = r.GetResponse();
            Stream ress = wres.GetResponseStream();
            XmlDocument resp = new XmlDocument();
            resp.Load(ress);
            return resp;
        }
    }
}
