using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBox
{
    public class ToolInvokeException : Exception
    {
        public ToolInvokeException(String message) : base(message) { }
    }
}
