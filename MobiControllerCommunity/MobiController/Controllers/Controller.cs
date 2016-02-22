using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.Serialization;

using MobiController.ToolBox;
using ModServer;
using MobiControllerBlackBox;

namespace MobiController.Controllers
{
    [Serializable()]
    public class Controller
    {
        private long htmlbodyStart;
        //private long pictureStart;
        private long toolChainStart;
        //private FileStream filestream;

        private static string tmpFileTail = ".~";

        public Dictionary<String, ATool> toolChain = new Dictionary<string, ATool>();

        private String Name;
        public String ControllerName
        {
            get { return Name; }
            set { Name = value; }
        }


        private String Description;
        public String ControllerDescription
        {
            get { return Description; }
            set { Description = value; }
        }


        private String html;
        public String ControllerHtml
        {
            get { return html; }
            set { html = value; }
        }


        private FileInfo filePath;
        public FileInfo FilePath
        {
            get
            {
                return filePath;
            }
        }

        public Dictionary<String, ATool> loadToolChain(Stream s, long toolChainStart)
        {
            StringBuilder html = new StringBuilder();

            CryptoStream cryptostream = BlackBox.getControllerReader(s);
            s.Position = toolChainStart;

            BinaryFormatter binf = new BinaryFormatter();
            try
            {
                return (Dictionary<String, ATool>)binf.Deserialize(cryptostream);
            }
            catch (SerializationException)
            {
                throw new ControllerException("Missing assembly required to load this Controller. Please install the appropriate plugin.", ControllerException.REASON.MISSING_MODULE);
            }
        }

        public bool loadController(Stream reader)
        {
            reader.Position = 0; // in case the stream was seeking
            htmlbodyStart = readBlock(reader);
            //pictureStart = readBlock(reader);
            toolChainStart = readBlock(reader);


            html = loadHtml(reader);
            toolChain = loadToolChain(reader);

            //reader.Close();
            return true;
        }

        public Dictionary<String, ATool> loadToolChain(Stream s)
        {
            CryptoStream cryptostream = cryptostream = BlackBox.getControllerReader(s); ;
            //if (FilePath.FullName.EndsWith("ermt"))
            //{
            //    cryptostream = BlackBox.getControllerReader(s,App.Config.username);
            //}
            //else
            //{
            //    cryptostream = BlackBox.getControllerReader(s);
            //}
            s.Position = toolChainStart;

            BinaryFormatter binf = new BinaryFormatter();
            try
            {
                return (Dictionary<String, ATool>)binf.Deserialize(cryptostream);
            }
            catch (SerializationException)
            {
                throw new ControllerException("Missing assembly required to load this Controller. Please install the appropriate plugin.", ControllerException.REASON.MISSING_MODULE);
            }
            catch (CryptographicException)
            {
                throw new ControllerException("Corrupted Controller.", ControllerException.REASON.BAD_REMOTE);
            }
        }

        public Controller(FileInfo file, bool metaOnly)
        {
            Stream s = File.OpenRead(file.FullName);

            filePath = file;
            String[] meta = getControllerMeta(s);
            Name = meta[0];
            Description = meta[1];

            if (! metaOnly)
            {
                loadController(s);
            }
            s.Close();
        }



        /// <summary>
        /// Used for the creation/design of controllers. No fields are set yet.
        /// Alos may be used to call the getControllerMeta method
        /// </summary>
        public Controller()
        {
            

        }

        public HttpResponse tryExec(String key, Dictionary<String, String> arguments, ClientContainer client)
        {
            if (toolChain.ContainsKey(key))
            {
                    return toolChain[key].Invoke(arguments,client);
            }
            else
            {
                throw new ToolInvokeException("The key was not found in the current loaded controllers toolchain.");
            }
        }

        private long readBlock(Stream s)
        {
            if (s.ReadByte() != 2)
            {
                throw new ControllerException("bad remote file!!", ControllerException.REASON.BAD_REMOTE);
            }
            byte[] tmp = new byte[8];
            s.Read(tmp, 0, 8);
            if (s.ReadByte() != 3)
            {
                throw new ControllerException("bad remote file!!", ControllerException.REASON.BAD_REMOTE);
            }
            return BitConverter.ToInt64(tmp, 0);
        }

        public String loadHtml(Stream s)
        {
            byte tmp;
            StringBuilder html = new StringBuilder();

            s.Position = htmlbodyStart;

            //int forDebug = s.ReadByte();
            if (s.ReadByte() != 02)
            {
                throw new ControllerException("bad remote file!!", ControllerException.REASON.BAD_REMOTE);
            }
            while ((tmp = (byte)s.ReadByte()) != 03)
            {
                html.Append(ASCIIEncoding.ASCII.GetString(new byte[] { tmp }));
            }
            return html.ToString();
        }

        public String loadHtml()
        {
            using (FileStream fs = File.OpenRead(FilePath.FullName))
                return loadHtml(fs);
        }



        public Dictionary<String, ATool> loadToolChain()
        {
            using (FileStream fs = File.OpenRead(FilePath.FullName))
                return loadToolChain(fs,toolChainStart);
        }

        public String[] getControllerMeta(Stream reader)
        {
            String[] meta = new String[] { "", "" };
            StringBuilder currentMeta = new StringBuilder();


            if (reader.ReadByte().Equals(2))
            {
                // place holder values accounted for
                reader.Position = (10 * 2); //must read the 02 or bad remote exception

                byte tmp;
                for (int i = 0; i < meta.Length; i++)
                {
                    currentMeta = new StringBuilder();
                    if (reader.ReadByte() != (byte)02)
                    {
                        throw new Exception("bad remote");
                    }
                    while ((tmp = (byte)reader.ReadByte()) != 03)
                    {
                        currentMeta.Append(ASCIIEncoding.ASCII.GetString(new byte[] { tmp }));
                    }
                    meta[i] = currentMeta.ToString();
                }
            }
            else
            {
                throw new ControllerException("bad remote file!!", ControllerException.REASON.BAD_REMOTE);
            }
            //reader.Close();
            return meta;
        }


        //Struc
        //html start
        //picture start
        //toolchain start
        //name
        //description
        //html
        //picture
        //toolchain
        public void saveController(String path, string key)
        {
            FileStream filewriter = File.Create(path + tmpFileTail);
            CryptoStream cryptostream = BlackBox.getControllerWriter(filewriter);
            //if (key.Equals(""))
            //{
            //    cryptostream = BlackBox.getControllerWriter(filewriter);
            //}
            //else
            //{
            //    cryptostream = new CryptoStream(filewriter, BlackBox.getCipher(App.Config.username).CreateEncryptor(), CryptoStreamMode.Write);
            //}

            Stream reader;
            Stream writer = filewriter;
            BinaryFormatter binwriter = new BinaryFormatter();

            //Dumping information into temporary file
            long placeHolder = 10 * 2;
            placeHolder += writeBlock(writer, Encoding.ASCII.GetBytes(Name));
            placeHolder += writeBlock(writer, Encoding.ASCII.GetBytes(Description));
            htmlbodyStart = placeHolder;
            placeHolder += writeBlock(writer, Encoding.UTF8.GetBytes(html));
            //pictureStart = placeHolder;
            //placeHolder += writeBlock(writer, picture);
            toolChainStart = placeHolder;
            //writer = cryptostream;
            binwriter.Serialize(cryptostream, toolChain);
            cryptostream.Close();
            filewriter.Close();

            //use information to construct file header
            reader = File.OpenRead(path + tmpFileTail);
            filewriter = File.Create(path);
            writer = filewriter;
            writeBlock(writer, BitConverter.GetBytes(htmlbodyStart));
            //writeBlock(writer, BitConverter.GetBytes(pictureStart));
            writeBlock(writer, BitConverter.GetBytes(toolChainStart));
            reader.CopyTo(writer);
            reader.Close();
            writer.Close();
            File.Delete(path + tmpFileTail);
        }

        public void saveController(String path)
        {
            saveController(path, "");
        }

        public long writeBlock(Stream s, Stream file)
        {
            s.WriteByte(02);
            file.CopyTo(s);
            s.WriteByte(03);
            return file.Length + 2;
        }

        public long writeBlock(Stream s, byte[] data)
        {
            s.WriteByte(02);
            s.Write(data, 0, data.Length);
            s.WriteByte(03);
            return data.Length + 2;
        }
    }
}
