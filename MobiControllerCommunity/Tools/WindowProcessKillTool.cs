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
    public class WindowProcessKillTool : ATool
    {
        private static string hWndID = "hWnd";

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            if (arguments.ContainsKey(hWndID))
            {
                try
                {
                    uint PID = 0;
                    WinAPI.GetWindowThreadProcessId(new IntPtr(Convert.ToInt32(arguments[hWndID])), out PID);
                    Process.GetProcessById((int)PID).Kill();

                    return FormatInvokeSuccess("Kill " + arguments[hWndID]);
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
