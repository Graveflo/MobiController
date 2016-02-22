using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiController
{
    public class Event
    {
        public static String DELIMETER = " | ";//char.ConvertFromUtf32(2);
        [Flags]
        public enum EVENT_FLAGS { NORMAL = 0x1, ERROR = 0x2, IMPORTANT = 0x4, CRITICAL = 0x8, DEBUG = 0x10, NOLOG=0x20 };
        private String message;
        private String group;
        private DateTime time;
        private EVENT_FLAGS flags;

        public Event previous;
        public Event next;

        public String Message
        {
            get
            {
                return message;
            }
        }
        public String Flags
        {
            get
            {
                return flags.ToString();
            }
        }
        public String EventType
        {
            get
            {
                return group;
            }
        
        }
        public String Time
        {
            get
            {
                return time.ToString();
            }
        }

        public Event(String message, EVENT_FLAGS flags)
        {
            time = DateTime.Now;
            this.message = message;
            this.flags = flags;
            this.group = "Default";
        }
        public Event(String message, EVENT_FLAGS flags, String group) : this(message,flags)
        {
            this.group = group;
        }

        public override String ToString()
        {
            return time + DELIMETER + "(" + flags + ")" + DELIMETER + group + DELIMETER + message;
        }
    }
}
