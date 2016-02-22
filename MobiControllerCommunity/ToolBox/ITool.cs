using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBox
{
    public interface ITool
    {
        String Invoke(Dictionary<String, String> arguments);
        String getComponentHtml();
    }
}
