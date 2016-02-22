using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using ModServer;
using ToolBox;
using WinAPIWrapper;

namespace Tools
{
    [Serializable()]
    public class SysCommandTool : ATool
    {
        private WinAPI.WM_SYSCOMMAND_WPARAM command;
        public WinAPI.WM_SYSCOMMAND_WPARAM Command
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
        public IntPtr lParam = IntPtr.Zero;
        public IntPtr LParam
        {
            set
            {
                lParam = value;
            }
        }
        public const string HWNDARGUMENTNAME = "hWnd";
        private bool ishWndArgument = false;
        public bool IsHWndArgument
        {
            set
            {
                ishWndArgument = value;
            }
        }

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            IntPtr capture;
            IntPtr hWnd;
            if (broadcast)
            {
                hWnd = WinAPI.HWND_BROADCAST;
            }
            else
            {
                hWnd = WinAPI.GetTopWindow(IntPtr.Zero);
                //hWnd = Process.GetProcessesByName("explorer")[0].Handle;
                //hWnd = WinAPI.GetDesktopWindow();
            }

            //int capture = WinAPI.SENDMESSAGE._RETURN.WM_COPYDATA; //Set to this for the end condition

            if (ishWndArgument)
            {
                try
                {
                    hWnd = new IntPtr(Convert.ToInt32(arguments[HWNDARGUMENTNAME]));
                }
                catch (FormatException) { }
                WinAPI.SendMessage(hWnd, WinAPI.SENDMESSAGE._MSG.WM_SYSCOMMAND, (IntPtr)command, lParam);
                return FormatInvokeSuccess();
            }
            else
            {
                IntPtr lasthWnd = WinAPI.GetWindow(IntPtr.Zero, (uint)WinAPI.GET_WINDOW_CMD.GW_HWNDLAST);
                while ((!(capture = (WinAPI.SendMessage(hWnd, WinAPI.SENDMESSAGE._MSG.WM_SYSCOMMAND, (IntPtr)command, lParam))).Equals(new IntPtr(0)))
                        && (lasthWnd != hWnd))
                {
                    hWnd = WinAPI.GetWindow(hWnd, (uint)WinAPI.GET_WINDOW_CMD.GW_HWNDPREV);
                }
                //http://msdn.microsoft.com/en-us/library/windows/desktop/ms646275(v=vs.85).aspx 1 is TRUE
                if (capture.Equals(new IntPtr(0)))
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
}
