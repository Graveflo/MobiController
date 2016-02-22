using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

using ModServer;
using ToolBox;
using WinAPIWrapper;

namespace Tools
{
    [Serializable()]
    public class KeyParseTool : ATool
    {
        private string strKey;
        public string StrKey
        {
            get { return strKey; }
            set { strKey = value; }
        }

        private char specialCharOpen = '{';
        public char SpecialCharOpen
        {
            get { return specialCharOpen; }
            set { specialCharOpen = value; }
        }

        private char specialCharClose = '}';
        public char SpecialCharClose
        {
            get { return specialCharClose; }
            set { specialCharClose = value; }
        }

        private char specialDown='>';
        public char SpecialDown
        {
          get { return specialDown; }
          set { specialDown = value; }
        }

        private char specialUp = '^';
        public char SpecialUp
        {
          get { return specialUp; }
          set { specialUp = value; }
        }

        // have characters added only of the keyup event when there are no other keys being pressed. The keydown event should have a buffer for a chain reaction.
        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            if (arguments.ContainsKey(strKey))
            {
                String parseme = arguments[strKey];
                StringBuilder currentKeyWord=null;
                int index = 0;
                char direction=(char)0;
                while(index<parseme.Length){
                    char thisChar = parseme[index];
                    if (currentKeyWord == null)
                    {
                        if (thisChar == specialDown || thisChar == specialUp)
                        {
                            direction = thisChar;
                        }
                        else if (thisChar == specialCharOpen)
                        {
                            currentKeyWord = new StringBuilder();
                        }
                        else
                        {
                            processRequest((byte)WinAPI.VkKeyScan(thisChar), direction);
                            direction = (char)0;
                        }
                    }else{
                        if (thisChar == specialCharClose)
                        {
                            processRequest(processKeyword(currentKeyWord.ToString()), direction);
                            currentKeyWord = null;
                            direction = (char)0;
                        }
                        else
                        {
                            currentKeyWord.Append(thisChar);
                        }
                    }
                    index++; 
                }
            }
            return FormatInvokeSuccess();
        }

        private void processRequest(byte key, char direction)
        {
            if (direction == SpecialUp)
            {
                WinAPIWrapper.WinAPI.keybd_event(key, 0, WinAPI.KEYBD_EVENT.dwFlags.KEYEVENTF_KEYUP, 0);
            }
            else if(direction == SpecialDown)
            {
                WinAPIWrapper.WinAPI.keybd_event(key, 0, 0, 0);
            }
            else
            {
                WinAPIWrapper.WinAPI.keybd_event(key, 0, 0, 0);
                WinAPIWrapper.WinAPI.keybd_event(key, 0, WinAPI.KEYBD_EVENT.dwFlags.KEYEVENTF_KEYUP, 0);
            }
        }

        private byte processKeyword(string keyword)
        {
            switch (keyword.ToUpper())
            {
                case "BS":
                    return (byte)System.Windows.Forms.Keys.Back;
                case "N":
                    return (byte)System.Windows.Forms.Keys.Enter;
                case "D":
                    return (byte)System.Windows.Forms.Keys.Down;
                case "U":
                    return (byte)System.Windows.Forms.Keys.Up;
                case "L":
                    return (byte)System.Windows.Forms.Keys.Left;
                case "R":
                    return (byte)System.Windows.Forms.Keys.Right;
                case "CAPS":
                    return (byte)System.Windows.Forms.Keys.CapsLock;
                case "DEL":
                    return (byte)System.Windows.Forms.Keys.Delete;
                case "END":
                    return (byte)System.Windows.Forms.Keys.End;
                case "ESC":
                    return (byte)System.Windows.Forms.Keys.Escape;
                case "HOME":
                    return (byte)System.Windows.Forms.Keys.Home;
                case "INS":
                    return (byte)System.Windows.Forms.Keys.Insert;
                case "LSHIFT":
                    return (byte)System.Windows.Forms.Keys.LShiftKey;
                case "RSHIFT":
                    return (byte)System.Windows.Forms.Keys.RShiftKey;
                case "CTRL":
                    return (byte)System.Windows.Forms.Keys.ControlKey;
                case "RCTRL":
                    return (byte)System.Windows.Forms.Keys.RControlKey;
                case "LCTRL":
                    return (byte)System.Windows.Forms.Keys.LControlKey;
                case "ALT":
                    return (byte)18;
                case "WIN":
                    return (byte)System.Windows.Forms.Keys.LWin;
                case "PD":
                    return (byte)System.Windows.Forms.Keys.PageDown;
                case "PU":
                    return (byte)System.Windows.Forms.Keys.PageUp;
                case "PS":
                    return (byte)System.Windows.Forms.Keys.PrintScreen;
                case "TAB":
                    return (byte)System.Windows.Forms.Keys.Tab;
                case "F1":
                    return (byte)System.Windows.Forms.Keys.F1;
                case "F2":
                    return (byte)System.Windows.Forms.Keys.F2;
                case "F3":
                    return (byte)System.Windows.Forms.Keys.F3;
                case "F4":
                    return (byte)System.Windows.Forms.Keys.F4;
                case "F5":
                    return (byte)System.Windows.Forms.Keys.F5;
                case "F6":
                    return (byte)System.Windows.Forms.Keys.F6;
                case "F7":
                    return (byte)System.Windows.Forms.Keys.F7;
                case "F8":
                    return (byte)System.Windows.Forms.Keys.F8;
                case "F9":
                    return (byte)System.Windows.Forms.Keys.F9;
                case "F10":
                    return (byte)System.Windows.Forms.Keys.F10;
                case "F11":
                    return (byte)System.Windows.Forms.Keys.F11;
                case "F12":
                    return (byte)System.Windows.Forms.Keys.F12;
            }
            return 0;
        }
    }
}
