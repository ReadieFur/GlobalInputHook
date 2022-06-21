/*Exit codes:
* 1 Invalid parent ID.
* 2 Parent process exited.
* 2 Invalid map argument.
* 3 Failed to map shared buffer.
*/

using System;
using System.Windows.Forms;
using Threading = System.Threading;
using GlobalInputHook.Objects;
using GlobalInputHook.Tools;
using System.Diagnostics;

namespace GlobalInputHook
{
    internal static class Program
    {
        public static string[] args
        {
            get => Environment.GetCommandLineArgs();
            private set {}
        }

        private static Threading.Timer parentProcessWatchTimer;
        private static SharedMemory<SSharedData> sharedMemory;
        private static SharedData sharedData;
        private static object sharedDataLocalMutexObject = new object();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if RELEASE || true
            SetupParentWatch();
#endif
            SetupSharedMemory();

            #region Setup hooks
            KeyboardHook.keyboardEvent += KeyboardHook_KeyboardEvent;
            KeyboardHook.Hook();

            MouseHook.mouseEvent += MouseHook_MouseEvent;
            MouseHook.Hook();
            #endregion

            Application.ApplicationExit += Application_ApplicationExit;

            Application.Run();
        }

        private static void Application_ApplicationExit(object? sender, EventArgs e)
        {
            parentProcessWatchTimer.Dispose();
            KeyboardHook.Unhook();
            MouseHook.Unhook();
            sharedMemory.Dispose();
        }

        private static void SetupParentWatch()
        {
            int parentProcessIDArgIndex = Array.FindIndex(args, itm => itm == "--parent-process-id");
            if (parentProcessIDArgIndex == -1) Environment.Exit(1);
            else if (++parentProcessIDArgIndex >= args.Length) Environment.Exit(1);
            int parentProcessID;
            if (!int.TryParse(args[parentProcessIDArgIndex], out parentProcessID)) Environment.Exit(1);
            Process? parentProcess = null;
            try { parentProcess = Process.GetProcessById(parentProcessID); }
            catch { Environment.Exit(1); }
            parentProcessWatchTimer = new Threading.Timer((_) =>
            {
                if (parentProcess!.HasExited) Environment.Exit(2);
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private static void SetupSharedMemory()
        {
#if DEBUG && false
            string ipcName = "global_input_hook";
#else
            int mapArgIndex = Array.FindIndex(args, itm => itm == "--ipc-name");
            if (mapArgIndex == -1) Environment.Exit(3);
            else if (++mapArgIndex >= args.Length) Environment.Exit(3);
            string ipcName = args[mapArgIndex];
#endif
            sharedMemory = new SharedMemory<SSharedData>(ipcName);
            sharedData = new SharedData();
        }

        private static void UpdateSharedDataWrapper(Action action)
        {
            if (!Threading.Monitor.TryEnter(sharedDataLocalMutexObject, HookClientHelper.UPDATE_RATE)) return;
            action();
            Threading.Monitor.Exit(sharedDataLocalMutexObject);
            sharedMemory.MutexWrite(sharedData.Freeze(), HookClientHelper.UPDATE_RATE);
        }

        private static void KeyboardHook_KeyboardEvent(SKeyboardEventData keyboardEventData)
        {
            UpdateSharedDataWrapper(() => sharedData.keyboardEventData = keyboardEventData);

#if DEBUG && false
            //Debug.WriteLine(keyboardEventData.keyCode);
            sharedMemory.MutexRead(out SSharedData outValue, 1);
            Debug.WriteLine(outValue.keyboardEventData.keyCode);
#endif
        }

        private static void MouseHook_MouseEvent(SMouseEventData mouseEventData)
        {
            UpdateSharedDataWrapper(() => sharedData.mouseEventData = mouseEventData);

#if DEBUG && false
            //Debug.WriteLine(mouseEventData.cursorPosition.x);
            sharedMemory.MutexRead(out SSharedData outValue, 1);
            Debug.WriteLine(outValue.mouseEventData.cursorPosition.x);
#endif
        }
    }
}
