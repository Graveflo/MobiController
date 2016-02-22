using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ToolBox;
using WinAPIWrapper;
using ModServer;

namespace Tools
{
    [Serializable()]
    public class AnimateWindowTool : ATool
    {
        private WinAPI.ANIMATE_WINDOW_FLAGS flags;
        public WinAPI.ANIMATE_WINDOW_FLAGS Flags
        {
            set
            {
                flags = value;
            }
        }

        public uint time = 200;
        public uint TimeMilli
        {
            set
            {
                time = value;
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
                    WinAPI.AnimateWindow(hWnd, time, flags);
                }
                catch (FormatException) { return FormatInvokeFailure(flags.ToString()); }
            }

            return FormatInvokeSuccess(flags.ToString());
        }
    }
}
