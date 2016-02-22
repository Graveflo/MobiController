using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

using ToolBox;
using WinAPIWrapper;
using ModServer;

namespace Tools
{
    [Serializable()]
    public class MouseEventTool : ATool
    {
        private WinAPI.MOUSE_EVENT.dwFlags flags;
        public WinAPI.MOUSE_EVENT.dwFlags Flags
        {
            set
            {
                flags = value;
            }
        }

        private WinAPI.MOUSE_EVENT.cButtons button = 0;
        public WinAPI.MOUSE_EVENT.cButtons Button
        {
            set
            {
                button = value;
            }
        }

        private int posx;
        public int Posx
        {
            get { return posx; }
            set { posx = value; }
        }

        private int posy;
        public int Posy
        {
            get { return posy; }
            set { posy = value; }
        }

        private string posxKey;
        public string PosxKey
        {
            get { return posxKey; }
            set { posxKey = value; }
        }

        private string posyKey;
        public string PosyKey
        {
            get { return posyKey; }
            set { posyKey = value; }
        }

        private string wheelDeltaKey;
        public string WheelDeltaKey
        {
            get { return wheelDeltaKey; }
            set { wheelDeltaKey = value; }
        }

        public const int MAX_POS = 65535;
        public const int WHEEL_DELTA_CLICK = 120;

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            int dwdata = (int)button;
            try
            {
                if (posxKey != null)
                {
                    posx = Convert.ToInt32(Math.Round(Convert.ToDouble(arguments[PosxKey])));
                }
                if (posyKey != null)
                {
                    posy = Convert.ToInt32(Math.Round(Convert.ToDouble(arguments[PosyKey])));
                }
            }
            catch (OverflowException)
            {
                return FormatInvokeFailure();
            }
            catch (FormatException)
            {
                return FormatInvokeFailure();
            }
            if (wheelDeltaKey != null)
            {
                try
                {
                    dwdata = Convert.ToInt32(arguments[wheelDeltaKey]) * WHEEL_DELTA_CLICK;
                }
                catch (KeyNotFoundException)
                {
                    return FormatInvokeFailure();
                }
                catch (FormatException)
                {
                    return FormatInvokeFailure();
                }
            }else{
            }

            if ((flags & WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_ABSOLUTE) == WinAPI.MOUSE_EVENT.dwFlags.MOUSEEVENTF_ABSOLUTE)
            {
                int xscale = MAX_POS / Screen.PrimaryScreen.Bounds.Width;
                int yscale = MAX_POS / Screen.PrimaryScreen.Bounds.Height;
                posx *= xscale;
                posy *= yscale;
            }

            WinAPI.mouse_event(flags, posx, posy, dwdata, 0);

            return FormatInvokeSuccess();
        }
    }
}
