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
    public class ShowWindowTool : ATool
    {
        private WinAPI.SHOW_WINDOW_NCMDSHOW nCmdShow;
        public WinAPI.SHOW_WINDOW_NCMDSHOW NCmdShow
        {
            set
            {
                nCmdShow = value;
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

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            IntPtr hWnd;

            if (ishWndArgument)
            {
                try
                {
                    hWnd = new IntPtr(Convert.ToInt32(arguments[HWNDARGUMENTNAME]));
                    WinAPI.ShowWindow(hWnd, nCmdShow);
                }
                catch (FormatException) { return FormatInvokeFailure(nCmdShow.ToString()); }
            }

            return FormatInvokeSuccess(nCmdShow.ToString());
        }
    }
}
