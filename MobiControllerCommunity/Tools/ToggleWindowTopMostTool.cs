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
    public class ToggleWindowTopMostTool : ATool
    {
        private static string hWndID = "hWnd";

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            if (arguments.ContainsKey(hWndID))
            {
                try
                {
                    IntPtr hWnd = new IntPtr(Convert.ToInt32(arguments[hWndID]));
                    if ((WinAPI.GetWindowLong(hWnd, WinAPI.GET_WINDOW_LONG.GWL_EXSTYLE)
                        & (long)WinAPI.GET_WINDOW_LONG.GWL_EXSTYLE_RETURN.WS_EX_TOPMOST) == 0)
                    {
                        WinAPI.SetWindowPos(hWnd, WinAPI.SET_WINDOW_POS.hWndInsertAfter.HWND_TOPMOST, 0, 0, 0, 0, WinAPI.SET_WINDOW_POS.uFlags.SWP_NOMOVE | WinAPI.SET_WINDOW_POS.uFlags.SWP_NOSIZE);
                    }
                    else
                    {
                        WinAPI.SetWindowPos(hWnd, WinAPI.SET_WINDOW_POS.hWndInsertAfter.HWND_NOTOPMOST, 0, 0, 0, 0, WinAPI.SET_WINDOW_POS.uFlags.SWP_NOSIZE | WinAPI.SET_WINDOW_POS.uFlags.SWP_NOMOVE);
                    }

                    return FormatInvokeSuccess();
                }
                catch (FormatException) { return FormatInvokeFailure(); }
            }
            else
            {
                return FormatInvokeFailure();
            }
        }
    }
}
