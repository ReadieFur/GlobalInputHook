using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace GlobalInputHook
{
    internal class HookManager : IDisposable
    {
        public IntPtr hookHandle { get; private set; }
        //Try to keep the callback as fast as possible. Ideally extract the hook data and then process on a different thread.
        private HookHelper.HookProc callback;
        //Required to keep the callback delegate in memory preventing the GC from disposing of the callback.
        //https://docs.microsoft.com/en-us/dotnet/framework/debug-trace-profile/callbackoncollecteddelegate-mda
        private HookHelper.HookProc hookProc;

        public HookManager(HookHelper.HookProc callback, int hookType, string? library = null)
        {
            this.callback = callback;
            hookProc = HookCallback;
#pragma warning disable CS0618 // Type or member is obsolete
            if (library == null) hookHandle = HookHelper.SetWindowsHookEx(hookType, hookProc, AppDomain.GetCurrentThreadId());
#pragma warning restore CS0618 // Type or member is obsolete
            else hookHandle = HookHelper.SetWindowsHookEx(hookType, hookProc, HookHelper.LoadLibrary("User32"));
            if (hookHandle == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public void Dispose()
        {
            if (!HookHelper.UnhookWindowsHookEx(hookHandle)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        private int HookCallback(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int callbackResult;
                try { callbackResult = callback(nCode, wParam, lParam); }
                catch { callbackResult = -1; }
                if (callbackResult < 0) return callbackResult;
            }

            return HookHelper.CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }
    }
}
