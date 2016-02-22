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
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;

using System.Threading;

using System.Net.Sockets;
using System.Net;

using ModServer;
using MobiControllerBlackBox.Controllers;

namespace MobiController
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class frmSettings : Window
    {
        public const string HKCU_RUN_KEY_PATH = @"Software\Microsoft\Windows\CurrentVersion\Run";
        public ObservableCollection<String> AccountList;
        public ObservableCollection<ControllerInfo> ControllerList;

        private bool isSeen;

        public bool IsSeen
        {
            get { return isSeen; }
            set { isSeen = value; }
        }

        private Boolean isModified;
        public Boolean IsModified
        {
            set
            {
                isModified = value;
                if (value)
                {
                    if (lblStatus == null || !this.IsLoaded) { return; }
                    lblStatus.Foreground = Brushes.Red;
                    lblStatus.Text = "Modified.";
                }
                else
                {
                    if (lblStatus == null || !this.IsLoaded) { return; }
                    lblStatus.Foreground = Brushes.Lime;
                    lblStatus.Text = "Saved.";
                }
            }
            get
            {
                return isModified;
            }
        }

        public Thread startme;

        public frmSettings()
        {
            DataContext = this;
            isSeen = false;

            AccountList = new ObservableCollection<String>();
            ControllerList = new ObservableCollection<ControllerInfo>();

            isModified = false;

            InitializeComponent();
            lstAccounts.ItemsSource = AccountList;
            lstTmpIPs.ItemsSource = myTcpServer.Blocked;
            lstTempAgents.ItemsSource = MyClientContainer.banAgents;
            loadSettings();

            updateNames();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (isModified == true && System.Windows.MessageBox.Show("You have modified some of the settings but not Saved them. Are you sure you want to CLOSE THIS WINDOW?", "Careful!", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            //else
            //{
            //    e.Cancel = true;
            //    HasClosed = true;
            //    App.MainWin.settings=null;
            //    this.Visibility = Visibility.Hidden;
            //}
            base.OnClosing(e);
        }

        private void loadSettings()
        {
            chkStartInTray.IsChecked = App.Config.isStartInTray;
            this.chkListenOnLoad.IsChecked = App.Config.isAutoListen;
            this.chkStartOnOSStartup.IsChecked = App.Config.isAutoStart;
            this.chkDisplayBaloon.IsChecked = App.Config.isShowMessageOnMinimize;
            this.chkUPnP.IsChecked = App.Config.isUPnPonStart;
            //
            this.txtName.Text = App.Config.ServerName;
            this.txtPort.Text = App.Config.SERVER_PORT.ToString();
            this.txtMaxConnections.Text = App.Config.MaxConnections.ToString();
            this.txtUPnPPort.Text = App.Config.UPnPPort.ToString();

            IsModified = false;
        }

        public bool updateNames()
        {
            updateUserNames(Bouncer.UserNames);
            return true;
        }

        public void updateUserNames(string[] keys)
        {
            AccountList.Clear();
            foreach (string s in keys)
            {
                AccountList.Add(s);
            }
        }

        private void cmdSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            App.Config.isAutoListen = chkListenOnLoad.IsChecked.Value;
            App.Config.isAutoStart = chkStartOnOSStartup.IsChecked.Value;
            App.Config.isStartInTray = chkStartInTray.IsChecked.Value;
            App.Config.isShowMessageOnMinimize = chkStartOnOSStartup.IsChecked.Value;
            App.Config.isUPnPonStart = chkUPnP.IsChecked.Value;
            //
            App.Config.ServerName = txtName.Text;
            int newPort = 80;
            bool isError = false;
            try
            {
                newPort = Convert.ToInt32(txtPort.Text);
                txtPort.Foreground = Brushes.White;
            }
            catch (FormatException)
            {
                isError = true;

            }
            if (isError || newPort < 1) { txtPort.Foreground = Brushes.Red; lblStatus.Text = "Errors! Check Values."; return; }
            else
            {
                bool restart = App.Config.SERVER_PORT != newPort;
                App.Config.SERVER_PORT = newPort;
                if (restart)
                    Dispatcher.Invoke(() =>
                    {
                        App.MainWin.StopServer(true);
                        App.MainWin.StartServer(true);
                    });
            }

            try
            {
                newPort = Convert.ToInt32(txtUPnPPort.Text);
                txtUPnPPort.Foreground = Brushes.White;
            }
            catch (FormatException)
            {
                isError = true;
            }
            if (isError || newPort < 1) { txtUPnPPort.Foreground = Brushes.Red; lblStatus.Text = "Errors! Check Values."; return; }
            else
            {
                App.Config.UPnPPort = newPort;
            }

            try
            {
                App.Config.MaxConnections = Convert.ToInt32(txtMaxConnections.Text);
                txtMaxConnections.Foreground = Brushes.White;
            }
            catch (FormatException)
            {
                txtMaxConnections.Foreground = Brushes.Red;

            }

            App.serialize(App.Config);

            var RunKey = Registry.CurrentUser.OpenSubKey(HKCU_RUN_KEY_PATH, true);
            if (chkStartOnOSStartup.IsChecked.Value)
            {
                RunKey.SetValue(App.APPNAME, "\"" + System.Reflection.Assembly.GetExecutingAssembly().CodeBase.ToString().Replace("file:///", "").Replace('/', '\\') + "\"");
            }
            else
            {
                if (RunKey.GetValue(App.APPNAME) != null)
                {
                    RunKey.DeleteValue(App.APPNAME);
                }
            }

            IsModified = false;
        }


        private void gen_Click(object sender, RoutedEventArgs e)
        {
            IsModified = true;
        }

        private void gen_Click(object sender, TextChangedEventArgs e)
        {
            IsModified = true;
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cmdRemoveUser_Click(object sender, RoutedEventArgs e)
        {
            if (Bouncer.UserNames.Contains(txtAccountUserName.Text))
            {
                MessageBox thisBox = new MessageBox(this, "Are you sure you want to remove the account " + txtAccountUserName.Text + "?", "Account Manager");
                thisBox.addButton("Cancel", delegate() { });
                thisBox.addButton("Remove", () => { Bouncer.removeAccount(txtAccountUserName.Text); updateNames(); });
                thisBox.Show();
            }
            else
            {
                new MessageBox("The user specified could not be found.", "Error").Show();
            }
        }

        private void cmdAddUser_Click(object sender, RoutedEventArgs e)
        {
            if (txtAccountUserName.Text.Length > 1 && txtAccountPassword.Password.Length > 1)
            {
                if (Bouncer.addAccount(txtAccountUserName.Text, txtAccountPassword.Password))
                {
                    updateNames();
                }
                else
                {
                    new MessageBox("You cannot have two users with the same account name.", "Error").Show();
                }
            }
        }

        private void lstAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                txtAccountUserName.Text = lstAccounts.SelectedItem.ToString();
            }
            catch (Exception) { };
        }

        private void cmdAddController_Click(object sender, RoutedEventArgs e)
        {
            App.MainWin.MenuItem_AddController_Click(sender, e);
        }

        private void cmdRemoveController_Click(object sender, RoutedEventArgs e)
        {
            if (lstControllers.SelectedItems.Count > 0)
            {
                List<ControllerInfo> controllersToRemove = new List<ControllerInfo>();
                StringBuilder controllerNames = new StringBuilder();

                foreach (ControllerInfo thiscontroller in lstControllers.SelectedItems)
                {
                    controllersToRemove.Add(thiscontroller);
                    controllerNames.Append(thiscontroller.Name);
                    controllerNames.Append("\r\n");
                }
                MessageBox thisMessage = new MessageBox("Are you sure you want to permanently delete the following Controller(s) from your computer? \r\n" + controllerNames.ToString(), "");
                thisMessage.addButton("Cancel", delegate() { });
                thisMessage.addButton("Remove Listed", () =>
                {
                    foreach (ControllerInfo thisController in controllersToRemove)
                    {
                        try
                        {
                            thisController.Path.Delete();
                        }
                        catch (IOException)
                        {
                        }
                        ControllerList.Remove(thisController);
                    }
                });
                thisMessage.Show();
                thisMessage.Focus();
            }
        }

        private void tabSettings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabSettings.SelectedItem == tabiControllers)
            {
                try
                {
                    ControllerList = new ObservableCollection<ControllerInfo>(ControllerInfo.getInstalledControllers(App.CONTROLLER_DIR));
                    lstControllers.ItemsSource = ControllerList;
                }
                catch (ControllerInfoException ex)
                {
                    NotifyBadController(ex);
                }
            }
        }

        public static void NotifyBadController(ControllerInfoException ex)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MessageBox alterBox = new MessageBox("An unexpected error occured while trying to read the controller file: " + ex.Controller.Name + "."); ;
                switch (ex.Reason)
                {
                    case ControllerException.REASON.BAD_REMOTE:
                        alterBox = new MessageBox("The controller file: " + ex.Controller.Name + ", was structured incorrectly and cannot be used.");
                        break;
                }
                alterBox.addButton("Remove Controller", () => File.Delete(ex.Controller.FullName));
                alterBox.addButton("Cancel", delegate() { });
                alterBox.Show();
            });
        }

        private void lstControllers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }


        private void getControllers_Click(object sender, SelectionChangedEventArgs e)
        {
            //new Controll
        }

        private void cmdUnban_Click(object sender, RoutedEventArgs e)
        {

            if (lstTempAgents.IsFocused)
            {
                string agent;
                try
                {
                    agent = lstTempAgents.SelectedItem.ToString();
                }
                catch (InvalidCastException) { return; }

                MyClientContainer.banAgents.Remove(agent);
            }
            else if (lstTmpIPs.IsFocused)
            {
                string agent;
                try
                {
                    agent = lstTempAgents.SelectedItem.ToString();
                }
                catch (InvalidCastException) { return; }

                MyClientContainer.banAgents.Remove(agent);
            }
        }

        private void resetAllIP_click(object sender, RoutedEventArgs e)
        {
            myTcpServer.Blocked = new HashSet<string>();
            lstTmpIPs.ItemsSource = new HashSet<string>();
            lstTmpIPs.ItemsSource = myTcpServer.Blocked;
        }

        private void resetBanIP_click(object sender, RoutedEventArgs e)
        {
            try
            {
                myTcpServer.Blocked.Remove(lstTmpIPs.SelectedItem.ToString());
                lstTmpIPs.UpdateLayout();
                lstTmpIPs.ItemsSource = new HashSet<string>();
                lstTmpIPs.ItemsSource = myTcpServer.Blocked;
            }
            catch (NullReferenceException) { }
        }

        private void resetBanDevice_click(object sender, RoutedEventArgs e)
        {
            try
            {
                MyClientContainer.banAgents.Remove(lstTempAgents.SelectedItem.ToString());
                //lstTempAg
                lstTempAgents.ItemsSource = new HashSet<string>();
                lstTempAgents.ItemsSource = MyClientContainer.banAgents;
            }
            catch (NullReferenceException) { }
        }

        private void resetAllDevice_click(object sender, RoutedEventArgs e)
        {
            MyClientContainer.banAgents = new HashSet<string>();
            lstTempAgents.ItemsSource = new HashSet<string>();
            lstTempAgents.ItemsSource = MyClientContainer.banAgents;
        }

        private void cmdHelp_Click(object sender, RoutedEventArgs e)
        {
            App.exec("http://mobicontroller.com/settings.php");
        }

        private void cmdUPnPEnable_Click(object sender, RoutedEventArgs e)
        {
            StartUPnP();
        }

        public void StartUPnP()
        {
            startme = new Thread(new ThreadStart(startUPnP));
            startme.Start();
        }

        private void startUPnP()
        {
            Dispatcher.Invoke(() =>
            {
                BitmapImage bi3 = new BitmapImage();
                bi3.BeginInit();
                bi3.UriSource = new Uri("/MobiController;component/Resources/x.ico", UriKind.Relative);
                bi3.EndInit();
                imgUPnPstat.Source = bi3;
                prgbarUPnP.Value = 0;
                lblUPnPStat.Text = "";
            });

            bool Discovered = false;
            try
            {
                Discovered = UPnP.Discover();
            }
            catch (SocketException)
            {
                Dispatcher.Invoke(() =>
                {
                    lblUPnPStat.Foreground = Brushes.Red;
                    lblUPnPStat.Text = "No UPnP enabled router was found.";
                });
            }
            catch (WebException)
            {
                Dispatcher.Invoke(() =>
                {
                    lblUPnPStat.Foreground = Brushes.Red;
                    lblUPnPStat.Text = "No UPnP enabled router was found.";
                });
            }
            int serverPort=-1;
            int UPnPPort=-1;
            try{
                Dispatcher.Invoke(() =>
                {
                    serverPort = Convert.ToInt32(txtPort.Text);
                    UPnPPort = Convert.ToInt32(txtUPnPPort.Text);
                });
            }catch(FormatException){
                Dispatcher.Invoke(() =>
                {
                    new MessageBox("Error converting ports. Choose numeric port numbers.").Show();
                });
                return;
            }
            if(serverPort < 1 || UPnPPort < 1){
                Dispatcher.Invoke(() => new MessageBox("Port error. Please specify positive port values.").Show());
                return;
            }
            if (Discovered)
            {
                Dispatcher.Invoke(()=>prgbarUPnP.Value += 20);
                try
                {
                    UPnP.deletePortMapping(UPnPPort, "TCP");
                }
                catch (SocketException)
                { }
                catch (WebException) { }
                Dispatcher.Invoke(()=>prgbarUPnP.Value += 20);
                try
                {
                    UPnP.addPortMapping(UPnPPort, serverPort, myTcpServer.getLocalIP().ToString(), "TCP","MobiController");
                }
                catch (SocketException)
                {
                    Dispatcher.Invoke(() =>
                    {
                        lblUPnPStat.Foreground = Brushes.Red;
                        lblUPnPStat.Text = "Error mapping port. Try a port number greater than 6000.";
                    });
                    return;
                }
                catch (WebException)
                {
                    Dispatcher.Invoke(() =>
                    {
                        lblUPnPStat.Foreground = Brushes.Red;
                        lblUPnPStat.Text = "Error mapping port. Try a port number greater than 6000.";
                    });
                    return;
                }
                Dispatcher.Invoke(()=>prgbarUPnP.Value += 20);
                string ExternalIp="";
                try
                {
                    ExternalIp = UPnP.getExternalIPAddress();
                }
                catch (SocketException)
                {
                    Dispatcher.Invoke(() =>
                    {
                        lblUPnPStat.Foreground = Brushes.Red;
                        lblUPnPStat.Text = "Unexpected error getting external IP. The port mapping may still work.";
                    });
                    return;
                }
                catch (WebException)
                {
                    Dispatcher.Invoke(() =>
                    {
                        lblUPnPStat.Foreground = Brushes.Red;
                        lblUPnPStat.Text = "Unexpected error getting external IP. The port mapping may still work.";
                    });
                    return;
                }
                Dispatcher.Invoke(() =>
                {
                    txtExternalIP.Text = ExternalIp;
                    if (UPnPPort != 80)
                    {
                        txtExternalIP.Text += ":" + UPnPPort.ToString();
                    }
                    prgbarUPnP.Value += 20;
                });
                try
                {
                    HttpBrowserEntitiy browser = new HttpBrowserEntitiy();
                    var conn = browser.openConnection(ExternalIp, UPnPPort);
                    var request = new HttpRequest("/UPnP");
                    request.addHeader("Connection", "Close");
                    request.addHeader("upnptestnum", myHttpEngine.UPNP_TEST_NUMBER.ToString());
                    request.Method = HttpEngine.META_HEAD;
                    conn.sendRequest(request);
                }
                catch (SocketException)
                {
                    Dispatcher.Invoke(() =>
                    {
                        lblUPnPStat.Foreground = Brushes.Red;
                        lblUPnPStat.Text = "Error testing connection, make sure there are no other port mappings to your local port.";
                    });
                }
            }
        }
        private void FinishUPnP(){
            prgbarUPnP.Value = prgbarUPnP.Maximum;
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri("/MobiController;component/Resources/check.ico", UriKind.Relative);
            bi3.EndInit();
            imgUPnPstat.Source = bi3;
            lblUPnPStat.Foreground = Brushes.Green;
            lblUPnPStat.Text = "Port mapping successful. Use the above number to access your computer from outside your home network.";
        }
        public void finishUPnP(){
            Dispatcher.Invoke(() => FinishUPnP());
        }

        private void cmdUPnPDisable_Click(object sender, RoutedEventArgs e)
        {
            disableUPnP();
        }
        public void disableUPnP()
        {
            Thread startme = new Thread(new ThreadStart(DisableUPnP));
            startme.Start();
        }
        private void DisableUPnP()
        {
            Dispatcher.Invoke(() =>
            {
                BitmapImage bi3 = new BitmapImage();
                bi3.BeginInit();
                bi3.UriSource = new Uri("/MobiController;component/Resources/x.ico", UriKind.Relative);
                bi3.EndInit();
                imgUPnPstat.Source = bi3;
                prgbarUPnP.Value = 0;
                lblUPnPStat.Text = "";
            });
            bool Discovered = false;
                        try
            {
                Discovered = UPnP.Discover();
            }
            catch (SocketException)
            {
                Dispatcher.Invoke(() =>
                {
                    lblUPnPStat.Foreground = Brushes.Red;
                    lblUPnPStat.Text = "No UPnP enabled router was found.";
                });
            }
            catch (WebException)
            {
                Dispatcher.Invoke(() =>
                {
                    lblUPnPStat.Foreground = Brushes.Red;
                    lblUPnPStat.Text = "No UPnP enabled router was found.";
                });
            }
            int serverPort=-1;
            int UPnPPort=-1;
            try{
                Dispatcher.Invoke(() =>
                {
                    serverPort = Convert.ToInt32(txtPort.Text);
                    UPnPPort = Convert.ToInt32(txtUPnPPort.Text);
                });
            }catch(FormatException){
                Dispatcher.Invoke(() =>
                {
                    new MessageBox("Error converting ports. Choose numeric port numbers.").Show();
                });
                return;
            }
            if(serverPort < 1 || UPnPPort < 1){
                Dispatcher.Invoke(() => new MessageBox("Port error. Please specify positive port values.").Show());
                return;
            }
            if (Discovered)
            {
                Dispatcher.Invoke(() => prgbarUPnP.Value += 50);
                try
                {
                    UPnP.deletePortMapping(UPnPPort, "TCP");
                }
                catch (SocketException)
                {
                    Dispatcher.Invoke(() =>
                    {
                        lblUPnPStat.Foreground = Brushes.Red;
                        lblUPnPStat.Text = "Error deleting port mapping. Make sure the port is set to the entry you want to remove.";
                    });
                    return;
                }
                catch (WebException)
                {
                    Dispatcher.Invoke(() =>
                    {
                        lblUPnPStat.Foreground = Brushes.Red;
                        lblUPnPStat.Text = "Error deleting port mapping. Make sure the port is set to the entry you want to remove.";
                    });
                    return;
                }
                Dispatcher.Invoke(() => {
                prgbarUPnP.Value = prgbarUPnP.Maximum;
                BitmapImage bi3 = new BitmapImage();
                bi3.BeginInit();
                bi3.UriSource = new Uri("/MobiController;component/Resources/check.ico", UriKind.Relative);
                bi3.EndInit();
                imgUPnPstat.Source = bi3;
                lblUPnPStat.Foreground = Brushes.Green;
                lblUPnPStat.Text = "Port mapping removed successfully.";
                });

            }
        }

        private void frmSettings_Loaded(object sender, RoutedEventArgs e)
        {
            isSeen = true;
        }
    }
}
