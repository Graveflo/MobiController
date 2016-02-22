using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

using ModServer;
using ToolBox;
using WinAPIWrapper;

namespace Tools
{
    [Serializable()]
    public class SuspendStateTool : ATool
    {
        public enum STATE { SLEEP, HIBERNATE };
        private STATE state;

        public STATE State
        {
            get { return state; }
            set { state = value; }
        }

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            switch (state)
            {
                case STATE.HIBERNATE:
                    WinAPI.SetSuspendState(true, true, true);
                    break;
                case STATE.SLEEP:
                    WinAPI.SetSuspendState(false, true, true);
                    break;
            }
            return FormatInvokeSuccess();
        }
    }
}
