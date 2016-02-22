using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModServer;
using ToolBox;
using WinAPIWrapper;

namespace Tools
{
    [Serializable()]
    public class SetWindowPosTool : ATool
    {
        private WinAPI.SET_WINDOW_POS.hWndInsertAfter hWndInsertAfter = WinAPI.SET_WINDOW_POS.hWndInsertAfter.HWND_TOPMOST;
        public WinAPI.SET_WINDOW_POS.hWndInsertAfter HWndInsertAfter
        {
            set
            {
                hWndInsertAfter = value;
            }
        }
        private WinAPI.SET_WINDOW_POS.uFlags uFlags = WinAPI.SET_WINDOW_POS.uFlags.SWP_ASYNCWINDOWPOS | WinAPI.SET_WINDOW_POS.uFlags.SWP_NOSIZE;
        public WinAPI.SET_WINDOW_POS.uFlags UFlags
        {
            set
            {
                uFlags = value;
            }
        }

        public const string HWNDARGUMENTNAME = "hWnd";
        private bool ishWndArgument = true;
        public bool IsHWndArgument
        {
            set
            {
                ishWndArgument = value;
            }
        }

        private IntPtr hWnd;
        public IntPtr HWnd
        {
            set
            {
                hWnd = value;
            }
        }

        private int x;

        public int X
        {
            get { return x; }
            set { x = value; }
        }
        private int y;

        public int Y
        {
            get { return y; }
            set { y = value; }
        }
        private int cx;

        public int Cx
        {
            get { return cx; }
            set { cx = value; }
        }
        private int cy;

        public int Cy
        {
            get { return cy; }
            set { cy = value; }
        }

        public override HttpResponse Invoke(Dictionary<string,string> arguments, ClientContainer client)
        {
            if (ishWndArgument)
            {
                try
                {
                    hWnd = new IntPtr(Convert.ToInt32(arguments[HWNDARGUMENTNAME]));
                }
                catch (FormatException) { return FormatInvokeFailure(); }
            }

            WinAPI.SetWindowPos(hWnd, hWndInsertAfter, x, y, cx, cy, uFlags);

            return FormatInvokeSuccess();
        }
    }
}
