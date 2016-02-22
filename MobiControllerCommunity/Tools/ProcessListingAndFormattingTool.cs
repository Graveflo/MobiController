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
    public class ProcessListingAndFormattingTool: ATool
    {

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            StringBuilder procTable = new StringBuilder();
            Array.ForEach(Process.GetProcesses(), (Process p) => procTable.AppendLine("<li><a>" + p.ProcessName + "</a><a onclick=\'$.post(\"/pkill\",\"pid=" + p.Id.ToString() + "\",function(){location.reload(true);})'>Kill</a></li>"));

            HttpResponse r = getBasicResponse();
            r.Body = procTable.ToString();
            r.guessContentLength();

            return r;
        }
    }
}
