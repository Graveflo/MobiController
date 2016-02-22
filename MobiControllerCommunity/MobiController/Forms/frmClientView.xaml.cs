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

using System.ComponentModel;

namespace MobiController
{
    /// <summary>
    /// Interaction logic for ClientView.xaml
    /// </summary>
    public partial class frmClientView : Window
    {
        public ObservableCollection<MyClientContainer> mylist;
        private string banme;

        public void ConnectionDisconnect(MyClientContainer client)
        {
            Dispatcher.Invoke(new Action(() => mylist.Remove(client)));
        }

        public void ConnectionInit(MyClientContainer client)
        {
            Dispatcher.Invoke(new Action(() => mylist.Add(client)));
        }

        private void refreshGroups()
        {
            //((ListViewItem)lstClients.Items[0]).gr
        }

        public void RefreshClientInfo(MyClientContainer client)
        {
            //Dispatcher.Invoke(new Action(() => mylist.Add(client)));
        }

        public frmClientView()
        {
            banme = "";
            //DataContext = this;
            InitializeComponent();
            mylist = new ObservableCollection<MyClientContainer>();
            //lstClients.Items.Clear();// = null;
            //lstClients.ItemsSource = mylist;

            ICollectionView view = CollectionViewSource.GetDefaultView(mylist);
            view.GroupDescriptions.Add(new PropertyGroupDescription("IP"));
            view.SortDescriptions.Add(new SortDescription("IP", ListSortDirection.Ascending));
            lstClients.ItemsSource = view;
            //lstClients.View = view;
            //lstClients.ItemsSource = mylist;
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = System.Windows.Visibility.Hidden;
            base.OnClosing(e);
        }

        private void lblIP_Click(object sender, MouseButtonEventArgs e)
        {
            TextBlock thisBlock = (TextBlock)sender;
            banme = thisBlock.Text;
            e.Handled = true;
        }

        private void tmpbanip_click(object sender, RoutedEventArgs e)
        {
            myTcpServer.tempBanIP(banme);
        }


        private void kickdevice_click(object sender, RoutedEventArgs e)
        {
            MyClientContainer client;
            try
            {
                client = (MyClientContainer)lstClients.SelectedItem;
            }
            catch (InvalidCastException) { return; }
            if (client != null)
            {
                client.SessionVariables.IsAuthenticated = false;
                client.SessionVariables.Auth = "None";
                client.SessionVariables.LoadedController = null;
            }
        }

        private void tmpbanagent_click(object sender, RoutedEventArgs e)
        {
            MyClientContainer client;
            try
            {
                client = (MyClientContainer)lstClients.SelectedItem;
            }
            catch (InvalidCastException) { return; }
            if (client != null)
            {
                myTcpServer.tempBanAgent(client.Agent);
            }
        }

        private void lstClients_Click(object sender, MouseButtonEventArgs e)
        {
            MyClientContainer client;
            try
            {
                client = (MyClientContainer)lstClients.SelectedItem;
            }
            catch (InvalidCastException) { return; }
            if (client != null)
            {
                banme = client.IP;
            }
        }

        private void onlinehelp_click(object sender, RoutedEventArgs e)
        {
            App.exec("http://mobicontroller.com/clientview.php");
        }
    }
}
