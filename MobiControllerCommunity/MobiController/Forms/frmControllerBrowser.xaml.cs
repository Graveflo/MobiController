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
using System.IO;
using System.Diagnostics;
using System.Net.Sockets;
using System.Collections.ObjectModel;

using ModServer;
using MobiControllerBlackBox.Controllers;

namespace MobiController
{
    /// <summary>
    /// Interaction logic for ControllerBrowser.xaml
    /// </summary>
    public partial class frmControllerBrowser : Window
    {

        public enum ASYNC_STATE { NOTREFRESHED, REFRESHING, REFRESHED };
        private ASYNC_STATE state;
        public ASYNC_STATE State
        {
            get { return state; }
            set
            {
                state = value;
                if (value == ASYNC_STATE.REFRESHED)
                {
                    // make sure an action is bound to the event
                    if (RefreshCompleted != null)
                    {
                        RefreshCompleted();
                    }
                }
            }
        }

        public delegate void UpdateHandler();
        public event UpdateHandler RefreshCompleted;

        public ObservableCollection<ControllerInfo> allControllers;
        public ObservableCollection<ControllerInfo> purchasedControllers;
        public ObservableCollection<ControllerInfo> downloadingControllers;

        public static int prgWidth = 180;

        private int finished;

        public frmControllerBrowser()
        {
            DataContext = this;
            State = ASYNC_STATE.NOTREFRESHED;
            allControllers = new ObservableCollection<ControllerInfo>();
            purchasedControllers = new ObservableCollection<ControllerInfo>();
            downloadingControllers = new ObservableCollection<ControllerInfo>();

            InitializeComponent();

            lstAllControllers.ItemsSource = allControllers;
            lstPurchasedControllers.ItemsSource = purchasedControllers;
            lstDownloading.ItemsSource = downloadingControllers;

            downloadingControllers.CollectionChanged += downloadingControllers_CollectionChanged;

            refreshControllers(null, null);
        }

        private void updateControllers(object sender, RoutedEventArgs e)
        {
            List<ControllerInfo> updateList = needsUpdate();
            foreach (ControllerInfo thisController in updateList)
            {
                downloadController(thisController);
            }
        }

        private void recallUpdateControllers()
        {
            RefreshCompleted -= recallUpdateControllers;
            updateControllers(null, null);
        }

        public void picture_Click(object sender, RoutedEventArgs e)
        {
            Image thisImage = (Image)sender;
            thisImage.Height = 500;
        }

        //private GridView get

        private void cmdDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ControllerInfo selectedController;
                if (tabs.SelectedItem == tabPurchased) // comapres references
                {
                    selectedController = (ControllerInfo)lstPurchasedControllers.SelectedItem;
                }
                else
                {
                    selectedController = (ControllerInfo)lstAllControllers.SelectedItem;
                    if (!selectedController.isfree && !purchasedControllers.Contains(selectedController))
                    {
                        App.exec("http://mobicontroller.com/controllers/" + selectedController.FileName);
                        //Process p = new Process(); deprechaited
                        //p.StartInfo.FileName = ;
                        //p.Start();
                        return;
                    }
                }

                //Terminating conditions !!
                if (selectedController == null)
                {
                    return;
                }
                //
                try
                {
                    selectedController.Name = selectedController.FileName;
                    downloadController(selectedController);
                }
                catch (ArgumentException)
                {

                }
            }
            catch (SocketException)
            {
                new MessageBox("Error connecting. Please check your connection.").Show();
            }
        }

        public List<ControllerInfo> needsUpdate()
        {
            List<ControllerInfo> lstNeedsUpdate = new List<ControllerInfo>();
            if (state == ASYNC_STATE.REFRESHED)
            {
                try
                {
                    foreach (ControllerInfo info in ControllerInfo.getInstalledControllers(App.CONTROLLER_DIR))
                    {
                        if (App.Config.updateTimeStamps.ContainsKey(info.FileName))
                        {
                            ControllerInfo officialController = findController(info.FileName); // this way the controller info is synced with mobicontroller.com
                            if (App.Config.updateTimeStamps[info.FileName] <= DateTime.Parse(officialController.Lastupdated))
                            {
                                lstNeedsUpdate.Add(officialController);
                            }
                        }
                        else
                        {
                            findController(info.FileName);
                            lstNeedsUpdate.Add(info);
                        }
                    }
                }
                catch (ControllerInfoException ex)
                {
                    frmSettings.NotifyBadController(ex);
                }
                catch (ArgumentException)// if controller is in the all controller list than add to list.
                { }
            }
            else
            {
                RefreshCompleted += recallUpdateControllers;
                refreshControllers(null, null);
            }
            return lstNeedsUpdate;
        }

        private ControllerInfo findController(string controllername)
        {
            foreach (ControllerInfo i in allControllers)
            {
                if (i.FileName.Equals(controllername))
                {
                    return i;
                }
            }
            throw new ArgumentException("Controller not found.");
        }

        private bool downloadController(ControllerInfo controller)
        {
            if (downloadingControllers.Contains(controller))
            {
                throw new ArgumentException("Already downloading this controller.");
            }
            downloadingControllers.Add(controller);

            string filepath = App.CONTROLLER_DIR + controller.FileName;
            if (controller.isfree)
            {
                filepath += ".rmt";
            }
            else
            {
                filepath += ".ermt";
            }

            Action closeUp = () =>
            {
                Dispatcher.Invoke(() => prgbar.IsIndeterminate = false);
                if (File.Exists(filepath + "~"))
                {
                    File.Delete(filepath);
                    File.Move(filepath + "~", filepath);
                    touchController(controller);
                    Dispatcher.Invoke(() => App.serialize(App.Config));
                }
                Dispatcher.Invoke(() =>
                {
                    if (prgbar.Maximum == prgbar.Value)
                    {
                        prgbar.Value = 0;
                        prgbar.Maximum = 1;
                    }
                    //downloadingControllers.Remove(controller);
                    controller.ImageURI = "/MobiController;component/Resources/check.ico";
                    finished++;
                    downloadingControllers_CollectionChanged(null, null);
                });
            };

            controller.FileLen = 1;
            controller.Progress = 0;
            try
            {
                if (controller.isfree)
                {
                    var newConnection = App.MainWin.Browser.openConnection(frmMain.WEB_ROOT, HttpBrowserEntitiy.PORTS.HTTP);

                    if (prgbar.Maximum <= 1)
                    {
                        prgbar.IsIndeterminate = true;
                        prgbar.Value = 0;
                    }

                    newConnection.sendRequestAsync(new HttpRequest("/releases/MobiController/controllers/free/" + controller.FileName + ".rmt"), (response) =>
                    {
                        if (response.Status == HttpResponse.ConnectionStatus.OK)
                        {
                            using (var fs = File.Create(filepath + "~"))
                            {
                                newConnection.continueDownload(fs, response, (filelen) => Dispatcher.Invoke(() =>
                                {
                                    if (prgbar.Maximum <= 1)
                                    {
                                        prgbar.Maximum = 0;
                                        prgbar.IsIndeterminate = false;
                                        filelen--; // because maximum is set to 1
                                    }
                                    controller.FileLen = filelen;
                                    prgbar.Maximum += filelen;
                                }), (read) => Dispatcher.Invoke(() =>
                                {
                                    prgbar.Value += read;
                                    controller.Progress += read;
                                }));
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(() =>
                            new MessageBox("There was an error downloading the controller: " + controller.FileName).Show());
                            controller.ImageURI = "/MobiController;component/Resources/x.ico";
                        }
                        closeUp();
                    });
                }
                else
                {
                    string hostname = "secure144.inmotionhosting.com";
                    var thisConnection = App.MainWin.Browser.openSecureConnection(hostname, HttpBrowserEntitiy.PORTS.SSL, hostname);
                    var request = new HttpRequest("/~r4msof5/releases/MobiController/controllers/download.php?file=" + controller.FileName);
                    request.cookies = App.MainWin.Browser.SessionCookies;
                    thisConnection.sendRequestAsync(request, (response) =>
                    {
                        bool error = false;
                        if (response.Status == HttpResponse.ConnectionStatus.FORBIDDEN)
                        {
                            Dispatcher.Invoke(() =>
                            new MessageBox("You are either not logged in or have not purchased this controller. mobicontroller.com has denied access.").Show());
                            error = true;
                        }
                        if (response.Status == HttpResponse.ConnectionStatus.FILE_NOT_FOUND)
                        {
                            Dispatcher.Invoke(() =>
                            new MessageBox("The file was not found on mobicontroller.com").Show());
                            error = true;
                        }
                        if (!error)
                        {
                            try
                            {
                                MobiControllerBlackBox.BlackBox.continueSafeDownload(thisConnection, response, App.Config.username, filepath + "~", (filelen) => Dispatcher.Invoke(() =>
                                {
                                    if (prgbar.Maximum <= 1)
                                    {
                                        prgbar.Maximum = 0;
                                        prgbar.IsIndeterminate = false;
                                        filelen--; // because maximum is set to 1
                                    }
                                    controller.FileLen = filelen;
                                    prgbar.Maximum += filelen;
                                }), (read) =>
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        prgbar.Value += read;
                                        controller.Progress += read;
                                    });
                                });
                            }
                            catch (ConnectionException)
                            {
                                Dispatcher.Invoke(() =>
                            new MessageBox("There was an unexpected response from the sever. Download Failed.").Show());
                                controller.ImageURI = "/MobiController;component/Resources/x.ico";
                            }
                        }
                        else
                        {
                            controller.ImageURI = "/MobiController;component/Resources/x.ico";
                        }
                        closeUp();
                    });
                }
            }
            catch (IOException)
            {
                new MessageBox("Error Saving controller. Make sure it is not loaded and that MobiController has permissions to write to: " + App.APP_DATA_FOLDER).Show();
                controller.ImageURI = "/MobiController;component/Resources/x.ico";
                return false;
            }
            catch (SocketException)
            {
                new MessageBox("Error connecting. Please check your connection.").Show();
                controller.ImageURI = "/MobiController;component/Resources/x.ico";
                return false;
            }
            return true;
        }

        private void touchController(ControllerInfo controller)
        {
            if (App.Config.updateTimeStamps.ContainsKey(controller.FileName))
            {
                App.Config.updateTimeStamps.Remove(controller.FileName);
            }

            App.Config.updateTimeStamps.Add(controller.FileName, DateTime.Now);
        }

        void downloadingControllers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            tabDownoading.Header = "Downloading (" + (downloadingControllers.Count - finished) + ")";
        }

        //private void enableButton()
        //{
        //    cmdDownload.Foreground = Brushes.White;
        //    cmdDownload.IsEnabled = true;
        //}

        private void picture_Click(object sender, MouseButtonEventArgs e)
        {
            Image thisimage = (Image)sender;
            picDisplay.Source = thisimage.Source;
            Shade.Visibility = System.Windows.Visibility.Visible;
            picDisplay.Visibility = System.Windows.Visibility.Visible;
        }

        private void Shade_Click(object sender, MouseButtonEventArgs e)
        {
            Shade.Visibility = System.Windows.Visibility.Hidden;
            picDisplay.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Shade_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Shade_Click(sender, null);
            }
        }

        private void refreshControllers(object sender, RoutedEventArgs e)
        {
            //HelperClass helper = new HelperClass();
            State = ASYNC_STATE.REFRESHING;
            allControllers.Clear();
            purchasedControllers.Clear();

            try
            {
                var thisConnection = App.MainWin.Browser.openConnection(frmMain.WEB_ROOT, 80);
                {
                    thisConnection.sendRequestAsync(new HttpRequest("/releases/MobiController/controllers/listpaid.php"), (response) =>
                    {

                        string[] cs = response.Body.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 1; i < cs.Length; i++) // i=1 accounts for apache's weird header
                        {
                            var newConnection = App.MainWin.Browser.openConnection(frmMain.WEB_ROOT, 80);
                            var thisController = new ControllerInfo(false) { Description = "sample description", ImageSource = "http://mobicontroller.com/controllers/" + cs[i] + "/1.png", Lastupdated = "never", FileName = cs[i] };
                            newConnection.sendRequestAsync(new HttpRequest("/controllers/" + cs[i] + "/index.php"), (htmlresponse) =>
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    thisController.Description = HelperClass.stripHtmlParagraph(HelperClass.getIsolatedString("<section>", "</section>", htmlresponse.Body));
                                    thisController.Lastupdated = HelperClass.getIsolatedString("<b> Last Updated: ", "</b>", htmlresponse.Body);
                                    allControllers.Insert(0, thisController);
                                });
                            });
                        }
                    });
                }

                thisConnection = App.MainWin.Browser.openConnection(frmMain.WEB_ROOT, 80); // just in case the other thread is slow
                thisConnection.sendRequestAsync(new HttpRequest("/releases/MobiController/controllers/listfree.php"), (response) =>
                {
                    string[] cs = response.Body.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 1; i < cs.Length; i++) // i=1 accounts for apache's weird header
                    {
                        var newConnection = App.MainWin.Browser.openConnection(frmMain.WEB_ROOT, 80);
                        HttpResponse htmlresponse = newConnection.sendRequest(new HttpRequest("/controllers/" + cs[i] + "/index.php"));
                        var thisController = new ControllerInfo(true) { Description = "sample description", ImageSource = "http://mobicontroller.com/controllers/" + cs[i] + "/1.png", Lastupdated = "never", FileName = cs[i] };
                        Dispatcher.Invoke(() =>
                        {
                            //Features</h2><div style='text-align:left'> <div style=\"text-align:center;\">
                            thisController.Description = HelperClass.stripHtmlParagraph(HelperClass.getIsolatedString("<section>", "</section>", htmlresponse.Body));
                            thisController.Lastupdated = HelperClass.getIsolatedString("<b> Last Updated: ", "</b>", htmlresponse.Body);
                            allControllers.Add(thisController);

                        });
                    }
                });
                if (App.MainWin.Browser.SessionCookies.ContainsKey(frmMain.PHP_COOKIE))
                {
                    refreshPurchased();
                }
                else
                {
                    State = ASYNC_STATE.REFRESHED;
                }
            }
            catch (SocketException)
            {
                State = ASYNC_STATE.NOTREFRESHED;
                new MessageBox("Error connecting. Please check your connection.").Show();
            }
        }

        public void refreshPurchased()
        {
            string hostname = "secure144.inmotionhosting.com";
            var request = new HttpRequest("/~r4msof5/releases/MobiController/controllers/listpurchased.php");
            try
            {
                var thisConnection = App.MainWin.Browser.openSecureConnection(hostname, HttpBrowserEntitiy.PORTS.SSL, hostname);
                request.cookies = App.MainWin.Browser.SessionCookies;
                thisConnection.sendRequestAsync(request, (response) =>
                {
                    string[] cs = response.Body.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    if (!response.Body.Contains("You have not purchased any controllers"))
                    {
                        for (int i = 1; i < cs.Length; i++) // i=1 accounts for apache's weird header
                        {
                            var newConnection = App.MainWin.Browser.openConnection(frmMain.WEB_ROOT, HttpBrowserEntitiy.PORTS.HTTP);
                            HttpResponse htmlresponse = newConnection.sendRequest(new HttpRequest("/controllers/" + cs[i] + "/index.php"));
                            Dispatcher.Invoke(() =>
                            {
                                var thisController = new ControllerInfo(false) { Description = "sample description", ImageSource = "http://mobicontroller.com/controllers/" + cs[i] + "/1.png", Lastupdated = "never", FileName = cs[i] };
                                thisController.Description = HelperClass.stripHtmlParagraph(HelperClass.getIsolatedString("<section>", "</section>", htmlresponse.Body));
                                thisController.Lastupdated = HelperClass.getIsolatedString("<b> Last Updated: ", "</b>", htmlresponse.Body);
                                purchasedControllers.Add(thisController);
                                State = ASYNC_STATE.REFRESHED;
                            });
                        }
                    }
                });
            }
            catch (SocketException)
            {
                State = ASYNC_STATE.NOTREFRESHED;
            }
        }

        private void prgbar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ProgressBar thisbar = (ProgressBar)sender;
            thisbar.Width -= 5;
        }

        private void cmdHelp_Click(object sender, RoutedEventArgs e)
        {
            App.exec("http://mobicontroller.com/controllerbrowser.php");
        }

        private void DownloadAllControllers(object sender, RoutedEventArgs e)
        {
            ObservableCollection<ControllerInfo> thisList;
            if (tabs.SelectedItem == tabPurchased)
            {
                thisList = purchasedControllers;
            }
            else
            {
                thisList = allControllers;
            }
            //ControllerInfo selectedController;
            bool showpage = false;
            foreach (ControllerInfo selectedController in thisList)
            {
                try
                {

                    if (!selectedController.isfree && !purchasedControllers.Contains(selectedController))
                    {
                        showpage = true;
                        continue;
                    }

                    //Terminating conditions !!
                    if (selectedController == null)
                    {
                        return;
                    }
                    //
                    try
                    {
                        selectedController.Name = selectedController.FileName;
                        downloadController(selectedController);
                    }
                    catch (ArgumentException)
                    {

                    }
                }
                catch (SocketException)
                {
                    new MessageBox("Error connecting. Please check your connection.").Show();
                }
            }
            if (showpage)
            {
                App.exec("http://mobicontroller.com/controllers");
            }
        }

    }
}
