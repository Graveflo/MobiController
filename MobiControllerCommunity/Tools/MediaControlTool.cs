using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

using ModServer;
using ToolBox;
using WinAPIWrapper;

namespace Tools
{
    [Serializable()]
    public class MediaControlTool : ATool
    {

        private WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM command;
        public WinAPI.SENDMESSAGE.WM_APPCOMMAND.LPARAM Command
        {
            set
            {
                command = value;
            }
        }
        private Boolean broadcast = false;
        public Boolean Broadcast
        {
            set
            {
                broadcast = value;
            }
        }
        private Boolean hwndbroadcast = false;
        public Boolean HwndBroadcast
        {
            set
            {
                hwndbroadcast = value;
            }
        }

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            IntPtr capture=IntPtr.Zero;
            if (hwndbroadcast)
            {
                capture = WinAPI.SendMessage(WinAPI.HWND_BROADCAST, WinAPI.SENDMESSAGE._MSG.WM_APPCOMMAND, IntPtr.Zero, (IntPtr)command);
            }
            else if (broadcast)
            {
                Process[] processes = Process.GetProcesses();
                foreach (Process p in processes)
                {
                    //make sure that explorer is not one of the windows?
                    if(! p.MainWindowHandle.Equals(new IntPtr(0))){
                        capture = WinAPI.SendMessage(p.MainWindowHandle, WinAPI.SENDMESSAGE._MSG.WM_APPCOMMAND, p.MainWindowHandle, (IntPtr)command);
                    }
                }
            }
            else
            {
                IntPtr hWnd = WinAPI.GetTopWindow(IntPtr.Zero);
                IntPtr lasthWnd = WinAPI.GetWindow(IntPtr.Zero, (uint)WinAPI.GET_WINDOW_CMD.GW_HWNDLAST);
                while (((capture = WinAPI.SendMessage(hWnd, WinAPI.SENDMESSAGE._MSG.WM_APPCOMMAND, hWnd, (IntPtr)command)) != new IntPtr(1))
                        && (lasthWnd != hWnd))
                {
                    hWnd = WinAPI.GetWindow(hWnd, (uint)WinAPI.GET_WINDOW_CMD.GW_HWNDPREV);
                }
                // at this point capture should be the very last window in the z-order. One final SnedMessage to this window is neccissary because
                // the loop will exist before
                // result 0 means processed message : http://msdn.microsoft.com/en-us/library/windows/desktop/ms646275(v=vs.85).aspx

            }
            if (capture == new IntPtr(1))
            {
                return FormatInvokeSuccess();
            }
            else
            {
                return FormatInvokeFailure();
            }
        }

    }
}
