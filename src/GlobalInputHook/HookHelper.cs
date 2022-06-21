using System;
using System.Runtime.InteropServices;

namespace GlobalInputHook
{
    internal class HookHelper
    {
		public delegate int HookProc(int nCode, int wParam, IntPtr lParam);
		public delegate void HookProcVoid(int nCode, int wParam, IntPtr lParam);
		/*public static HookProc WrapHookProcCallback(int hookID, HookProc callback)
		{
			return (nCode, wParam, lParam) =>
			{
				if (nCode >= 0)
				{
					int callbackResult;
					try { callbackResult = callback(nCode, wParam, lParam); }
					catch { callbackResult = -1; }
					if (callbackResult < 0) return callbackResult;
				}

				return CallNextHookEx(hookID, nCode, wParam, lParam);
			};
		}*/
		public static HookProc WrapHookProcVoidCallback(HookProcVoid callback)
		{
			return (nCode, wParam, lParam) =>
			{
				bool error = false;
				try { callback(nCode, wParam, lParam); }
				catch { error = true; }
				return error ? -1 : 0;
			};
		}

		/// <summary>
		/// Sets the windows hook, do the desired event, one of hModule or threadId must be non-null.
		/// </summary>
		/// <param name="hookTypeID">The id of the event you want to hook.</param>
		/// <param name="callback">The callback.</param>
		/// <param name="hModule">The module you want to attach the event to, can be null.</param>
		/// <param name="threadId">The thread you want to attach the event to, can be null.</param>
		/// <returns>A handle to the desired hook.</returns>
		[DllImport("user32.dll")]
		private static extern IntPtr SetWindowsHookEx(int hookTypeID, HookProc callback, IntPtr hModule, int threadId);
		//For low-level hooks.
		public static IntPtr SetWindowsHookEx(int hookTypeID, HookProc callback, IntPtr hModule) => SetWindowsHookEx(hookTypeID, callback, hModule, 0);
		//For high-level hooks.
		public static IntPtr SetWindowsHookEx(int hookTypeID, HookProc callback, int threadId) => SetWindowsHookEx(hookTypeID, callback, IntPtr.Zero, threadId);

		/// <summary>
		/// Unhooks the windows hook.
		/// </summary>
		/// <param name="hookHandle">The hook handle that was returned from SetWindowsHookEx</param>
		/// <returns>True if successful, false otherwise</returns>
		[DllImport("user32.dll")]
		public static extern bool UnhookWindowsHookEx(IntPtr hookID);

		/// <summary>
		/// Calls the next hook.
		/// </summary>
		/// <param name="hookHandle">The hook handle.</param>
		/// <param name="nCode">The hook code.</param>
		/// <param name="wParam">The wparam.</param>
		/// <param name="lParam">The lparam.</param>
		[DllImport("user32.dll")]
		public static extern int CallNextHookEx(IntPtr hookHandle, int nCode, int wParam, IntPtr lParam);

		/// <summary>
		/// Loads the library.
		/// </summary>
		/// <param name="lpFileName">Name of the library.</param>
		/// <returns>A handle to the library.</returns>
		[DllImport("kernel32.dll")]
		public static extern IntPtr LoadLibrary(string lpFileName);

		public static T? IntPtrToStruct<T>(IntPtr intPtr) => (T?)Marshal.PtrToStructure(intPtr, typeof(T));
	}
}
