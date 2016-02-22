using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

using ModServer;
using ToolBox;

namespace Tools
{
    [Serializable()]
    public class FileExecTool : ATool
    {

        private string filePathVariable;
        public string FilePathVariable
        {
            set
            {
                filePathVariable = value;
            }
        }

        private string fileNameVariable;
        public string FileNameVariable
        {
            set
            {
                fileNameVariable = value;
            }
        }

        private HelperClass helper;

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            HttpResponse response;
            helper = new HelperClass(client.getClient().GetStream(), client.getClient());

            if (arguments == null || arguments.Count < 1 || !arguments.ContainsKey(filePathVariable) || !arguments.ContainsKey(fileNameVariable))
            {
                procedureFail(client);
                return FormatInvokeFailure();
            }
            string path = (arguments[filePathVariable]).Replace('/', '\\').Trim();
            if(!path.EndsWith("\\"))
            {
                path += '\\';
            }
            path += arguments[fileNameVariable];
            try
            {
                FileInfo thisfile = new FileInfo(path);
                Process p = new Process() { StartInfo = new ProcessStartInfo(thisfile.FullName) };
                if (p.Start())
                {
                    return FormatInvokeSuccess();
                }
                else
                {
                    return FormatInvokeFailure();
                }
                //helper.SocketWriteLine("");
            }
            catch (IOException)
            {
                return procedureFail(client);
            }
            catch (UnauthorizedAccessException)
            {
                return procedureFail(client);
            }

            return null; // do not send out a response from the engine
        }

        private HttpResponse procedureFail(ClientContainer client)
        {
            HttpResponse response = new HttpResponse(HttpResponse.ConnectionStatus.NO_CONTENT, "keep-alive", null);
            response.addHeader("Content-Length", "0");

            return response;
        }
    }
}