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
    public class FileServingTool : ATool
    {

        private string filePathVariable;
        public string FilePathVariable
        {
            set
            {
                filePathVariable = value;
            }
        }

        private bool attatchment;
        public bool isAttatchment
        {
            set
            {
                attatchment = value;
            }
        }

        private HelperClass helper;

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            HttpResponse response;
            helper = new HelperClass(client.getClient().GetStream(), client.getClient());

            if (arguments == null || arguments.Count < 1 || !arguments.ContainsKey(filePathVariable))
            {
                procedureFail(client);
                return FormatInvokeFailure();
            }
            string path = ((string)arguments[filePathVariable]).Replace('/', '\\').Trim();
            response = new HttpResponse(HttpResponse.ConnectionStatus.OK, "keep-alive", null);
            if (attatchment)
            {
                response.addHeader("Content-Disposition", "attachment; filename=\"" + new FileInfo(path).Name + '"');
            }
            try
            {
                FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                response.addHeader("Content-Length", file.Length.ToString());
                helper.SocketWriteLine(response.ToString());
                file.CopyTo(client.getClient().GetStream());
                file.Close();
                //helper.SocketWriteLine("");
            }
            catch (IOException ex)
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