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
    public class ScreenServingTool : ATool
    {
        public enum FileFormat { PNG, JPG };
        private FileFormat fileType = FileFormat.PNG;

        public FileFormat FileType
        {
            get { return fileType; }
            set { fileType = value; }
        }

        private string keyCenter;
        public string KeyCenter
        {
            get { return keyCenter; }
            set { keyCenter = value; }
        }

        private string keyCorner;
        public string KeyCorner
        {
            get { return keyCorner; }
            set { keyCorner = value; }
        }

        private string keyDemensions;
        public string KeyDemensions
        {
            get { return keyDemensions; }
            set { keyDemensions = value; }
        }

        public static Rectangle screenDem = new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            Stream pipe = client.getClient().GetStream();
            HttpResponse thisResponse = new HttpResponse(HttpResponse.ConnectionStatus.OK, "keep-alive", null);
            var helper = new HelperClass(pipe, client.getClient());

            var imageHolder = new MemoryStream();

            Rectangle rect = screenDem; // get screen resolution

            if (keyCenter != null)
            {
                if (!arguments.ContainsKey(keyCenter))
                {
                    return FormatInvokeFailure();
                }
                if (!arguments.ContainsKey(keyDemensions))
                {
                    return FormatInvokeFailure();
                }

                string Size = arguments[keyDemensions];
                string Center = arguments[keyCenter];
                int imageHeight, imageWidth;
                int centerX, centerY;
                try
                {
                    string[] peices = Size.Split(',');
                    imageWidth = Convert.ToInt32(peices[0]);
                    imageHeight = Convert.ToInt32(peices[1]);

                    peices = Center.Split(',');
                    centerX = Convert.ToInt32(peices[0]);
                    centerY = Convert.ToInt32(peices[1]);
                }
                catch (FormatException)
                { return FormatInvokeFailure(); }

                double scale = rect.Width / Convert.ToDouble(imageWidth);

                int realX = Convert.ToInt32((centerX * scale) - (imageWidth / 2));
                int realY = Convert.ToInt32((centerY * scale) - (imageHeight / 2));

                int xbound = rect.Width - imageWidth;
                int ybound = rect.Height - imageHeight;

                if (realX > xbound)
                    realX = xbound;
                if (realY > ybound)
                    realY = ybound;
                if (realX < 0)
                    realX = 0;
                if (realY < 0)
                    realY = 0;

                rect = new Rectangle(realX, realY, imageWidth, imageHeight);
            }
            if (keyCorner != null)
            {
                int cornerX, cornerY;
                int imageHeight, imageWidth;
                try
                {
                    string[] peices = arguments[keyCorner].Split(',');
                    cornerX = Convert.ToInt32(peices[0]);
                    cornerY = Convert.ToInt32(peices[1]);

                    peices = arguments[keyDemensions].Split(',');
                    imageWidth = Convert.ToInt32(peices[0]);
                    imageHeight = Convert.ToInt32(peices[1]);
                }
                catch (FormatException)
                { return FormatInvokeFailure(); }
                catch (KeyNotFoundException)
                { return FormatInvokeFailure(); }

                rect = new Rectangle(cornerX, cornerY, imageWidth, imageHeight);
            }

            var bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb); // needs updated rect
            Graphics g = Graphics.FromImage(bmp);
            try
            {
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                SizeF PainInTheNeck = g.MeasureString("Error", SystemFonts.DefaultFont);
                g = Graphics.FromImage((bmp = new Bitmap((int)PainInTheNeck.Width, (int)PainInTheNeck.Height, PixelFormat.Format32bppArgb)));
                g.DrawString("Error", SystemFonts.DefaultFont, Brushes.Red, new PointF(0, 0));
            }
            switch (FileType)
            {
                case FileFormat.JPG:
                    bmp.Save(imageHolder, ImageFormat.Jpeg);
                    thisResponse.addHeader("Content-Type", "image/png");
                    break;
                case FileFormat.PNG:
                    bmp.Save(imageHolder, ImageFormat.Png);
                    thisResponse.addHeader("Content-Type", "image/png");
                    break;
            }

            imageHolder.Position = 0;

            try
            {
                thisResponse.addHeader("Content-Length", imageHolder.Length.ToString());
                helper.SocketWriteLine(thisResponse.ToString());
                imageHolder.CopyTo(pipe);
            }
            catch (IOException) { }
            return null;
        }
    }
}
