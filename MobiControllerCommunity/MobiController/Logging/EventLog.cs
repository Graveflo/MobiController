using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MobiController
{
    public class EventLog
    {
        public static string LOG_FILE = "_log.txt";
        public delegate void LogNewEvent(Event loggedEvent);
        public static event LogNewEvent LoggedNewEvent;
        protected ICollection<Event> events;


        public virtual bool shouldWriteOut(string message, Event.EVENT_FLAGS Flags)
        {
            return (Flags & Event.EVENT_FLAGS.NOLOG) != Event.EVENT_FLAGS.NOLOG;
        }

        public virtual ICollection<Event> LogEvents
        {
            get
            {
                return events;
            }
        }

        public void writeLogOut(string text)
        {
            StreamWriter output;
            if (File.Exists(LOG_FILE))
            {
                output = File.AppendText(LOG_FILE);
            }
            else
            {
                output = File.CreateText(LOG_FILE);
            }
            try
            {
                output.WriteLine(text);
                output.Close();
            }
            catch (IOException)
            {
                logEvent("ERROR WRITING TO LOGFILE. ERROR HANDLING IS NOT WORKING CORRECTLY.", Event.EVENT_FLAGS.ERROR | Event.EVENT_FLAGS.CRITICAL);
            }
        }

        public EventLog()
        {
            events = new HashSet<Event>();
        }

        public virtual void logEvent(String messageBody, Event.EVENT_FLAGS Flags)
        {
            if ((Flags & Event.EVENT_FLAGS.DEBUG) != 0)
            {
                //This is a debug message
#if DEBUG
                if (!(messageBody[messageBody.IndexOf("/") + 1] == 's'))
                {
                    logEvent(messageBody, Flags ^ Event.EVENT_FLAGS.DEBUG);
                }
#endif
            }
            else
            {
                Event thisevent = new Event(messageBody, Flags);
                if (shouldWriteOut(messageBody, Flags))
                {
                    writeLogOut(thisevent.ToString());
                }
                events.Add(thisevent);
                if (LoggedNewEvent != null)
                {
                    LoggedNewEvent(thisevent);
                }
            }
        }
        public virtual void logEvent(String[] messageBody, Event.EVENT_FLAGS Flags)
        {
            StringBuilder total = new StringBuilder();
            for (int i = 0; i < messageBody.Length; i++)
            {
                total.AppendLine(messageBody[i]);
            }
            logEvent(total.ToString(), Flags);
        }
    }
}
