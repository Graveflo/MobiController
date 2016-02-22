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
    public class ShellTool : ATool
    {
        private string path;
        private string arguemnts;
        private bool elevatePrivileges;
        public bool ElevatePrivileges
        {
            set
            {
                elevatePrivileges = value;
            }
        }
        private ProcessStartInfo startInfo;
        public ProcessStartInfo StartInfo
        {
            set
            {
                startInfo = value;
            }
        }

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            Process newProcess = new Process();
            newProcess.StartInfo = startInfo;
            if (elevatePrivileges)
            {
                newProcess.StartInfo.Verb = "runas";
            }
            newProcess.Start();
            return FormatInvokeSuccess();
        }
    }
}
