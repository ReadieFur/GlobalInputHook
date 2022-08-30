//#define DEBUG_OVERRIDE

using System;
using System.Windows.Forms;
using Threading = System.Threading;
using GlobalInputHook.Objects;
using System.Diagnostics;
using CSharpTools.Pipes;

namespace GlobalInputHook
{
    //TODO: Make this program a singleton which requires an owner at all times, if the host process ends, signal another program to take over (if applicable).

    internal static class Program
    {
        public static readonly string[] args = Environment.GetCommandLineArgs();

        private static int maxUpdateRateMS;
        private static DateTime lastUpdateTime;
        private static Threading.Timer parentProcessWatchTimer;
        private static PipeServerManager pipeServerManager;
        private static CapturedData capturedData = new();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if !DEBUG || !DEBUG_OVERRIDE
            SetupParentWatch();
#endif
            SetupIPC();
            SetupHooks();

            Application.ApplicationExit += Application_ApplicationExit;

            Application.Run();
        }

        private static void Application_ApplicationExit(object? sender, EventArgs e)
        {
            parentProcessWatchTimer.Dispose();
            KeyboardHook.INSTANCE.Unhook();
            MouseHook.INSTANCE.Unhook();
            pipeServerManager.Dispose();
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
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private static void SetupHooks()
        {
            KeyboardHook.INSTANCE.onData += KeyboardHook_KeyboardEvent;
            MouseHook.INSTANCE.onData += MouseHook_MouseEvent;
            
            KeyboardHook.INSTANCE.Hook();
            MouseHook.INSTANCE.Hook();
        }

        private static void SetupIPC()
        {
#if DEBUG && DEBUG_OVERRIDE
            string ipcName = "global_input_hook";
            maxUpdateRateMS = 1;
#else
            int mapArgIndex = Array.FindIndex(args, itm => itm == "--ipc-name");
            if (mapArgIndex == -1 || ++mapArgIndex >= args.Length) Environment.Exit((int)EExitCodes.InvalidMapArgument);
            string ipcName = args[mapArgIndex];

            int maxUpdateRateMSArgIndex = Array.FindIndex(args, itm => itm == "--max-update-rate-ms");
            if (maxUpdateRateMSArgIndex == -1 || ++maxUpdateRateMSArgIndex >= args.Length) Environment.Exit((int)EExitCodes.InvalidMaxUpdateRateArgument);
            if (!int.TryParse(args[maxUpdateRateMSArgIndex], out maxUpdateRateMS)) Environment.Exit((int)EExitCodes.InvalidMaxUpdateRateArgument);
#endif

            pipeServerManager = new PipeServerManager(ipcName, Helpers.ComputeBufferSizeOf<SHookData>());
            pipeServerManager.onMessage += PipeServerManager_onMessage;
        }

        //For manual request of data.
        private static void PipeServerManager_onMessage(Guid id, ReadOnlyMemory<byte> data) =>
            pipeServerManager.SendMessage(id, Helpers.Serialize(capturedData.Freeze().Value));

        private static void BroadcastIPC(EHookEvent hookEvent)
        {
            SHookData? data = capturedData.Freeze(hookEvent);
            if (data == null) return;

            DateTime now = DateTime.Now;
            if (now - lastUpdateTime < TimeSpan.FromMilliseconds(maxUpdateRateMS)) return;
            lastUpdateTime = now;
            
            pipeServerManager.BroadcastMessage(Helpers.Serialize(data.Value));
        }

        private static void KeyboardHook_KeyboardEvent(SKeyboardEventData keyboardEventData)
        {
            EHookEvent hookEvent = EHookEvent.ManualRequest; //This value will get overwritten, it is just here as a placeholder.
            switch (keyboardEventData.eventType)
            {
                case EKeyEvent.SYSKEY_DOWN:
                case EKeyEvent.KEY_DOWN:
                    if (capturedData.pressedKeyboardKeys.Contains((EKeyboardKeys)keyboardEventData.key)) return;
                    capturedData.pressedKeyboardKeys.Add((EKeyboardKeys)keyboardEventData.key);
                    hookEvent = EHookEvent.KeyboardKeyDown;
                    break;
                case EKeyEvent.SYSKEY_UP:
                case EKeyEvent.KEY_UP:
                    if (!capturedData.pressedKeyboardKeys.Contains((EKeyboardKeys)keyboardEventData.key)) return;
                    capturedData.pressedKeyboardKeys.Remove((EKeyboardKeys)keyboardEventData.key);
                    hookEvent = EHookEvent.KeyboardKeyUp;
                    break;
            }
            BroadcastIPC(hookEvent);
        }

        private static void MouseHook_MouseEvent(SMouseEventData mouseEventData)
        {
            EHookEvent hookEvent = EHookEvent.ManualRequest;
            switch (mouseEventData.eventType)
            {
                case EMouseEvent.MOUSEWHEEL:
                    //Skip mouse wheel events for now (this is because I don't know a way to tell if the mousewheel is still being used).
                    return;
                case EMouseEvent.LBUTTON_UP:
                    if (!capturedData.pressedMouseButtons.Contains(EMouseButtons.LeftButton)) return;
                    capturedData.pressedMouseButtons.Remove(EMouseButtons.LeftButton);
                    hookEvent = EHookEvent.MouseButtonUp;
                    break;
                case EMouseEvent.LBUTTON_DOWN:
                    if (capturedData.pressedMouseButtons.Contains(EMouseButtons.LeftButton)) return;
                    capturedData.pressedMouseButtons.Add(EMouseButtons.LeftButton);
                    hookEvent = EHookEvent.MouseButtonDown;
                    break;
                case EMouseEvent.RBUTTON_UP:
                    if (!capturedData.pressedMouseButtons.Contains(EMouseButtons.RightButton)) return;
                    capturedData.pressedMouseButtons.Remove(EMouseButtons.RightButton);
                    hookEvent = EHookEvent.MouseButtonUp;
                    break;
                case EMouseEvent.RBUTTON_DOWN:
                    if (capturedData.pressedMouseButtons.Contains(EMouseButtons.RightButton)) return;
                    capturedData.pressedMouseButtons.Add(EMouseButtons.RightButton);
                    hookEvent = EHookEvent.MouseButtonDown;
                    break;
                case EMouseEvent.MOUSE_MOVE:
                    capturedData.mousePosition = mouseEventData.cursorPosition;
                    hookEvent = EHookEvent.MouseMove;
                    break;
            }
            BroadcastIPC(hookEvent);
        }
    }
}
