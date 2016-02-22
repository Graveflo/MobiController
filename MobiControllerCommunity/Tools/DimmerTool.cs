using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

using ModServer;
using ToolBox;

namespace Tools
{
    [Serializable()]
    public class DimmerTool : ATool
    {
        private byte dimmAmt;

        public byte DimmAmt
        {
            get { return dimmAmt; }
            set { dimmAmt = value; }
        }

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            using (ManagementObjectSearcher searcher =
    new ManagementObjectSearcher(new ManagementScope("root\\WMI"), new SelectQuery("WmiMonitorBrightnessMethods")))
            {
                using (ManagementObjectCollection objectCollection = searcher.Get())
                {
                    try
                    {
                        foreach (ManagementObject mObj in objectCollection)
                        {
                            mObj.InvokeMethod("WmiSetBrightness",
                                new Object[] { UInt32.MaxValue, dimmAmt });
                            break;
                        }
                    }
                    catch (ManagementException) { }
                }
            }
            return FormatInvokeSuccess();
        }
    }
}
