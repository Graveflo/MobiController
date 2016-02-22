using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using ModServer;
using WinAPIWrapper;
using ToolBox;

namespace Tools
{
    [Serializable()]
    public class SendMessageTool : ATool
    {
        private int command;
        public int wParam
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
        public int LParam
        {
            set
            {
                lParam = new IntPtr(value);
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

        private WinAPI.SENDMESSAGE._MSG message;
        public WinAPI.SENDMESSAGE._MSG Message { get { return message; } set { message = value; } }

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

            //IntPtr capture = (IntPtr)WinAPI.SENDMESSAGE._RETURN.WM_COPYDATA; //Set to this for the end condition

            if (ishWndArgument)
            {
                try
                {
                    hWnd = new IntPtr(Convert.ToInt32(arguments[HWNDARGUMENTNAME]));
                }
                catch (FormatException) { }
                WinAPI.SendMessage(hWnd, message, (IntPtr)command, lParam);
                return FormatInvokeSuccess();
            }
            else
            {
                IntPtr lasthWnd = WinAPI.GetWindow(IntPtr.Zero, (uint)WinAPI.GET_WINDOW_CMD.GW_HWNDLAST);
                while ((!(capture = WinAPI.SendMessage(hWnd, message, (IntPtr)command, lParam)).Equals(new IntPtr(0)))
                    && (lasthWnd != hWnd))
                {
                    hWnd = WinAPI.GetWindow(hWnd, (uint)WinAPI.GET_WINDOW_CMD.GW_HWNDPREV);
                }
                // many commands return 0 for processes
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
