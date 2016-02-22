using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Controls;

using ModServer;

namespace ToolBox
{
    [Serializable()]
    public abstract class ATool
    {
        protected static String INVOKE_SUCCESS_FORMAT = "The command {0} executed successfully.";
        protected static String INVOKE_FAILURE_FORMAT = "The command {0} failed!";
        
        //[NonSerialized()] public Grid InitializationGrid;
        private string html;
        public String ComponentHtml;
        private string javascript;
        public String Javascript;
        private string name;
        public String Name{
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }

        // pass the values from POST submission
        public abstract HttpResponse Invoke(Dictionary<String, String> arguments, ClientContainer client);


        protected HttpResponse FormatInvokeSuccess(string command)
        {
            HttpResponse r = new HttpResponse(HttpResponse.ConnectionStatus.OK, "keep-alive", null);
            r.Body = String.Format(INVOKE_SUCCESS_FORMAT, command);
            r.guessContentLength();
            return r;
        }

        protected HttpResponse FormatInvokeSuccess()
        {
            HttpResponse r = new HttpResponse(HttpResponse.ConnectionStatus.OK, "keep-alive", null);
            r.Body = String.Format(INVOKE_SUCCESS_FORMAT, "has");
            r.guessContentLength();
            return r;
        }


        protected HttpResponse FormatInvokeFailure(string command)
        {
            HttpResponse r = new HttpResponse(HttpResponse.ConnectionStatus.FORBIDDEN, "keep-alive", null);
            r.Body = String.Format(INVOKE_FAILURE_FORMAT, command);
            r.guessContentLength();
            return r;
        }

        protected HttpResponse FormatInvokeFailure()
        {
            HttpResponse r = new HttpResponse(HttpResponse.ConnectionStatus.FORBIDDEN, "keep-alive", null);
            r.Body = String.Format(INVOKE_FAILURE_FORMAT, "has");
            r.guessContentLength();
            return r;
        }

        protected HttpResponse getBasicResponse()
        {
            return new HttpResponse(HttpResponse.ConnectionStatus.OK, "keep-alive", null);
        }

    }
}
