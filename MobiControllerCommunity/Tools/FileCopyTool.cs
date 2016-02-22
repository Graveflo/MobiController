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
    public class FileCopyTool : ATool
    {

        private string destPathVariable;
        public string DestPathVariable
        {
            set
            {
                destPathVariable = value;
            }
        }

        private string filePathVariable;
        public string FilePathVariable
        {
            set
            {
                filePathVariable = value;
            }
        }

        private bool isMove=false;
        public bool IsMove
        {
            set
            {
                isMove = value;
            }
        }

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            if (arguments == null || !arguments.ContainsKey(filePathVariable) || !arguments.ContainsKey(destPathVariable))
            {
                procedureFail(client);
                return FormatInvokeFailure();
            }

            string src = (arguments[filePathVariable]).Replace('/', '\\').Trim();
            string dest = (arguments[destPathVariable]).Replace('/', '\\').Trim();

            string[] filename = src.Split('\\');

            try
            {
                File.Copy(src, dest+filename[filename.Length-1]);
                if (isMove)
                {
                    File.Delete(src);
                }
            }
            catch (IOException)
            {
                return procedureFail(client);
            }
            catch (UnauthorizedAccessException)
            {
                return procedureFail(client);
            }

            return FormatInvokeSuccess(); // do not send out a response from the engine
        }

        private HttpResponse procedureFail(ClientContainer client)
        {
            HttpResponse response = new HttpResponse(HttpResponse.ConnectionStatus.NO_CONTENT, "keep-alive", null);
            response.addHeader("Content-Length", "0");

            return response;
        }
    }
}