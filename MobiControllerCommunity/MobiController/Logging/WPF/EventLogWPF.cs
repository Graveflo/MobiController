using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Interop;

using WinAPIWrapper;

namespace MobiController
{
    public class EventLogWPF : EventLog
    {
        public ObservableCollection<Event> ObservableEvents
        {
            get
            {
                return (ObservableCollection<Event>) events;
            }
        }

        public EventLogWPF()
        {
            events = new ObservableCollection<Event>();
        }

        public override bool shouldWriteOut(string message, Event.EVENT_FLAGS Flags)
        {
            return (Flags & Event.EVENT_FLAGS.IMPORTANT) != 0;
        }

        public override void logEvent(string messageBody, Event.EVENT_FLAGS Flags)
        {
            if (App.MainWin == null)
            {
                base.logEvent(messageBody, Flags);
                return;
            }
            if ((Flags & Event.EVENT_FLAGS.IMPORTANT) != 0)
            {
                App.MainWin.Dispatcher.Invoke(() =>
                {
                    App.MainWin.notifyIcon.BalloonTipClicked += baloonClick;
                    App.MainWin.notifyIcon.ShowBalloonTip(2000, "Important!", messageBody, System.Windows.Forms.ToolTipIcon.Info);
                    
                });
            }
            App.MainWin.Dispatcher.Invoke(new Action(() => base.logEvent(messageBody, Flags)));
        }

        void baloonClick(object sender, EventArgs e)
        {
            App.LogView.Show();
            IntPtr hWnd = new WindowInteropHelper(App.LogView).Handle;
            WinAPI.SetForegroundWindow(hWnd);
            App.MainWin.notifyIcon.BalloonTipClicked -= baloonClick;
        }
    }
}
