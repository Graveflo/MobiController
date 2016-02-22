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
    public class FileUploadHandlerTool : ATool
    {

        private string filePathVariable;
        public string FilePathVariable
        {
            set
            {
                filePathVariable = value;
            }
        }

        private int timeout;
        public int TimeOut
        {
            set
            {
                timeout = value;
            }
        }

        private HelperClass helper;

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            Stream inStream = client.getClient().GetStream();

            if (arguments == null || arguments.Count < 1 || !arguments.ContainsKey(filePathVariable))
            {
                return FormatInvokeFailure();
            }
            string path = ((string)arguments[filePathVariable]).Replace('/', '\\').Trim();
            FileStream filestream;
            if (File.Exists(path))
            {
                return FormatInvokeFailure(",because of an invalid path, has");
            }
            try
            {
                int contentlen = 0;
                try
                {
                    contentlen = Convert.ToInt32(client.CurrentRequest.requestMetaInfo("Content-Length").Trim());
                }
                catch (KeyNotFoundException)
                {
                    return FormatInvokeFailure(", because content-length is not supported by this browser, has");
                }
                int bytesReceived = 0;

                string boundary = client.CurrentRequest.requestMetaInfo("Content-Type").Split(new string[] { "boundary=" }, StringSplitOptions.None)[1];
                String strMultiPart = client.CurrentRequest.MessageBody;
                byte[] multiPartRAW = client.CurrentRAW;

                byte[] buffer = new byte[1024];
                bool isChunked = client.CurrentRequest.MessageBody.Length < 5;
                if (isChunked) // the multipart form data was not included in the origonal data
                {
                    int endread = inStream.Read(buffer, 0, buffer.Length);
                    multiPartRAW = new byte[endread];
                    for (int i = 0; i < endread; i++)
                    {
                        multiPartRAW[i] = buffer[i];
                    }
                    strMultiPart = Encoding.UTF8.GetString(multiPartRAW);
                }

                string filename = HelperClass.getIsolatedString("filename=\"", "\"", strMultiPart);//client.CurrentRequest.ToString().Split(new string[] { "filename=\"", "\"" },StringSplitOptions.None)[0];

                if (!path.EndsWith("\\"))
                {
                    path += '\\';
                }

                filestream = File.Create(path + filename);

                int startfileindex = strMultiPart.IndexOf("\r\n\r\n") + "\r\n\r\n".Length;
                int sy = strMultiPart.IndexOf(boundary, startfileindex + 1);
                int filelen = sy - startfileindex;
                filelen -= Environment.NewLine.Length; // there is a line break right before the boundary
                filelen -= "--".Length; // this has to be there for some reason !!!!!!re-examine later

                bytesReceived += startfileindex + 1; // include the multipart headers/junk all the way up to the begining of the file binary
                if (!isChunked)
                {
                    startfileindex += client.CurrentRequest.HeaderLength; //needed to set the offset for the httprequest RAW
                }

                // if the end of the file is not in the request
                if (filelen < 1)
                {
                    filelen = multiPartRAW.Length - startfileindex; //until the end of the buffer
                }
                else
                {
                    bytesReceived = contentlen; // make sure the loop does not go through. All the file is here.
                }

                filestream.Write(multiPartRAW, startfileindex, filelen);

                bytesReceived += filelen;
                //bytesReceived += (Environment.NewLine.Length * 2) + boundary.Length;

                byte[] boundaryBytes = Encoding.UTF8.GetBytes(boundary);
                // REMEMBER that the first character can randomly come before the actual boundary making fail

                List<int> boundaryCounters = new List<int>();

                buffer = new byte[1048576];
                bool eof = false;
                while (bytesReceived < contentlen && !eof)
                {
                    int read = inStream.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < read; i++)
                    {
                        for (int j = 0; j < boundaryCounters.Count; j++)
                        {
                            int boundaryCounter = boundaryCounters[j];
                            if (buffer[i] == boundaryBytes[boundaryCounter])
                            {
                                boundaryCounters.Remove(boundaryCounter);
                                boundaryCounter++;
                                boundaryCounters.Insert(j, boundaryCounter);
                                if (boundaryCounter >= boundaryBytes.Length)
                                {
                                    eof = true;
                                    break;
                                }
                            }
                            else
                            {
                                boundaryCounters.Remove(boundaryCounter);
                                j--;
                            }
                        }
                        if (buffer[i] == boundaryBytes[0])
                        {
                            boundaryCounters.Add(1);
                        }
                        if (eof) //this is here to prevent the last bad bit from being added
                        {
                            break; // will cause buffer[i] == boundaryBytes[boundaryCounter] to have index out of bounds exception
                        }
                        else
                        {
                            filestream.WriteByte(buffer[i]);
                        }
                        bytesReceived++;
                    }
                    filestream.Flush();
                }
                if (eof)
                {
                    filestream.SetLength(filestream.Length - (boundary.Length + Environment.NewLine.Length + 1));
                }
                inStream.Flush();
                filestream.Close();
            }
            catch (IOException)
            {
                return FormatInvokeFailure(", because of a file/permission error, has");
            }

            HttpResponse r = new HttpResponse(HttpResponse.ConnectionStatus.FOUND, "keep-alive", null);
            r.addHeader("Content-Length", "0");
            r.addHeader("Location", client.CurrentRequest.requestMetaInfo("Referer"));
            return r;
        }

        public static void writeMultiFormFile(byte[] boundary, byte[] buffer, int bufferLen, Stream fileout)
        {
            if (boundary.Length < 1)
            {
                throw new ArgumentException("Must have a bounary.");
            }
            for (int i = 0; i < bufferLen - (boundary.Length - 1); i++)
            { // no need to check if the size left is smaller then the boundary
                if (buffer[i] == boundary[0])
                {
                    if (checkForBoundary(i + 1, buffer, boundary)) // zero indicates nothing found because the boundary must be longer then 1 character
                    {
                        return;
                    }
                }
                fileout.WriteByte(buffer[i]);
            }
            // if at the end the boundary is not found write out the remaining bytes
            for (int i = bufferLen - (boundary.Length - 1); i < bufferLen; i++)
            {
                fileout.WriteByte(buffer[i]);
            }
        }
        private static bool checkForBoundary(int startIndex, byte[] buffer, byte[] boundary)
        {
            int i;
            for (i = 1; i < boundary.Length; i++)
            {
                if (boundary[i] != buffer[startIndex + i])
                {
                    return false;
                }
            }
            return true; // if the boundary passes all the way through return the end of the index
        }
    }
}
