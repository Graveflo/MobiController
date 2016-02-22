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
using System.Diagnostics;
using System.Collections.Generic;

namespace MobiController
{
    /// <summary>
    /// Interaction logic for frmLogView.xaml
    /// </summary>
    public partial class frmLogView : Window
    {
        //public Action ViewLogFileLoc
        //{
        //    get
        //    {
        //        return () => new MessageBox("sdf").Show();
        //    }
        //}

        public ObservableCollection<Event> myList
        {
            get
            {
                return App.Log.ObservableEvents;
            }
        }

        public frmLogView()
        {
            InitializeComponent();
        }

        //public void LogNewEvent(Event loggedEvent)
        //{
        //    String[] message = loggedEvent.ToString().Split(Event.DELIMETER.ToCharArray());

        //    ListViewItem newitem = new ListViewItem();
            
        //    newitem.SubItems.Add(message[1]);
        //    newitem.SubItems.Add(message[2]);
        //    newitem.SubItems.Add(message[3]);
        //    if (lstEventVeiw.InvokeRequired)
        //    {
        //        try
        //        {
        //            lstEventVeiw.Invoke(new Action<Event>(LogNewEvent), loggedEvent);
        //        }
        //        catch (ObjectDisposedException) { };
        //        {

        //        }
        //        return;
        //    }
        //    else
        //    {
        //        lstEventVeiw.Items.Add(newitem);
        //    }
        //}
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = System.Windows.Visibility.Hidden;
            base.OnClosing(e);
        }

        private void lstEventsDouble_Click(object sender, MouseButtonEventArgs e)
        {
            if (lstEventLog.SelectedItems.Count > 0)
            {
                foreach (Event thisEvent in lstEventLog.SelectedItems)
                {
                    LogInfoView l = new LogInfoView(thisEvent.Message);
                    l.Show();
                }
            }
        }

        private void menuItem_logfilelocation_click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "explorer.exe";
            p.StartInfo.Arguments = "/select," + EventLog.LOG_FILE;
            p.Start();
        }

        private void MenuItem_ViewDetail_Click(object sender, RoutedEventArgs e)
        {
            lstEventsDouble_Click(sender, null);
        }

        private void MenuItem_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (lstEventLog.SelectedItems.Count > 0)
            {
                List<Event> eventsToRemove = new List<Event>();
                foreach (Event thisevent in lstEventLog.SelectedItems)
                {
                    eventsToRemove.Add(thisevent);
                }
                foreach (Event thisEvent in eventsToRemove)
                {
                    App.Log.ObservableEvents.Remove(thisEvent);
                }
            }
        }

        private void MenuItem_OnlineHelp_Click(object sender, RoutedEventArgs e)
        {
            App.exec("http://mobicontroller.com/logview.php");
        }

    }
}
