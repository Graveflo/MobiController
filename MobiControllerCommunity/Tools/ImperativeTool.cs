using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using ModServer;
using ToolBox;

namespace Tools
{
    [Serializable()]
    public class ImperativeTool : ATool
    {
        private ATool[] tools;

        public ATool[] Tools
        {
            get { return tools; }
            set { tools = value; }
        }

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            foreach (ATool thisTool in tools)
            {
                thisTool.Invoke(arguments, client);
            }

            return FormatInvokeSuccess(); // do not send out a response from the engine
        }
    }
}