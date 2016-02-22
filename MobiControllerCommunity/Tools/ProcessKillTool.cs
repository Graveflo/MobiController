using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using ModServer;
using ToolBox;

namespace Tools
{
    [Serializable()]
    public class ProcessKillTool : ATool
    {
        private static string pidID = "pid";

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            if (arguments.ContainsKey(pidID))
            {
                try
                {
                    int PID = Convert.ToInt32(arguments[pidID]);
                    try
                    {
                        Process.GetProcessById(PID).Kill();
                    }
                    catch (ArgumentException)
                    {
                        return FormatInvokeSuccess("Could not kill " + arguments[pidID] + " because it is already running.");
                    }
                    return FormatInvokeSuccess("Killed " + arguments[pidID]);
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
