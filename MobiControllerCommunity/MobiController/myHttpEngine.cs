using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Resources;
using System.Reflection;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Tools;
using ModServer;
using MobiControllerBlackBox.Controllers;
using WinAPIWrapper;
using ToolBox;

namespace MobiController
{
    interface html
    {

        bool writeToStream(Stream stream);
    }

    public class myHttpEngine : HttpEngine
    {
        public static string SESSIONID_COOKIE_PASSWORD = "SES";
        public static string SESSIONID_COOKIE_USERNAME = "NME";


        private static char REMOTE_TABLE_DELIMETER = '~';
        private static readonly string HTML_USERNAME_TEXT_ID = "txtusr";
        private static readonly string HTML_PASSWORD_TEXT_ID = "txtpassword";
        public static readonly int UPNP_TEST_NUMBER = new Random(1000).Next();

        private static HashSet<string> setAllAccessFiles = new HashSet<String> { "/", "/base64.js", "/jsbn.js", "/prng4.js", "/rng.js", "/rsa.js", "/aes.js", "/", "/auth/7.jpg", "favicon.ico", "/favicon.ico", "/auth/auth.htm", "/m/auth/index.html", "/m/jquery.mobile-1.3.2.min.css", "/m/darktheme.min.css", "/m/jquery.mobile-1.3.2.min.js", "/jquery-1.9.1.min.js" };
        private static HashSet<string> setAllAccessDirectories = new HashSet<string> { "/m/", "/m/auth/", "/m/images/" };

        private new MyClientContainer client; //superclass override CONTAINS SESSIONINFO!!
        public MyClientContainer Client
        {
            get
            {
                return client;
            }
        }
        private bool needsUpdate;
        private bool isFirst;

        public myHttpEngine(Stream networkStream, MyClientContainer client)
            : base(networkStream, client.getClient())
        {
            //Only rewrite clientcontainer vlues if it is recognized that they are missing.
            //se then on / request
            //this.client = new ClientContainer(client);
            this.client = client;
            needsUpdate = true;
            isFirst = true;

            //addPageMutation("{RSAKEY}", BitConverter.ToString(Bouncer.rcsp.ExportParameters(false).Modulus).Replace("-", ""));
            //addPageMutation("{RSAEX}", BitConverter.ToString(Bouncer.rcsp.ExportParameters(false).Exponent, 0).ToString().Replace("-", ""));
            //feedRemoteListHtml();
        }

        public override void process(string[] strmessage, byte[] message)
        {
            App.Log.logEvent(strmessage, Event.EVENT_FLAGS.NORMAL | Event.EVENT_FLAGS.DEBUG);
            client.CurrentRAW = message;
            base.process(strmessage, message);
        }

        protected override bool processHead(HttpRequest request)
        {
            if (request.Path.Equals("/UPnP"))
            {
                if (request.headers.ContainsKey("upnptestnum") && (Convert.ToInt32(request.headers["upnptestnum"])== UPNP_TEST_NUMBER))
                {
                    try
                    {
                        App.MainWin.settings.finishUPnP();
                        myTcpServer.sDisconnect(client);
                    }
                    catch (NullReferenceException) { }
                }
            }
            return base.processHead(request);
        }

        protected override bool processGet(HttpRequest request)
        {
            HttpResponse response;
            String pathDirectory = request.Path.Substring(0, request.Path.LastIndexOf('/') + 1);

            client.CurrentRequest = request;

            if (needsUpdate)
            {
                procedureUpdate(request);
            }

            // assures that the request cannot be completed unless the user is authenticated or the resource was made public
            if (client.SessionVariables.IsAuthenticated | (setAllAccessDirectories.Contains(pathDirectory) && !request.Path.Equals("/controllers")) | setAllAccessFiles.Contains(request.Path))
            {
                response = new HttpResponse(HttpResponse.ConnectionStatus.OK, "keep-alive", null);
                //
                if (isFirst)
                {
                    procedureAddAuth(response);
                    isFirst = false;
                }

                response.addHeader("Content-Type", request.getGuessFileType());

                // do path check by filetype
                string[] ext = request.Path.Split('.');
                if (ext.Length > 1)
                {
                    string extention = ext[ext.Length - 1].Replace("/", ""); //get rid of trailing slash if any
                    Assembly a = Assembly.GetExecutingAssembly();
                    Stream resStream = a.GetManifestResourceStream("MobiController.Content" + request.Path.Replace('/', '.'));

                    switch (extention)
                    {
                        case "htm":
                        case "html":
                            if (resStream != null)
                            {
                                writeHtmlStream(resStream, response);
                            }
                            else
                            {
                                if (!base.processGet(request))
                                {
                                    on404(null);
                                    return false;
                                }
                            }
                            break;
                        case "ermt":
                        case "rmt":
                            // if the controller is already loaded the request is executed
                            string thisRequestControllerName = ext[0].Substring(pathDirectory.Length);
                            if (client.SessionVariables.LoadedController != null &&
                                client.SessionVariables.LoadedController.ControllerName.Equals(thisRequestControllerName))
                            {
                                servePageFromString(client.SessionVariables.LoadedController.ControllerHtml, HttpResponse.ConnectionStatus.OK);
                            }
                            else // or else the controller is not loaded
                            {
                                try
                                {
                                    if (extention.EndsWith("ermt"))
                                    {
                                        client.SessionVariables.LoadedController = new Controller(new FileInfo(App.APP_DATA_FOLDER + request.Path), App.Config.username);
                                    }
                                    else
                                    {
                                        client.SessionVariables.LoadedController = new Controller(new FileInfo(App.APP_DATA_FOLDER + request.Path));
                                    }
                                    servePageFromString(client.SessionVariables.LoadedController.ControllerHtml, HttpResponse.ConnectionStatus.OK);
                                }
                                catch (ControllerException c)
                                {
                                    response = new HttpResponse(HttpResponse.ConnectionStatus.IM_A_TEAPOT, "keep-alive", null);
                                    response.Body = c.Reason.ToString();
                                    response.guessContentLength();
                                    App.Log.logEvent("The remote file requested could not be loaded: " + request.Path + "\r\n reason:" + c.Reason + " \r\n" + c.StackTrace, Event.EVENT_FLAGS.IMPORTANT);
                                    procedureSendResponse(response);
                                }//catch()
                            }
                            break;
                        default:
                            if (resStream != null)
                            {
                                writeStream(resStream, response);
                            }
                            else
                            {
                                if (!base.processGet(request))
                                {
                                    on404(null);
                                    return false;
                                }
                            }
                            break;
                    }

                }
                else // no filetype
                {

                    switch (request.Path)
                    {
                        case "/controllers":
                            response.addHeader("Content-Type", "text/html");
                            response.addHeader("Transfer-Encoding", "chunked");
                            try
                            {
                                SocketWriteLine(response.ToString());
                                feedRemoteListHtml();
                            }
                            catch (IOException ex)
                            {
                                App.Log.logEvent("IOException serving page to : " + client.ToString() + "\r\n Stack:" + ex.StackTrace, Event.EVENT_FLAGS.IMPORTANT | Event.EVENT_FLAGS.CRITICAL);
                            }
                            break;
                        case "/":
                            response.Status = HttpResponse.ConnectionStatus.FOUND;
                            response.addHeader("Content-Length", "0");
                            response.addHeader("Location", onInit(request));
                            procedureSendResponse(response);
                            break;
                        default: // Controllers are responsible for sending their own http responses.
                            try
                            {
                                String trimmedpath;
                                try
                                {
                                    trimmedpath = request.Path.Substring(0, request.Path.IndexOf('/', 1));
                                }
                                catch (ArgumentException)
                                {
                                    trimmedpath = request.Path;
                                }
                                response = client.SessionVariables.LoadedController.tryExec(trimmedpath, HelperClass.mergeDictionaries(request.postValues, request.getValues), client);
                                if (response != null)
                                {
                                    procedureSendResponse(response);
                                }
                            }
                            catch (ControllerException) { }
                            // likely a key not found exception on tool chain
                            catch (ToolInvokeException) { }
                            catch (NullReferenceException)
                            {
                                // TODO: no controller loaded. Redirect 
                            }
                            break;
                    }

                }

                if (response != null)
                {
                    App.Log.logEvent(response.ToString(), Event.EVENT_FLAGS.DEBUG);
                }
                return true;
            }
            else //not authenticated or accessable
            {
                response = new HttpResponse(HttpResponse.ConnectionStatus.FOUND, "keep-alive", null);
                response.addHeader("Content-Length", "0");
                response.addHeader("Location", onInit(request));
                //procedureAddAuth(response);
                try
                {
                    SocketWriteLine(response.ToString());
                }
                catch (IOException ex)
                {
                    procedureLogWriteOutException(ex.StackTrace);

                }
                needsUpdate = true;
                return false;
            }
        }

        private void procedureSendResponse(HttpResponse r)
        {
            try
            {
                SocketWriteLine(r.ToString());
            }
            catch (IOException ex)
            {
                procedureLogWriteOutException(ex.StackTrace);
            }
        }

        private void procedureLogWriteOutException(string stacktrace)
        {
            App.Log.logEvent("IOException serving page to : " + client.ToString() + "\r\n Stack:" + stacktrace, Event.EVENT_FLAGS.IMPORTANT | Event.EVENT_FLAGS.CRITICAL);

        }

        private void servePageFromString(String html, HttpResponse.ConnectionStatus status)
        {
            HttpResponse response = new HttpResponse(status, "keep-alive", null);
            response.addHeader("Content-Type", "text/html");
            response.addHeader("Content-Length", html.Length.ToString());
            App.Log.logEvent(response.ToString(), Event.EVENT_FLAGS.DEBUG);
            try
            {
                SocketWriteLine(response.ToString());
                SocketWriteLine(html);
            }
            catch (IOException ex)
            {
                App.Log.logEvent("IOException serving page to : " + client.ToString() + "\r\n Stack:" + ex.StackTrace, Event.EVENT_FLAGS.IMPORTANT | Event.EVENT_FLAGS.CRITICAL);
            }
        }

        public void feedRemoteListHtml()
        {
            //comments are for debugging
            //FileStream filestream;
            ////StreamWriter s = File.CreateText("txt.txt");

            String[] splithtml = MobiController.Properties.Resources.remotelist.Split(REMOTE_TABLE_DELIMETER);
            DirectoryInfo controllerDir;
            if (Directory.Exists(App.CONTROLLER_DIR))
            {
                controllerDir = new DirectoryInfo(App.CONTROLLER_DIR);
            }
            else
            {
                controllerDir = Directory.CreateDirectory(App.CONTROLLER_DIR);
            }

            //FileStream rs = File.Create(Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\MobiController") + "\\asdf.rmt");
            //rs.WriteByte(2);
            //rs.Write(Encoding.UTF8.GetBytes("This is a test string"), 0, Encoding.UTF8.GetByteCount("This is a test string"));
            //rs.WriteByte(3);
            //rs.Close();
            ////s.WriteLine(splithtml[0].Length.ToString("X"));
            ////s.WriteLine(splithtml[0]);
            SocketWriteLine(splithtml[0].Length.ToString("X"));
            SocketWriteLine(splithtml[0]);
            String tableEntry;
            foreach (ControllerInfo controller in ControllerInfo.getInstalledControllers(App.CONTROLLER_DIR))
            {
                string[] meta = Controller.getControllerMeta(controller.Path);
                //rel="external" so that jquery will reload the header of the remote and re-init the DOM for multipage browsing!!!!
                tableEntry = "<li ><a rel=\"external\" href=\"/controllers/" + controller.Path.Name + "\"> <h3>" + meta[0] + "</h3><p>" + meta[1] + "</p></a></li>";
                SocketWriteLine(tableEntry.Length.ToString("X"));
                SocketWriteLine(tableEntry);
            }
            tableEntry = "<li ><a rel=\"external\" href=\"http://mobicontroller.com/controllers/\"> <h3>Get More Controllers</h3></a></li>";
            SocketWriteLine(tableEntry.Length.ToString("X"));
            SocketWriteLine(tableEntry);
            SocketWriteLine(splithtml[1].Length.ToString("X"));
            ////s.WriteLine(splithtml[1].Length.ToString("X"));
            ////s.WriteLine(splithtml[1]);
            SocketWriteLine(splithtml[1]);
            SocketWriteLine("0\r\n");
            stream.Flush();
            //s.Close();
        }

        protected override void processPost(HttpRequest request)
        {
            bool cancelResponse = false;
            HttpResponse response = new HttpResponse(HttpResponse.ConnectionStatus.NOT_IMPLEMENTED, "keep-alive", null);

            if (needsUpdate)
            {
                procedureUpdate(request);
            }

            client.CurrentRequest = request;

            switch (request.Path)
            {
                //serialize the fields for a command in the remote file. (for example send message to window)
                //work by form name with dictionary of names and values
                // code modular output first. then modular input

                case "/m/auth/localAuth":
                    procedureAuthenticateLocally();
                    cancelResponse = true;
                    break;
                case "/auth":
                    response = new HttpResponse(HttpResponse.ConnectionStatus.FOUND, "keep-alive", null);
                    response.addHeader("Content-Length", "0");
                    procedureAuthenticate(request, response);
                    break;
                case "/m/message":
                    if (client.SessionVariables.IsAuthenticated)
                    {
                        response = processMessage(request);
                    }
                    break;
                default:
                    response.addHeader("Content-Length", "0");
                    if (client.SessionVariables.IsAuthenticated == false)
                        procedureUpdate(request);
                    if (client.SessionVariables.IsAuthenticated)
                    {
                        if (client.SessionVariables.LoadedController != null)
                        {
                            try
                            {
                                HttpResponse tmp = client.SessionVariables.LoadedController.tryExec(request.Path, HelperClass.mergeDictionaries(request.postValues, request.getValues), client);
                                if (tmp != null)
                                {
                                    response = tmp;
                                }
                            }
                            catch (ToolInvokeException) { }
                        }
                        //Should refer to a call to the clients action tree.
                        //action tree can have serialized members
                    }
                    else
                    {
                        needsUpdate = true;
                    }
                    break;
            }

            if (!cancelResponse)
            {
                App.Log.logEvent(response.ToString(), Event.EVENT_FLAGS.DEBUG);
                try
                {
                    SocketWriteLine(response.ToString());
                }
                catch (IOException ex)
                {
                    App.Log.logEvent("IOException returning post : " + request.Path + " to : " + client.ToString() + "\r\n Stack:" + ex.StackTrace, Event.EVENT_FLAGS.IMPORTANT | Event.EVENT_FLAGS.CRITICAL);
                }
            }
        }

        private HttpResponse processMessage(HttpRequest r)
        {
            var vals = HelperClass.mergeDictionaries(r.postValues, r.getValues);
            string message;
            if (vals.ContainsKey("m"))
            {
                message = vals["m"];
            }
            else
            {
                message = r.MessageBody;
            }
            if (vals.ContainsKey("t"))
            {
                if (vals["t"] == "messagebox")
                {
                    App.MainWin.Dispatcher.Invoke(() => new MessageBox(message, "Message from: " + client.EndPoint).Show());
                    return new HttpResponse(HttpResponse.ConnectionStatus.OK, "keep-alive", null);
                }
            }
            App.Log.logEvent(message, Event.EVENT_FLAGS.NOLOG | Event.EVENT_FLAGS.IMPORTANT);
            return new HttpResponse(HttpResponse.ConnectionStatus.OK, "keep-alive", null);
        }

        private void procedureAuthenticate(HttpRequest request, HttpResponse response)
        {
            if (request.postValues.ContainsKey(HTML_USERNAME_TEXT_ID) && request.postValues.ContainsKey(HTML_PASSWORD_TEXT_ID))
            {
                string username = request.postValues[HTML_USERNAME_TEXT_ID];
                if (Bouncer.validateCredentials(username, System.Net.WebUtility.UrlDecode(request.postValues[HTML_PASSWORD_TEXT_ID])))
                {
                    HttpAuth.allowClient(client, username);
                    response.addHeader("Location", "/controllers");
                    return;
                }
            }
            response = new HttpResponse(HttpResponse.ConnectionStatus.FORBIDDEN, "keep-alive", null);
            response.addHeader("Content-Length", "0");
        }

        private void procedureAddAuth(HttpResponse response)
        {
            String pass;
            do
            {
                pass = Convert.ToBase64String(Encoding.ASCII.GetBytes(Path.GetRandomFileName())).Trim();
            } while (HttpAuth.hasKey(pass));

            HttpAuth.checkInClient(client, pass);
            response.setCookie(SESSIONID_COOKIE_PASSWORD, pass + "; Path=/");
            //needsUpdate = true;
        }

        public void procedureAuthenticateLocally()
        {
            if (client.SessionVariables.IsAuthenticated)
            {
                return;
            }
            //string endpoint = "";
            string ip = client.IP;// endpoint.Substring(0, endpoint.IndexOf(':'));
            if (myTcpServer.Blocked.Contains(ip))
            {
                return; // should soon die. This is the clientveiw list being destroyed before the clienttbale in tcpserver
            }
            Action acceptClient = () =>
            {
                HttpAuth.allowClient(client, "Default");
                HttpResponse response = new HttpResponse(HttpResponse.ConnectionStatus.FOUND, "keep-alive", null);
                response.addHeader("Content-Length", "0");
                response.addHeader("Location", "/controllers");
                App.Log.logEvent(response.ToString(), Event.EVENT_FLAGS.DEBUG);
                try
                {
                    SocketWriteLine(response.ToString());
                }
                catch (IOException ex)
                {
                    App.Log.logEvent("IOException serving page to : " + client.ToString() + "\r\n Stack:" + ex.StackTrace, Event.EVENT_FLAGS.IMPORTANT | Event.EVENT_FLAGS.CRITICAL);
                }
                
            };
            App.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    PasswordBox pass = new System.Windows.Controls.PasswordBox() { FlowDirection = FlowDirection.LeftToRight, Foreground = Brushes.White, CaretBrush = Brushes.White, Margin = new Thickness(10, 0, 0, 0), Height = 20, Width = 200 };
                    TextBlock lblpass = new System.Windows.Controls.TextBlock() { FlowDirection = System.Windows.FlowDirection.LeftToRight, TextWrapping = System.Windows.TextWrapping.NoWrap, HorizontalAlignment = System.Windows.HorizontalAlignment.Left, Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, Style = (Style)App.MainWin.FindResource("HeadingWhiteShadowBold"), Text = "Please enter your password then hit enter to ALLOW:", Foreground = Brushes.Gold };
                    MessageBox thisMessageBox = new MessageBox("A device: " + client.EndPoint + " is trying to authenticate. Give permission to access Controllers?", "Local Authentication");

                    pass.KeyDown += delegate(object sender, System.Windows.Input.KeyEventArgs e)
                    {
                        if (e.Key == System.Windows.Input.Key.Enter)
                        {
                            byte[] passtry = new System.Security.Cryptography.SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(pass.Password));
                            if (App.Config.password.SequenceEqual(passtry))
                            {
                                acceptClient.Invoke();
                                thisMessageBox.Close();
                            }
                            else
                            {
                                lblpass.Foreground = Brushes.Red;
                                lblpass.Text = "Incorrect password. Please try again.";
                            }
                            pass.Clear();
                        }
                    };
                    //lblpass.KeyDown += Allow_Try;

                    thisMessageBox.addButton("Deny Once", Deny_Click);
                    thisMessageBox.addButton("Block Device", delegate()
                    {
                        myTcpServer.tempBanIP(client.IP);
                    });
                    if (App.Config.username == null || App.Config.username == "")
                    {
                        thisMessageBox.addButton("Accept", acceptClient, true);
                        lblpass.Text = "Log in to password protect this prompt.";
                    }
                    else
                    {
                        thisMessageBox.ButtonPannel.Children.Add(pass);
                    }
                    thisMessageBox.ButtonPannel.Children.Add(lblpass);
                    thisMessageBox.Width = 720;
                    thisMessageBox.Show();
                    pass.Focus();


                    //MessageBox thisMessageBox = new MessageBox();
                    //thisMessageBox.addButton("Deny", Deny_Click);
                    //thisMessageBox.addButton("Allow", Allow_Click);
                    //thisMessageBox.Show();
                    //thisMessageBox.Focus();
                    //thisMessageBox.bringForward();
                }));
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //(old)
            //if (System.Windows.MessageBox.Show("A device: " + client.IP + " is trying to authenticate. Give permission to access Controllers?","Local Authentication",System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            //{
            //    HttpAuth.authenticateClient(client);
            //}
            //else
            //{
            //    client.SessionVariables.IsAuthenticated = false;
            //}
        }

        //public void Allow_Click()
        //{
        //    HttpAuth.authenticateClient(client);
        //    HttpResponse response = new HttpResponse(HttpResponse.ConnectionStatus.FOUND, "keep-alive", null);
        //    response.addHeader("Content-Length", "0");
        //    response.addHeader("Location", "/controllers");
        //    App.Log.logEvent(response.ToString(), Event.EVENT_FLAGS.DEBUG);
        //    try
        //    {
        //        SocketWriteLine(response.ToString());
        //    }
        //    catch (IOException ex)
        //    {
        //        App.Log.logEvent("IOException serving page to : " + client.ToString() + "\r\n Stack:" + ex.StackTrace, Event.EVENT_FLAGS.IMPORTANT | Event.EVENT_FLAGS.CRITICAL);
        //    }

        //}
        public void Deny_Click()
        {
            client.SessionVariables.IsAuthenticated = false;
            HttpResponse response = new HttpResponse(HttpResponse.ConnectionStatus.FORBIDDEN, "keep-alive", null);
            response.addHeader("Content-Length", "0");
            App.Log.logEvent(response.ToString(), Event.EVENT_FLAGS.DEBUG);
            try
            {
                SocketWriteLine(response.ToString());
            }
            catch (IOException ex)
            {
                App.Log.logEvent("IOException serving page to : " + client.ToString() + "\r\n Stack:" + ex.StackTrace, Event.EVENT_FLAGS.IMPORTANT | Event.EVENT_FLAGS.CRITICAL);
            }
        }
        //callback to GUI?
        //handle user name and password system

        protected override void on404(HttpRequest request)
        {
            HttpResponse response = new HttpResponse(HttpResponse.ConnectionStatus.FILE_NOT_FOUND, "keep-alive", null);
            App.Log.logEvent(response.ToString(), Event.EVENT_FLAGS.DEBUG);
            servePageFromString(MobiController.Properties.Resources.html404, HttpResponse.ConnectionStatus.FILE_NOT_FOUND);
        }

        protected override String onInit(HttpRequest request)
        {
            if (client.isMobile)
            {
                if (client.SessionVariables.IsAuthenticated)
                {
                    return "/controllers";
                }
                //response.addHeader("Location", "/m/auth/index.html");
                return "/m/auth/index.html";
            }
            else
            {
                if (client.SessionVariables.IsAuthenticated)
                {
                    return "/controllers";
                }
                //response.addHeader("Location", "/auth/index.htm");
                return "/m/auth/index.html";
            }
        }

        private void procedureUpdate(HttpRequest request)
        {
            client.Cookies = request.cookies;
            try
            {
                client.parseLanguage(request.requestMetaInfo("Accept-Language"));
                client.parseOS(request.requestMetaInfo("User-Agent"));
            }
            catch (KeyNotFoundException)
            {
            }

            if (HttpAuth.setSession(client) == false)
                needsUpdate = false;

            //    if (RefreshClient != null)
            //        RefreshClient(client);
        }
    }
}