using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiController.Controllers
{
    public class ControllerException : Exception
    {
        [Flags]public enum REASON { BAD_REMOTE=1, OLD_VERSION= 2, UNSUPPORTED_VERSION=4, KEY_NOT_FOUND=8, MISSING_MODULE=16 };
        private REASON thisreason;
        public REASON Reason
        {
            get
            {
                return thisreason;
            }
        }
        public ControllerException(string message, REASON r)
            : base(message)
        {
            thisreason = r;
        }
    }
}
