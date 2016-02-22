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
    public class FileListerAndFormattingTool : ATool
    {
        private string pathVariable;
        public string PathVaraible
        {
            set
            {
                pathVariable = value;
            }
        }
        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            string path;
            StringBuilder returnTable = new StringBuilder();
            HttpResponse r;

            if (arguments == null || arguments.Count < 1)
            {
                foreach (DriveInfo d in DriveInfo.GetDrives())
                {
                    returnTable.AppendLine("<li><a onclick='AbsoluteDir(\"" + d.Name.Replace('\\', '/').Replace("/", "") + "\");'><img src=\"http://mobicontroller.com/images/hdd.png\" />" + d.Name + "</a></li>");
                }
                r = getBasicResponse();
                r.Body = returnTable.ToString();
                r.guessContentLength();
                return r; // dont try and parse out non-existant path
                //return returnTable.ToString();
            }
            try
            {
                path = ((string)arguments[pathVariable]).Replace('/', '\\').Trim();
                if (path.Equals("\\") || path.Equals(""))
                {
                    foreach (DriveInfo d in DriveInfo.GetDrives())
                    {
                        returnTable.AppendLine("<li><a onclick='AbsoluteDir(\"" + d.Name.Replace('\\', '/').Replace("/", "") + "\");'><img src=\"http://mobicontroller.com/images/hdd.png\" />" + d.Name + "</a></li>");
                    }
                }
                else
                {
                    string asdf = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    DirectoryInfo thisDir;
                    try
                    {
                        thisDir = new DirectoryInfo(path); //PATHS CAN BE CLICKED TWICE AND MESS THE WHOLE THING UP!
                    }
                    catch (Exception)
                    {
                        return FormatInvokeFailure();
                    }
                    try
                    {
                        foreach (DirectoryInfo d in thisDir.EnumerateDirectories())
                        {
                            try
                            {
                                d.EnumerateDirectories();
                                returnTable.AppendLine("<li style=\"height:80%;\"><a onclick='AscendDir(\"" + d.Name.Replace('\\', '/') + "\");'><img class=\"ui-li-icon\" src=\"http://mobicontroller.com/images/folder.ico\" />" + d.Name + "</a></li>");
                            }
                            catch (UnauthorizedAccessException)
                            { }
                        }
                        foreach (FileInfo f in thisDir.EnumerateFiles())
                        {
                            returnTable.AppendLine("<li><a onclick='od(\"" + f.Name + "\");' href='#fOptions' data-rel=\"popup\" >" + f.Name + "</a></li>");
                        }
                    }
                    catch (IOException)
                    {
                        r = FormatInvokeFailure();
                        r.Body += " The resource was unavialable.";
                        r.guessContentLength();

                        return r;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                r = FormatInvokeFailure();
                r.Body += " Access denied.";
                r.guessContentLength();

                return r;
            }
            r = getBasicResponse();
            r.Body = returnTable.ToString();
            r.guessContentLength();
            return r;
        }
    }
}
