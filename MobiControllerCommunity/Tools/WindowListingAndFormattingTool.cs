using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using ModServer;
using WinAPIWrapper;
using ToolBox;

namespace Tools
{
    [Serializable()]
    public class WindowListingAndFormattingTool : ATool
    {
        private static ConcurrentDictionary<IntPtr, string> Windows;

        public WindowListingAndFormattingTool() : base()
        {
            Windows = new ConcurrentDictionary<IntPtr, string>();
        }
        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            Windows = new ConcurrentDictionary<IntPtr, string>(); //cannot serialize static
            WinAPI.EnumWindows(EnumTheWindows, IntPtr.Zero);
            StringBuilder returnList = new StringBuilder();
            foreach (KeyValuePair<IntPtr, string> kp in Windows)
            {
                returnList.AppendLine("<li><a onclick='windowPanel(" + kp.Key.ToString() + ")'>"+kp.Value+"</a></li>");
            }
            HttpResponse r = getBasicResponse();
            r.Body = returnList.ToString();
            r.guessContentLength();

            return r;
        }
        public static bool EnumTheWindows(IntPtr hWnd, IntPtr lParam)
        {
            int size = WinAPI.GetWindowTextLength(hWnd);
            if (size++ > 0 && WinAPI.IsWindowVisible(hWnd))
            {
                StringBuilder sb = new StringBuilder(size);
                WinAPI.GetWindowText(hWnd, sb, size);
                Windows.TryAdd(hWnd,sb.ToString());
            }
            return true;
        }
    }
}
