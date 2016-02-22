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
    public class PathTool : ATool
    {
        public enum PATH { USER_PROFILE, MY_DOCUMENTS, RECENT, FAVORITES, DESKTOP };
        private PATH returnPath;

        public PATH ReturnPath
        {
            get { return returnPath; }
            set { returnPath = value; }
        }

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            HttpResponse thisResponse = getBasicResponse();

            switch (returnPath)
            {
                case PATH.DESKTOP:
                    thisResponse.Body = Environment.GetFolderPath(Environment.SpecialFolder.Desktop).Replace('\\', '/') ;
                    break;
                case PATH.FAVORITES:
                    thisResponse.Body =  Environment.GetFolderPath(Environment.SpecialFolder.Favorites).Replace('\\', '/') ;
                    break;
                case PATH.MY_DOCUMENTS:
                    thisResponse.Body = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace('\\', '/');
                    break;
                case PATH.RECENT:
                    thisResponse.Body =  Environment.GetFolderPath(Environment.SpecialFolder.Recent).Replace('\\', '/') ;
                    break;
                case PATH.USER_PROFILE:
                    thisResponse.Body = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace('\\', '/');
                    break;
            }
            thisResponse.guessContentLength();

            return thisResponse;
        }
    }
}
