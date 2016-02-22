using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

using ToolBox;
using ModServer;

namespace Tools
{
    [Serializable()]
    public class ScreenResQueryTool : ATool
    {
        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            HttpResponse response = FormatInvokeSuccess();
            response.Body = Screen.PrimaryScreen.Bounds.Width + "," + Screen.PrimaryScreen.Bounds.Height;
            response.guessContentLength();

            return response;
        }
    }
}
