using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace WinAPIWrapper
{

    public static class WinAPI
    {
        public static IntPtr HWND_BROADCAST = new IntPtr(0xffff);
        public struct SENDMESSAGE
        {
            public enum _MSG : int { WM_KEYDOWN = 0x0100, WM_KEYUP = 0x0101, WM_SYSCOMMAND = 0x0112, WM_MOUSEMOVE = 0x0200, WM_APPCOMMAND = 0x319 };
            public enum _RETURN : int { WM_COPYDATA = 0x004A };
            public struct WM_APPCOMMAND
            {
                //public enum LPARAM : int { APPCOMMAND_BASS_BOOST = 20, APPCOMMAND_BASS_DOWN = 19, APPCOMMAND_BASS_UP = 21, APPCOMMAND_BROWSER_BACKWARD = 1, APPCOMMAND_BROWSER_FAVORITES = 6, APPCOMMAND_BROWSER_FORWARD = 2, APPCOMMAND_BROWSER_HOME = 7, APPCOMMAND_BROWSER_REFRESH = 3, APPCOMMAND_BROWSER_SEARCH = 5, APPCOMMAND_BROWSER_STOP = 4, APPCOMMAND_CLOSE = 31, APPCOMMAND_COPY = 36, APPCOMMAND_CORRECTION_LIST = 45, APPCOMMAND_CUT = 37, APPCOMMAND_DICTATE_OR_COMMAND_CONTROL_TOGGLE = 43, APPCOMMAND_FIND = 28, APPCOMMAND_FORWARD_MAIL = 40, APPCOMMAND_HELP = 27, APPCOMMAND_LAUNCH_APP1 = 17, APPCOMMAND_LAUNCH_APP2 = 18, APPCOMMAND_LAUNCH_MAIL = 15, APPCOMMAND_LAUNCH_MEDIA_SELECT = 16, APPCOMMAND_MEDIA_CHANNEL_DOWN = 52, APPCOMMAND_MEDIA_CHANNEL_UP = 51, APPCOMMAND_MEDIA_FAST_FORWARD = 49, APPCOMMAND_MEDIA_NEXTTRACK = 11, APPCOMMAND_MEDIA_PAUSE = 47, APPCOMMAND_MEDIA_PLAY = 46, APPCOMMAND_MEDIA_PLAY_PAUSE = 14, APPCOMMAND_MEDIA_PREVIOUSTRACK = 12, APPCOMMAND_MEDIA_RECORD = 48, APPCOMMAND_MEDIA_REWIND = 50, APPCOMMAND_MEDIA_STOP = 13, APPCOMMAND_MIC_ON_OFF_TOGGLE = 44, APPCOMMAND_MICROPHONE_VOLUME_DOWN = 25, APPCOMMAND_MICROPHONE_VOLUME_MUTE = 24, APPCOMMAND_MICROPHONE_VOLUME_UP = 26, APPCOMMAND_NEW = 29, APPCOMMAND_OPEN = 30, APPCOMMAND_PASTE = 38, APPCOMMAND_PRINT = 33, APPCOMMAND_REDO = 35, APPCOMMAND_REPLY_TO_MAIL = 39, APPCOMMAND_SAVE = 32, APPCOMMAND_SEND_MAIL = 41, APPCOMMAND_SPELL_CHECK = 42, APPCOMMAND_TREBLE_DOWN = 22, APPCOMMAND_TREBLE_UP = 23, APPCOMMAND_UNDO = 34, APPCOMMAND_VOLUME_DOWN = 9, APPCOMMAND_VOLUME_MUTE = 8, APPCOMMAND_VOLUME_UP = 10 };
                public enum LPARAM : int { APPCOMMAND_BASS_BOOST = 1310720, APPCOMMAND_BASS_DOWN = 1245184, APPCOMMAND_BASS_UP = 1376256, APPCOMMAND_BROWSER_BACKWARD = 65536, APPCOMMAND_BROWSER_FAVORITES = 393216, APPCOMMAND_BROWSER_FORWARD = 131072, APPCOMMAND_BROWSER_HOME = 458752, APPCOMMAND_BROWSER_REFRESH = 196608, APPCOMMAND_BROWSER_SEARCH = 327680, APPCOMMAND_BROWSER_STOP = 262144, APPCOMMAND_CLOSE = 2031616, APPCOMMAND_COPY = 2359296, APPCOMMAND_CORRECTION_LIST = 2949120, APPCOMMAND_CUT = 2424832, APPCOMMAND_DICTATE_OR_COMMAND_CONTROL_TOGGLE = 2818048, APPCOMMAND_FIND = 1835008, APPCOMMAND_FORWARD_MAIL = 2621440, APPCOMMAND_HELP = 1769472, APPCOMMAND_LAUNCH_APP1 = 1114112, APPCOMMAND_LAUNCH_APP2 = 1179648, APPCOMMAND_LAUNCH_MAIL = 983040, APPCOMMAND_LAUNCH_MEDIA_SELECT = 1048576, APPCOMMAND_MEDIA_CHANNEL_DOWN = 3407872, APPCOMMAND_MEDIA_CHANNEL_UP = 3342336, APPCOMMAND_MEDIA_FAST_FORWARD = 3211264, APPCOMMAND_MEDIA_NEXTTRACK = 720896, APPCOMMAND_MEDIA_PAUSE = 3080192, APPCOMMAND_MEDIA_PLAY = 3014656, APPCOMMAND_MEDIA_PLAY_PAUSE = 917504, APPCOMMAND_MEDIA_PREVIOUSTRACK = 786432, APPCOMMAND_MEDIA_RECORD = 3145728, APPCOMMAND_MEDIA_REWIND = 3276800, APPCOMMAND_MEDIA_STOP = 851968, APPCOMMAND_MIC_ON_OFF_TOGGLE = 2883584, APPCOMMAND_MICROPHONE_VOLUME_DOWN = 1638400, APPCOMMAND_MICROPHONE_VOLUME_MUTE = 1572864, APPCOMMAND_MICROPHONE_VOLUME_UP = 1703936, APPCOMMAND_NEW = 1900544, APPCOMMAND_OPEN = 1966080, APPCOMMAND_PASTE = 2490368, APPCOMMAND_PRINT = 2162688, APPCOMMAND_REDO = 2293760, APPCOMMAND_REPLY_TO_MAIL = 2555904, APPCOMMAND_SAVE = 2097152, APPCOMMAND_SEND_MAIL = 2686976, APPCOMMAND_SPELL_CHECK = 2752512, APPCOMMAND_TREBLE_DOWN = 1441792, APPCOMMAND_TREBLE_UP = 1507328, APPCOMMAND_UNDO = 2228224, APPCOMMAND_VOLUME_DOWN = 589824, APPCOMMAND_VOLUME_MUTE = 524288, APPCOMMAND_VOLUME_UP = 655360 };
                public enum UDEVICE : int { FAPPCOMMAND_KEY = 0, FAPPCOMMAND_MOUSE = 0x8000, FAPPCOMMAND_OEM = 0x1000 };
                public enum DWKEYS : int { MK_CONTROL = 0x0008, MK_LBUTTON = 0x0001, MK_MBUTTON = 0x0010, MK_RBUTTON = 0x0002, MK_SHIFT = 0x0004, MK_XBUTTON1 = 0x0020, MK_XBUTTON2 = 0x0040 };
            }
            public struct WM_MOUSEMOVE
            {
                public enum WPARAM : int { MK_CONTROL = 0x0008, MK_LBUTTON = 0x0001, MK_MBUTTON = 0x0010, MK_RBUTTON = 0x0002, MK_SHIFT = 0x0004, MK_XBUTTON1 = 0x0020, MK_XBUTTON2 = 0x0040 };
                public struct lParam //must be a pointer to a long low order mouse x high order mouse y
                {
                }
            }
        }

        public struct SET_WINDOW_POS
        {
            public enum hWndInsertAfter : int { HWND_BOTTOM = 1, HWND_NOTOPMOST = -2, HWND_TOP = 0, HWND_TOPMOST = -1 };
            [Flags]
            public enum uFlags : uint { SWP_ASYNCWINDOWPOS = 0x4000, SWP_DEFERERASE = 0x2000, SWP_DRAWFRAME = 0x0020, SWP_FRAMECHANGED = 0x0020, SWP_HIDEWINDOW = 0x0080, SWP_NOACTIVATE = 0x0010, SWP_NOCOPYBITS = 0x0100, SWP_NOMOVE = 0x0002, SWP_NOOWNERZORDER = 0x0200, SWP_NOREDRAW = 0x0008, SWP_NOREPOSITION = 0x0200, SWP_NOSENDCHANGING = 0x0400, SWP_NOSIZE = 0x0001, SWP_NOZORDER = 0x0004, SWP_SHOWWINDOW = 0x0040 };
        }

        public struct GET_WINDOW_LONG
        {
            public const int GWL_EXSTYLE = -20;
            public enum GWL_EXSTYLE_RETURN : long { WS_EX_ACCEPTFILES = 0x00000010L, WS_EX_APPWINDOW = 0x00040000L, WS_EX_CLIENTEDGE = 0x00000200L, WS_EX_COMPOSITED = 0x02000000L, WS_EX_CONTEXTHELP = 0x00000400L, WS_EX_CONTROLPARENT = 0x00010000L, WS_EX_DLGMODALFRAME = 0x00000001L, WS_EX_LAYERED = 0x00080000, WS_EX_LAYOUTRTL = 0x00400000L, WS_EX_LEFT = 0x00000000L, WS_EX_LEFTSCROLLBAR = 0x00004000L, WS_EX_LTRREADING = 0x00000000L, WS_EX_MDICHILD = 0x00000040L, WS_EX_NOACTIVATE = 0x08000000L, WS_EX_NOINHERITLAYOUT = 0x00100000L, WS_EX_NOPARENTNOTIFY = 0x00000004L, WS_EX_NOREDIRECTIONBITMAP = 0x00200000L, WS_EX_RIGHT = 0x00001000L, WS_EX_RIGHTSCROLLBAR = 0x00000000L, WS_EX_RTLREADING = 0x00002000L, WS_EX_STATICEDGE = 0x00020000L, WS_EX_TOOLWINDOW = 0x00000080L, WS_EX_TOPMOST = 0x00000008L, WS_EX_TRANSPARENT = 0x00000020L, WS_EX_WINDOWEDGE = 0x00000100L };
        }

        public struct FLASH_WINDOW
        {
            //Flash both the window caption and taskbar button.
            //This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags. 
            public const UInt32 FLASHW_ALL = 3;

            // Flash continuously until the window comes to the foreground. 
            public const UInt32 FLASHW_TIMERNOFG = 12;

            [StructLayout(LayoutKind.Sequential)]
            public struct FLASHWINFO
            {
                public UInt32 cbSize;
                public IntPtr hwnd;
                public UInt32 dwFlags;
                public UInt32 uCount;
                public UInt32 dwTimeout;
            }
            public static FLASHWINFO createFlashWindowInfo(IntPtr hWnd)
            {
                FLASHWINFO s = new FLASHWINFO();
                s.cbSize = Convert.ToUInt32(Marshal.SizeOf(s));
                s.hwnd = hWnd;
                s.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
                s.uCount = UInt32.MaxValue;
                s.dwTimeout = 0;
                //change this to take argument of enums if needed
                return s;
            }
        }

        public enum SHOW_WINDOW_NCMDSHOW : int { SW_FORCEMINIMIZE = 11, SW_HIDE = 0, SW_MAXIMIZE = 3, SW_MINIMIZE = 6, SW_RESTORE = 9, SW_SHOW = 5, SW_SHOWDEFAULT = 10, SW_SHOWMAXIMIZED = 3, SW_SHOWMINIMIZED = 2, SW_SHOWMINNOACTIVE = 7, SW_SHOWNA = 8, SW_SHOWNOACTIVATE = 4, SW_SHOWNORMAL = 1 };
        [Flags]
        public enum ANIMATE_WINDOW_FLAGS : uint { AW_ACTIVATE = 0x00020000, AW_BLEND = 0x00080000, AW_CENTER = 0x00000010, AW_HIDE = 0x00010000, AW_HOR_POSITIVE = 0x00000001, AW_HOR_NEGATIVE = 0x00000002, AW_SLIDE = 0x00040000, AW_VER_POSITIVE = 0x00000004, AW_VER_NEGATIVE = 0x00000008 };

        // WM_SYSCOMMAND lParam:
        //The low-order word specifies the horizontal position of the cursor, in screen coordinates, if a window menu command is chosen with the mouse. Otherwise, this parameter is not used.
        //The high-order word specifies the vertical position of the cursor, in screen coordinates, if a window menu command is chosen with the mouse. This parameter is –1 if the command is chosen using a system accelerator, or zero if using a mnemonic.
        public enum WM_SYSCOMMAND_WPARAM : int { SC_CLOSE = 0xF060, SC_CONTEXTHELP = 0xF180, SC_DEFAULT = 0xF160, SC_HOTKEY = 0xF150, SC_HSCROLL = 0xF080, SCF_ISSECURE = 0x00000001, SC_KEYMENU = 0xF100, SC_MAXIMIZE = 0xF030, SC_MINIMIZE = 0xF020, SC_MONITORPOWER = 0xF170, SC_MOUSEMENU = 0xF090, SC_MOVE = 0xF010, SC_NEXTWINDOW = 0xF040, SC_PREVWINDOW = 0xF050, SC_RESTORE = 0xF120, SC_SCREENSAVE = 0xF140, SC_SIZE = 0xF000, SC_TASKLIST = 0xF130, SC_VSCROLL = 0xF070 };


        [Flags]
        public enum EXIT_WINDOWS_EXT_FLAGS : uint { EWX_HYBRID_SHUTDOWN = 0x00400000, EWX_LOGOFF = 0, EWX_POWEROFF = 0x00000008, EWX_REBOOT = 0x00000002, EWX_RESTARTAPPS = 0x00000040, EWX_SHUTDOWN = 0x00000001, EWX_FORCE = 0x00000004, EWX_FORCEIFHUNG = 0x00000010 };
        [Flags]
        public enum EXIT_WINDOWS_EXT_REASONS : uint { SHTDN_REASON_MAJOR_APPLICATION = 0x00040000, SHTDN_REASON_MAJOR_HARDWARE = 0x00010000, SHTDN_REASON_MAJOR_LEGACY_API = 0x00070000, SHTDN_REASON_MAJOR_OPERATINGSYSTEM = 0x00020000, SHTDN_REASON_MAJOR_OTHER = 0x00000000, SHTDN_REASON_MAJOR_POWER = 0x00060000, SHTDN_REASON_MAJOR_SOFTWARE = 0x00030000, SHTDN_REASON_MAJOR_SYSTEM = 0x00050000, SHTDN_REASON_MINOR_BLUESCREEN = 0x0000000F, SHTDN_REASON_MINOR_CORDUNPLUGGED = 0x0000000b, SHTDN_REASON_MINOR_DISK = 0x00000007, SHTDN_REASON_MINOR_ENVIRONMENT = 0x0000000c, SHTDN_REASON_MINOR_HARDWARE_DRIVER = 0x0000000d, SHTDN_REASON_MINOR_HOTFIX = 0x00000011, SHTDN_REASON_MINOR_HOTFIX_UNINSTALL = 0x00000017, SHTDN_REASON_MINOR_HUNG = 0x00000005, SHTDN_REASON_MINOR_INSTALLATION = 0x00000002, SHTDN_REASON_MINOR_MAINTENANCE = 0x00000001, SHTDN_REASON_MINOR_MMC = 0x00000019, SHTDN_REASON_MINOR_NETWORK_CONNECTIVITY = 0x00000014, SHTDN_REASON_MINOR_NETWORKCARD = 0x00000009, SHTDN_REASON_MINOR_OTHER = 0x00000000, SHTDN_REASON_MINOR_OTHERDRIVER = 0x0000000e, SHTDN_REASON_MINOR_POWER_SUPPLY = 0x0000000a, SHTDN_REASON_MINOR_PROCESSOR = 0x00000008, SHTDN_REASON_MINOR_RECONFIG = 0x00000004, SHTDN_REASON_MINOR_SECURITY = 0x00000013, SHTDN_REASON_MINOR_SECURITYFIX = 0x00000012, SHTDN_REASON_MINOR_SECURITYFIX_UNINSTALL = 0x00000018, SHTDN_REASON_MINOR_SERVICEPACK = 0x00000010, SHTDN_REASON_MINOR_SERVICEPACK_UNINSTALL = 0x00000016, SHTDN_REASON_MINOR_TERMSRV = 0x00000020, SHTDN_REASON_MINOR_UNSTABLE = 0x00000006, SHTDN_REASON_MINOR_UPGRADE = 0x00000003, SHTDN_REASON_MINOR_WMI = 0x00000015, SHTDN_REASON_FLAG_USER_DEFINED = 0x40000000, SHTDN_REASON_FLAG_PLANNED = 0x80000000 };

        public enum OPEN_PROCESS_TOKEN_ACCESS : short { TOKEN_ADJUST_PRIVILEGES = 32, TOKEN_QUERY = 8 };

        public const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        public const short SE_PRIVILEGE_ENABLED = 2;
        public struct LUID
        {
            public int LowPart;
            public int HighPart;
        }
        public struct LUID_AND_ATTRIBUTES
        {
            public LUID pLuid;
            public int Attributes;
        }
        public struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }
        public enum GET_WINDOW_CMD : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        public struct KEYBD_EVENT
        {
            [Flags]
            public enum dwFlags : uint { KEYEVENTF_EXTENDEDKEY = 0x001, KEYEVENTF_KEYUP = 0x0002 };
        }

        public struct MOUSE_EVENT
        {
            [Flags]
            public enum dwFlags : uint { MOUSEEVENTF_ABSOLUTE = 0x8000, MOUSEEVENTF_LEFTDOWN = 0x0002, MOUSEEVENTF_LEFTUP = 0x0004, MOUSEEVENTF_MIDDLEDOWN = 0x0020, MOUSEEVENTF_MIDDLEUP = 0x0040, MOUSEEVENTF_MOVE = 0x0001, MOUSEEVENTF_RIGHTDOWN = 0x0008, MOUSEEVENTF_RIGHTUP = 0x0010, MOUSEEVENTF_WHEEL = 0x0800, MOUSEEVENTF_XDOWN = 0x0080, MOUSEEVENTF_XUP = 0x0100, MOUSEEVENTF_HWHEEL = 0x01000 };
            public enum cButtons : uint { XBUTTON1 = 0x0001, XBUTTON2 = 0x0002 };
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(MOUSE_EVENT.dwFlags dwFlags, int dx, int dy, int cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern short VkKeyScan(char ch);
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, KEYBD_EVENT.dwFlags dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetTopWindow(IntPtr hWnd);
        [DllImport("User32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlashWindowEx(ref FLASH_WINDOW.FLASHWINFO pwfi);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UpdateWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern long GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, SET_WINDOW_POS.hWndInsertAfter hWndInsertAfter, int x, int Y, int cx, int cy, SET_WINDOW_POS.uFlags wFlags);
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32")]
        public static extern bool AnimateWindow(IntPtr hwnd, uint time, ANIMATE_WINDOW_FLAGS flags);
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, SHOW_WINDOW_NCMDSHOW nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool BringWindowToTop(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("advapi32.dll")]
        public static extern int OpenProcessToken(IntPtr ProcessHandle,
                             int DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
            [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState,
            UInt32 BufferLength,
            IntPtr PreviousState,
            IntPtr ReturnLength);

        [DllImport("advapi32.dll")]
        public static extern int LookupPrivilegeValue(string lpSystemName,
                               string lpName, out LUID lpLuid);
        [DllImport("User32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, SENDMESSAGE._MSG Msg,
            IntPtr wParam, IntPtr lParam);
        [DllImport("User32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, int Msg,
            IntPtr wParam, IntPtr lParam);
        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

    }
}
