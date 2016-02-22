using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.IO;

using MobiControllerBlackBox.Controllers;

namespace MobiController
{
    public class ControllerInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private string description;

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        private string imageSource;
        public string ImageSource
        {
            get { return imageSource; }
            set { imageSource = value; }
        }

        private string lastupdated;
        public string Lastupdated
        {
            get { return lastupdated; }
            set { lastupdated = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public bool isfree;

        public ControllerInfo(bool isFree)
        {
            this.isfree = isFree;
        }
        private int progress;

        public int Progress
        {
            get { return progress; }
            set { progress = value; NotifyPropertyChanged("Progress"); }
        }

        private string imageuri;
        public String ImageURI
        {
            get
            {
                if (Progress==FileLen)
                    return "/MobiController;component/Resources/check.ico";
                else
                {
                    return imageuri;
                }
            }
            set
            {
                imageuri = value;
                NotifyPropertyChanged("ImageURI");
            }
        }

        public string Total
        {
            get
            {
                return Progress.ToString() + "/" + FileLen.ToString();
            }
        }

        private int fileLen;
        public int FileLen
        {
            get { return fileLen; }
            set { fileLen = value; }
        }

        private string fileName;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        private FileInfo path;
        public FileInfo Path
        {
            get { return path; }
            set { path = value; }
        }

        public static List<ControllerInfo> getInstalledControllers(string path)
        {
            List<ControllerInfo> installedControllers = new List<ControllerInfo>();
            DirectoryInfo controllerDir;
            if (Directory.Exists(App.CONTROLLER_DIR))
            {
                controllerDir = new DirectoryInfo(App.CONTROLLER_DIR);
            }
            else
            {
                controllerDir = Directory.CreateDirectory(App.CONTROLLER_DIR);
            }
            foreach (FileInfo file in controllerDir.GetFiles())
            {
                try
                {
                    String[] metaData = Controller.getControllerMeta(file);
                    if (file.FullName.EndsWith("ermt"))
                    {
                        installedControllers.Add(new ControllerInfo(false) { Name = metaData[0], Description = metaData[1], Path = file, FileName=file.Name.Split('.')[0] });
                    }
                    else
                    {
                        installedControllers.Add(new ControllerInfo(true) { Name = metaData[0], Description = metaData[1], Path = file, FileName = file.Name.Split('.')[0] });
                    }
                }
                catch (ControllerException ex)
                {
                    throw new ControllerInfoException(file, ex.Message, ex.Reason);
                }
            }
            return installedControllers;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (!(obj is ControllerInfo))
            {
                return false;
            }
            ControllerInfo compareme = (ControllerInfo)obj;

            try
            {
                return (compareme.FileName.Equals(FileName) ||
                        (compareme.Name.Equals(Name)));
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }
    }
}
