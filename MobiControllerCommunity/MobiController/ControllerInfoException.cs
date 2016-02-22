using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MobiControllerBlackBox.Controllers;

namespace MobiController
{
    public class ControllerInfoException : ControllerException
    {
        private FileInfo controller;

        public FileInfo Controller
        {
            get { return controller; }
            set { controller = value; }
        }


        public ControllerInfoException(FileInfo controller, string message, REASON reason)
            : base(message, reason)
        {
            this.controller = controller;
        }
    }
}
