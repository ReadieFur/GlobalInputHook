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

        private static int updateRateMS;
        private static DateTime lastUpdateTime;
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
            if (parentProcessIDArgIndex == -1 || ++parentProcessIDArgIndex >= args.Length) Environment.Exit((int)EExitCodes.InvalidParentProcessID);
            
            int parentProcessID;
            if (!int.TryParse(args[parentProcessIDArgIndex], out parentProcessID)) Environment.Exit((int)EExitCodes.InvalidParentProcessID);
            
            Process? parentProcess = null;
            try { parentProcess = Process.GetProcessById(parentProcessID); }
            catch { Environment.Exit((int)EExitCodes.InvalidParentProcessID); }
            
            parentProcessWatchTimer = new Threading.Timer((_) =>
            {
                if (parentProcess!.HasExited) Environment.Exit((int)EExitCodes.ParentProcessExited);
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private static void SetupSharedMemory()
        {
#if DEBUG && false
            string ipcName = "global_input_hook";
            updateRateMS = 1;
#else
            int mapArgIndex = Array.FindIndex(args, itm => itm == "--ipc-name");
            if (mapArgIndex == -1 || ++mapArgIndex >= args.Length) Environment.Exit((int)EExitCodes.InvalidMapArgument);
            string ipcName = args[mapArgIndex];

            int updateRateArgIndex = Array.FindIndex(args, itm => itm == "--update-rate");
            if (updateRateArgIndex == -1
                || ++updateRateArgIndex >= args.Length
                || !int.TryParse(args[updateRateArgIndex], out updateRateMS))
                Environment.Exit((int)EExitCodes.InvalidUpdateRateArgument);
#endif
            sharedMemory = new SharedMemory<SSharedData>(ipcName);
            sharedData = new SharedData();
        }

        private static void UpdateSharedDataWrapper(Action action)
        {
            //Waiting on the mutex here hopefulyl shouldn't be a problem, so I am skipping it to save some CPU time.
            //if (!Threading.Monitor.TryEnter(sharedDataLocalMutexObject, HookClientHelper.UPDATE_RATE_MS)) return;
            action();
            //Threading.Monitor.Exit(sharedDataLocalMutexObject);
            
            DateTime now = DateTime.Now;
            if (now - lastUpdateTime < TimeSpan.FromMilliseconds(updateRateMS)) return;
            lastUpdateTime = now;

            sharedMemory.MutexWrite(sharedData.Freeze(), updateRateMS);
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
