using System;
using System.Diagnostics;
using GlobalInputHook.Objects;
using Timers = System.Timers;

namespace GlobalInputHook.Tools
{
    //Manage the shared memory and raise events on data.
    public class HookClientHelper : IDisposable
    {
        #region Static
        public const int UPDATE_RATE = 1; //In milliseconds.
        public static HookClientHelper instance { get; private set; }

        public static HookClientHelper GetOrCreateInstance(string ipcName, string? inputHookBinaryPath = null)
        {
            if (instance != null) return instance;
            instance = new HookClientHelper(ipcName, inputHookBinaryPath);
            return instance;
        }
        #endregion

        public event Action<SKeyboardEventData> keyboardEvent;
        public event Action<SMouseEventData> mouseEvent;

        private SharedMemory<SSharedData> sharedMemory;
        private Process process;
        private Timers.Timer timer;
        private SSharedData lastSharedData;

        private HookClientHelper(string ipcName, string? inputHookBinaryPath = null)
        {
            sharedMemory = new SharedMemory<SSharedData>(ipcName, 1);

            process = new Process();
            process.StartInfo.FileName = (inputHookBinaryPath ?? Environment.CurrentDirectory) + "\\GlobalInputHook.exe";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = $"--parent-process-id {Process.GetCurrentProcess().Id} --ipc-name {ipcName}";
            process.Start();

            timer = new Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = UPDATE_RATE; //Limited to 1000 updates per second (this is the quickest this type of loop can fire).
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        public void Dispose()
        {
            timer.Stop();
            timer.Dispose();
            process.Close();
            process.Dispose();
            sharedMemory.Dispose();
        }

        private void Timer_Elapsed(object? sender, Timers.ElapsedEventArgs e)
        {
            if (!sharedMemory.MutexRead(out SSharedData sharedData, UPDATE_RATE)) return; //Set the timeout to be no longer than the update rate.

            #region Keyboard data checks.
            if (sharedData.keyboardEventData.keyCode != lastSharedData.keyboardEventData.keyCode
                || sharedData.keyboardEventData.eventType != lastSharedData.keyboardEventData.eventType
            ) keyboardEvent?.Invoke(sharedData.keyboardEventData);
            #endregion

            #region Mouse data checks.
            if (sharedData.mouseEventData.eventType != lastSharedData.mouseEventData.eventType
                || sharedData.mouseEventData.cursorPosition.x != lastSharedData.mouseEventData.cursorPosition.x
                || sharedData.mouseEventData.cursorPosition.y != lastSharedData.mouseEventData.cursorPosition.y
            ) mouseEvent?.Invoke(sharedData.mouseEventData);
            #endregion

            lastSharedData = sharedData;
        }
    }
}
