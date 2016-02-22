using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using System.Net.Sockets;
using System.Diagnostics;
using System.Timers;
using System.IO;
using System.Windows.Interop;
using System.Security;
using System.Security.Cryptography;
using System.Threading;

using ModServer;
using WinAPIWrapper;

namespace MobiController
{
    /// <summary>
    /// Interaction logic for frmMain.xaml
    /// </summary>
    public partial class frmMain : Window
    {
        public const string PHP_COOKIE = "PHPSESSID";
        public const string WEB_ROOT = "mobicontroller.com";
        public const string URL_REGISTER = "https://secure144.inmotionhosting.com/~r4msof5/accounts/register.php";
        public const string PHP_REFLECT_SCRIPT = "/~r4msof5/releases/MobiController/controllers/reflect.php";
        public const string PHP_LOGIN_SCRIPT = "/~r4msof5/accounts/login.php";

        public frmSettings settings;
        private bool iconOpacityFlipper = false;
        private bool Dieing;
        myTcpServer server;

        public System.Windows.Forms.NotifyIcon notifyIcon;

        System.Timers.Timer iconOpacityTimer;
        System.Windows.Threading.DispatcherTimer dispatcherTimer;
        System.Threading.Timer loginTimeout;
        Thread loginThread;

        public Action showTrayMessage;

        public IntPtr HWnd
        {
            get { return new WindowInteropHelper(this).Handle; }
        }

        private HttpBrowserEntitiy browser;
        public HttpBrowserEntitiy Browser
        {
            get
            {
                return browser;
            }
            set
            {
                browser = value;
            }
        }

        private bool isOpeningControllerBrowser;
        private frmControllerBrowser preloadBrowser;

        public frmMain()
        {
            browser = new HttpBrowserEntitiy();
            myTcpServer.SocketException += SocketException;
            iconOpacityFlipper = false;
            Dieing = false;
            isOpeningControllerBrowser = false;

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            System.Windows.Forms.ContextMenu trayMenu = new System.Windows.Forms.ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            notifyIcon.Icon = new System.Drawing.Icon(MobiController.Properties.Resources.favicon, 40, 40);
            notifyIcon.Visible = true;
            notifyIcon.ContextMenu = trayMenu;

            showTrayMessage = () =>
            {
                if (App.Config.isShowMessageOnMinimize)
                {
                    notifyIcon.BalloonTipClicked += baloonClick;
                    notifyIcon.ShowBalloonTip(2000, "", "You can open Mobicontroller by double clicking this icon. " + lblServerStatus.Text, System.Windows.Forms.ToolTipIcon.Info);
                }
            };

            InitializeComponent(); // all preceeding commands must stay before this!!!
            lblTime.Text = DateTime.Now.ToString("hh:mm tt");
            notifyIcon.DoubleClick += (sender, e) => { Show(); WinAPI.SetForegroundWindow(HWnd); };

            if (App.Config.username.Equals(""))
            {
                login();
            }
            else
            {
                procedureLoggedin();
            }

            try
            {
                var updateConnection = browser.openConnection(WEB_ROOT, HttpBrowserEntitiy.PORTS.HTTP);
                // the version.txt file on the server contains the current version of MobiController
                updateConnection.sendRequestAsync(new HttpRequest("/releases/MobiController/version.txt"), (response) =>
                {
                    string strVersion = response.Body.Trim();
                    if (!App.VERSION.Equals(strVersion))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var msg = new MessageBox("An update to MobiController is available. Would you like to download the the installer?", "Update");
                            msg.PrgBar.Visibility = System.Windows.Visibility.Visible;
                            msg.addButton("Download", () =>
                            {
                                Dispatcher.Invoke(() => msg.prgbar.IsIndeterminate = true);
                                var newConnection = Browser.openConnection(WEB_ROOT, HttpBrowserEntitiy.PORTS.HTTP);
                                newConnection.sendRequestAsync(new HttpRequest("/releases/MobiController/" + strVersion + "/MobiController_v" + strVersion + ".exe"), (dlResponse) =>
                                {
                                    try
                                    {
                                        using (var fs = File.Create(App.APP_DATA_FOLDER + "update.exe"))
                                        {
                                            newConnection.continueDownload(fs, dlResponse, (contentlen) =>
                                            {
                                                Dispatcher.Invoke(() =>
                                                {
                                                    Dispatcher.Invoke(() => msg.prgbar.IsIndeterminate = false);
                                                    msg.prgbar.Maximum = contentlen;
                                                });
                                            }, (inc) =>
                                            {
                                                Dispatcher.Invoke(() => msg.prgbar.Value += inc);
                                            });
                                        }
                                        Dispatcher.Invoke(() => msg.prgbar.IsIndeterminate = true);
                                        App.exec(App.APP_DATA_FOLDER + "update.exe");

                                        Dispatcher.Invoke(() => msg.prgbar.IsIndeterminate = false);
                                    }
                                    catch (System.ComponentModel.Win32Exception)
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            var errmsg = new MessageBox("The installer could not be started.");
                                            errmsg.addButton("Open Folder Location", () =>
                                            {
                                                App.exec("explorer.exe", '"' + App.APP_DATA_FOLDER+'"');
                                                //Process explorer = new Process();
                                                //explorer.StartInfo.FileName = "explorer.exe";
                                                //explorer.StartInfo.Arguments = '"' + App.APP_DATA_FOLDER;
                                                //explorer.Start();
                                            });
                                            errmsg.Show();
                                        });
                                    }
                                    catch (ConnectionException)
                                    {
                                        Dispatcher.Invoke(() => { new MessageBox("There was a problem with the server. Try again later or download it manually from the site.").Show(); });
                                    }
                                    Dispatcher.Invoke(() => msg.Close());
                                });
                            }, false);
                            msg.addButton("Go Online", () =>
                            {
                                //Process p = new Process();
                                //p.StartInfo.FileName = "http://" + WEB_ROOT;
                                //p.Start();
                                App.exec("http://" + WEB_ROOT);
                            });

                            msg.Show();
                        });
                    }
                });
            }
            catch (SocketException)
            {
            }
        }

        public void KickIP(string ip)
        {
            Thread kickthread = new Thread(() => server.DisconnectAll(ip));
            kickthread.Start();
        }


        void baloonClick(object sender, EventArgs e)
        {
            try
            {
                Show();
                WinAPI.SetForegroundWindow(HWnd);
                notifyIcon.BalloonTipClicked -= baloonClick;
            }
            catch (InvalidOperationException) { Close(); }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            if (App.Config.isAutoListen)
            {
                StartServer(true);
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            server.kill();
            if (settings != null && settings.startme != null)
            {
                settings.startme.Abort();
            }
            Dieing = true; //Works better in both places for some reason (right click notify uses this as click event)
            Application.Current.Shutdown();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (Dieing == false)
            {
                //notifyIcon.Visible = true;
                showTrayMessage.Invoke();
                Visibility = System.Windows.Visibility.Hidden;
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        //protected override void OnActivated(EventArgs e)
        //{
        //    notifyIcon.Visible = false;
        //    base.OnActivated(e);
        //}

        public void StartServer(bool showMessage)
        {
            server = new myTcpServer(App.Config.SERVER_PORT);

            try
            {
                server.start();
                string currentIP;
                if(!App.Config.ipAddress.Equals((currentIP=TCPserver.getLocalIP().ToString().Trim()))){
                    App.Log.logEvent("The IP address of this computer has changed!! Take note of the new IP (erase your bookmark and add): " + currentIP, Event.EVENT_FLAGS.IMPORTANT);
                    App.Config.ipAddress = currentIP;
                    App.serialize(App.Config);
                }
                this.lblAddress.Text = currentIP;

                if (App.Config.SERVER_PORT != 80)
                {
                    lblAddress.Text += ":" + App.Config.SERVER_PORT;
                }
                cmdStartServer.Visibility = Visibility.Hidden;
                cmdStopServer.Visibility = Visibility.Visible;
                lblServerStatus.Text = "The server is.. Started.";
                Event.EVENT_FLAGS flags = Event.EVENT_FLAGS.NORMAL;
                if (showMessage)
                {
                    flags |= Event.EVENT_FLAGS.IMPORTANT;
                }
                App.Log.logEvent("The server has been started.", flags);
            }
            catch (NetworkException ex)
            {
                App.Log.logEvent(ex.Message, Event.EVENT_FLAGS.IMPORTANT);
            }
            catch (SocketException exception)
            {
                App.Log.logEvent("critical error: " + exception.Message + "\r\n" + exception.StackTrace.ToString(), Event.EVENT_FLAGS.IMPORTANT);
            }
        }

        public void StopServer(bool showMessage)
        {
            try
            {
                server.kill();
            }
            catch (NullReferenceException)
            {
            }
            cmdStartServer.Visibility = Visibility.Visible;
            cmdStopServer.Visibility = Visibility.Hidden;
            lblServerStatus.Text = "The server is.. Stopped.";

            Event.EVENT_FLAGS flags = Event.EVENT_FLAGS.NORMAL;
            if (showMessage)
            {
                flags |= Event.EVENT_FLAGS.IMPORTANT;
            }
            App.Log.logEvent("The server has been killed.", flags);
        }

        private void cmdCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            App.exec(URL_REGISTER);
        }

        private void cmdStartServer_Click(object sender, RoutedEventArgs e)
        {
            StartServer(true);
        }

        private void cmdStopServer_Click(object sender, RoutedEventArgs e)
        {
            StopServer(true);
        }

        private void frmMain_Loaded(object sender, RoutedEventArgs e)
        {
            //TIMER
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(iconOpacityTimer_Elapsed);
            dispatcherTimer.Interval = new TimeSpan(500000);
            dispatcherTimer.Start();

            //var ControllerBrowser = new frmControllerBrowser();
            //if (ControllerBrowser.needsUpdate().Count > 0)
            //{
            //    var ThisMessage = new MessageBox("Controller updates are avialable. On the main window click 'Controllers' => 'Get Controllers', then press 'Update All' or click below.");
            //    ThisMessage.addButton("Get Controllers", () =>
            //    {
            //        openControllerBrowser(ControllerBrowser);
            //    });
            //    ThisMessage.Show();
            //}

        }

        void iconOpacityTimer_Elapsed(object sender, EventArgs e)
        {
            try
            {
                iconOpacityTimer_Elapsed(imgImportant);
                iconOpacityTimer_Elapsed(imgImportant1);
            }
            catch (Exception) { dispatcherTimer.Stop(); } //who cares
        }

        void iconOpacityTimer_Elapsed(Image currentImage)
        {
            if (iconOpacityFlipper)
            {
                currentImage.Opacity += .05;
                if (currentImage.Opacity >= 1)
                {
                    currentImage.Opacity -= .05;
                    iconOpacityFlipper = false;
                    //var Tim = System.TimeZone.CurrentTimeZone.ToUniversalTime();
                    lblTime.Text = DateTime.Now.ToString("h:mm tt");
                }
            }
            else
            {
                currentImage.Opacity -= .05;
                if (currentImage.Opacity < .05)
                {
                    currentImage.Opacity += .05;
                    iconOpacityFlipper = true;
                }
            }
        }
        void loginTimeout_Elapsed(object sender, ElapsedEventArgs e)
        {
            // kill thread
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            App.LogView.Show();
            App.LogView.Focus();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            App.ClientView.Show();
            App.LogView.Focus();
        }

        private void onlinehelp_Click(object sender, RoutedEventArgs e)
        {
            //Process p = new Process();
            //p.StartInfo.FileName = "http://mobicontroller.com/help.php/";
            //p.Start();
            App.exec("http://mobicontroller.com/help.php/");
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ExitProgram_Click(object sender, RoutedEventArgs e)
        {
            Dieing = true;
            OnExit(sender, e);
        }

        public void SocketException(SocketException ex)
        {
            Dispatcher.Invoke(new Action(() => handleSocketException(ex)));
        }

        public void handleSocketException(SocketException ex)
        {
            switch (ex.ErrorCode)
            {
                case 10013:
                    //TODO server not started
                    System.Windows.MessageBox.Show(this, "The network denied access to the server to start. Please check and make sure port " + App.Config.SERVER_PORT + " is not in use or blocked by your software.", "Error", MessageBoxButton.OK);
                    StopServer(false);
                    break;
                case 10048:
                    App.Log.logEvent("The server could not be started. The port: " + App.Config.SERVER_PORT + " may be in use by another program. You can choose a different port in the settings.", Event.EVENT_FLAGS.IMPORTANT);
                    StopServer(false);
                    break;
                default:
                    App.Log.logEvent("An unexpected error has occured. This is important please take note of the situation and let me know what happened. You may want to restar the server.", Event.EVENT_FLAGS.IMPORTANT);
                    break;
            }
        }

        private void showSettings(object sender, RoutedEventArgs e)
        {
            if (settings == null || settings.IsSeen)
            {
                settings = new frmSettings();
            }
            settings.Show();
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cmdCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(lblAddress.Text);
        }

        private void MenuItem_RemoveController_Click(object sender, RoutedEventArgs e)
        {
            //Go to remove controller veiw
            showSettings(sender, e);
            settings.Focus();
            settings.tabSettings.SelectedItem = settings.tabiControllers;
        }

        public void MenuItem_AddController_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".rmt"; // Default file extension
            dlg.Filter = "Controller Files (.rmt)|*.rmt"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                App.addController(dlg.FileName);
            }
            //go to open file dialouge
        }

        private void cmdDonate_Click(object sender, RoutedEventArgs e)
        {
            //Process p = new Process();
            //p.StartInfo.FileName = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=58XX6J6FBP6H8";
            //p.Start();
            App.exec("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=58XX6J6FBP6H8");
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                if (txtPassword.Password.Length < 1)
                {
                    lblPass.Foreground = Brushes.Red;
                    return;
                }
                lblPass.Foreground = Brushes.White;
                if (txtUsername.Text.Length < 1)
                {
                    lblUser.Foreground = Brushes.Red;
                    return;
                }
                lblUser.Foreground = Brushes.White;

                string hostname = "secure144.inmotionhosting.com";
                Browser = new HttpBrowserEntitiy();
                var connection = Browser.openSecureConnection(hostname, HttpBrowserEntitiy.PORTS.SSL, hostname);
                var request = new HttpRequest("POST", PHP_LOGIN_SCRIPT);

                txtUsername.IsEnabled = false;
                txtPassword.IsEnabled = false;

                request.addPost("username", txtUsername.Text);
                request.addPost("password", txtPassword.Password.ToString());
                request.addHeader("Content-Type", "application/x-www-form-urlencoded");
                loginThread = connection.sendRequestAsync(request, (HttpResponse r) =>
                {
                    if (r.headers.ContainsKey("location") && r.headers["location"].Contains("home.php"))
                    {
                        Browser.processResponse(r);
                        request = new HttpRequest("POST", PHP_REFLECT_SCRIPT);
                        request.cookies = Browser.SessionCookies;
                        var response = connection.sendRequest(request);
                        string[] bodyparts = response.Body.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        string name = bodyparts[1]; // should be the reflected user name (in case email address was used to log in)
                        
                        loginThread = null;
                        Dispatcher.Invoke(() =>
                        {
                            SHA256Managed hasher = new SHA256Managed();
                            byte[] password = Encoding.UTF8.GetBytes(txtPassword.Password.ToString());

                            AccountContent.Visibility = Visibility.Hidden;
                            Sheet.Visibility = Visibility.Hidden;
                            MainContentPane.Effect = null;

                            App.Config.username = name;
                            App.Config.password = hasher.ComputeHash(password);

                            txtUsername.IsEnabled = true;
                            txtPassword.IsEnabled = true;
                            txtPassword.Clear();

                            procedureLoggedin();

                            App.serialize(App.Config);

                        });
                        //Bouncer.addAccount(txtUsername.Text, txtPassword.Password);
                        //AccountContent.Visibility = Visibility.Hidden;
                        //Sheet.Visibility = Visibility.Hidden;
                        //MainContentPane.Effect = null;
                        //client authenticated
                    }
                    else
                    {
                        Dispatcher.Invoke(()=>{
                            lblLoginFail.Foreground = Brushes.Red;
                            lblLoginFail.Text = "Login failed.";
                            txtUsername.IsEnabled = true;
                            txtPassword.IsEnabled = true;
                            txtPassword.Focus();
                        });
                        // not authenticated
                    }
                });

                this.loginTimeout = new System.Threading.Timer((stateinfo) =>
                {
                    if (loginThread != null)
                    {
                        loginThread.Suspend();
                        //loginThread.Abort(); // handle exception first (IOException)
                        loginThread = null;
                        Dispatcher.Invoke(() =>
                        {
                            lblLoginFail.Foreground = Brushes.Red;
                            lblLoginFail.Text = "Login timed out.";
                            txtUsername.IsEnabled = true;
                            txtPassword.IsEnabled = true;
                            txtPassword.Focus();
                        });
                    }
                    loginTimeout.Dispose();
                }, null, 20000, 0);
                
            }
        }

        private void procedureLoggedin()
        {
            if (isOpeningControllerBrowser)
            {
                preloadBrowser.refreshPurchased();
                preloadBrowser.Show();
                isOpeningControllerBrowser = false;
            }
            txtLoginText.Text = "Logged in as : " + App.Config.username + ". Click to log out.";
            BitmapImage checkImage = new BitmapImage(new Uri("pack://application:,,,/MobiController;component/Resources/check.ico"));
            
            imgLoginIcon.Source = checkImage;
            imgLoginIcon.Stretch = Stretch.Uniform;
        }

        private void cmdCloseSheet_Click(object sender, MouseButtonEventArgs e)
        {
            AccountContent.Visibility = Visibility.Hidden;
            Sheet.Visibility = Visibility.Hidden;
            MainContentPane.Effect = null;
        }

        private void cmdCreateLocalAccount_Click(object sender, RoutedEventArgs e)
        {
            showSettings(sender, e);
            settings.Focus();
            settings.tabSettings.SelectedItem = settings.tabAccounts;
        }

        private void cmdLoginInfo_Click(object sender, RoutedEventArgs e)
        {
            if (App.Config.username.Equals(""))
            {
                login();
            }
            else
            {
                logOut();
            }
        }

        public void login()
        {
            Sheet.Visibility = System.Windows.Visibility.Visible;
            AccountContent.Visibility = Visibility.Visible;
            lblLoginFail.Foreground = Brushes.Green;
            lblLoginFail.Text = "Press Enter to log in.";
            lblLoginFail.Visibility = System.Windows.Visibility.Visible;
            txtUsername.Focus();
        }

        public void logOut()
        {
            App.Config.username = "";
            App.Config.password = null;
            App.serialize(App.Config);

            txtLoginText.Text = "You are not logged in. Click here to log in.";
            BitmapImage checkImage = new BitmapImage(new Uri("pack://application:,,,/MobiController;component/Resources/x.ico"));
            imgLoginIcon.Source = checkImage;
            imgLoginIcon.Stretch = Stretch.None;
        }

        private void getControllers_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            openControllerBrowser(new frmControllerBrowser());
        }

        private void openControllerBrowser(frmControllerBrowser browser)
        {
            if (!App.MainWin.Browser.SessionCookies.ContainsKey(PHP_COOKIE))
            {
                isOpeningControllerBrowser = true;
                preloadBrowser = browser;
                logOut();
                login();
            }
            else
            {
                browser.Show();
            }
        }

        private void txtUser_keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtPassword.Focus();
            }
        }

        private void Address_Copy_Click(object sender, RoutedEventArgs e)
        {
            cmdCopy_Click(sender, e);
        }
    }
}