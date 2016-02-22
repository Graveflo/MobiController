using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using ModServer;
using ToolBox;
using WinAPIWrapper;

namespace Tools
{
    [Serializable()]
    public class ExitWindowsTool : ATool
    {
        private bool force;
        public bool Force
        {
            set
            {
                force = value;
            }
        }
        private WinAPI.EXIT_WINDOWS_EXT_FLAGS command;
        public WinAPI.EXIT_WINDOWS_EXT_FLAGS Command
        {
            set
            {
                command = value;
            }
        }
        public override HttpResponse Invoke(Dictionary<string, string> arguments, ClientContainer client)
        {
            IntPtr TokenProcessHandle;
            WinAPI.OpenProcessToken(Process.GetCurrentProcess().Handle, (short)(WinAPI.OPEN_PROCESS_TOKEN_ACCESS.TOKEN_ADJUST_PRIVILEGES | WinAPI.OPEN_PROCESS_TOKEN_ACCESS.TOKEN_QUERY), out TokenProcessHandle);
            WinAPI.TOKEN_PRIVILEGES tkp;
            tkp.PrivilegeCount = 1;
            tkp.Privileges.Attributes = WinAPI.SE_PRIVILEGE_ENABLED;
            WinAPI.LookupPrivilegeValue("", WinAPI.SE_SHUTDOWN_NAME, out tkp.Privileges.pLuid);
            WinAPI.AdjustTokenPrivileges(TokenProcessHandle, false, ref tkp, 0U, IntPtr.Zero, IntPtr.Zero);

            bool result;
            if (force)
            {
                foreach (Process p in Process.GetProcesses())
                {
                    try
                    {
                        if (p.SessionId != Process.GetCurrentProcess().SessionId)
                        {
                            p.Kill();
                        }
                    }
                    catch (Exception) { }
                }
                result = WinAPI.ExitWindowsEx((uint)command, 0) &&
                        WinAPI.ExitWindowsEx((uint)WinAPI.EXIT_WINDOWS_EXT_FLAGS.EWX_FORCEIFHUNG, 0);
            }
            else
            {
                result = WinAPI.ExitWindowsEx((uint)command, 0);
            }
            if (result)
            {
                return FormatInvokeSuccess();
            }
            else
            {
                return FormatInvokeFailure();
            }
        }
    }
}
