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
    public class KeyPressTool : ATool
    {
        private byte keyVal;
        public byte KeyVal
        {
            get { return keyVal; }
            set { keyVal = value; }
        }

        private string strKey;
        public string StrKey
        {
            get { return strKey; }
            set { strKey = value; }
        }

        private string strBackKey;
        public string StrBackKey
        {
            get { return strBackKey; }
            set { strBackKey = value; }
        }

        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            if (strKey == null)
            {
                WinAPIWrapper.WinAPI.keybd_event(keyVal, 0, 0, 0);
                return FormatInvokeSuccess();
            }
            else
            {
                if (arguments.ContainsKey(strKey))
                {
                    string thisString = arguments[strKey];
                    if (thisString.Length < 1)
                    {
                        return FormatInvokeSuccess();
                    }
                    for (int i = 0; i < thisString.Length; i++)
                    {
                        char thisChar = thisString[i];
                        bool wasShift = true;
                        switch (thisChar)
                        {
                            case '!':
                                break;
                            case '@':
                                break;
                            case '#':
                                break;
                            case '$':
                                break;
                            case '%':
                                break;
                            case '^':
                                break;
                            case '&':
                                break;
                            case '*':
                                break;
                            case '(':
                                break;
                            case ')':
                                break;
                            case '_':
                                break;
                            case '+':
                                break;
                            case '{':
                                break;
                            case '}':
                                break;
                            case ':':
                                break;
                            case '"':
                                break;
                            case '<':
                                break;
                            case '>':
                                break;
                            case '?':
                                break;
                            default:
                                wasShift = (thisChar >= 'A') && (thisChar <= 'Z');
                                break;
                        }
                        if (wasShift)
                        {
                            WinAPIWrapper.WinAPI.keybd_event((byte)System.Windows.Forms.Keys.ShiftKey, 0, 0, 0);
                        }
                        WinAPIWrapper.WinAPI.keybd_event((byte)WinAPI.VkKeyScan(thisChar), 0, 0, 0);
                        if (wasShift)
                        {
                            WinAPIWrapper.WinAPI.keybd_event((byte)System.Windows.Forms.Keys.ShiftKey, 0, WinAPI.KEYBD_EVENT.dwFlags.KEYEVENTF_KEYUP, 0);
                        }
                    }
                    return FormatInvokeSuccess();
                }
                else if (arguments.ContainsKey(strBackKey))
                {
                    string thisString = arguments[strBackKey];
                    if (thisString.Length < 1)
                    {
                        return FormatInvokeSuccess();
                    }
                    int thisLen;
                    try
                    {
                        thisLen = Convert.ToInt32(thisString);
                    }
                    catch (FormatException)
                    {
                        thisLen = 0;
                    }
                    for (int i = 0; i < thisLen; i++)
                    {
                        WinAPIWrapper.WinAPI.keybd_event((byte)(System.Windows.Forms.Keys.Back), 0, 0, 0);
                    }
                    return FormatInvokeSuccess();
                }
                else
                {
                    return FormatInvokeFailure();
                }
            }
        }
    }
}
