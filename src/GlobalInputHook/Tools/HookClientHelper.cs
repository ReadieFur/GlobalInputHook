using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using GlobalInputHook.Objects;
using Timers = System.Timers;

namespace GlobalInputHook.Tools
{
    //Manage the shared memory and raise events on data.
    public class HookClientHelper : IDisposable
    {
        #region Static
        //public const int UPDATE_RATE_MS = 1; //In milliseconds.
        public static HookClientHelper instance { get; private set; }

        public static HookClientHelper GetOrCreateInstance(string ipcName, int updateRateMS = 1, string? inputHookBinaryPath = null)
        {
            if (instance != null) return instance;
            instance = new HookClientHelper(ipcName, updateRateMS, inputHookBinaryPath);
            return instance;
        }
        #endregion
        
        public int updateRateMS { get; private set; } = 1;
        public event Action<SKeyboardEventData> keyboardEvent;
        public event Action<SMouseEventData> mouseEvent;

        private string ipcName;
        private string inputHookBinary;
        private bool shouldExit = false;
        private SharedMemory<SSharedData> sharedMemory;
        private Process? process;
        private Timers.Timer updateTimer;
        private SSharedData lastSharedData;

        private HookClientHelper(string ipcName, int updateRateMS = 1, string? inputHookBinaryPath = null)
        {
            sharedMemory = new SharedMemory<SSharedData>(ipcName, 1);

            this.updateRateMS = 1;
            this.ipcName = ipcName;
            inputHookBinary = (inputHookBinaryPath ?? Environment.CurrentDirectory) + "\\GlobalInputHook.exe";
            StartHookProcess();

            updateTimer = new Timers.Timer();
            updateTimer.AutoReset = true;
            updateTimer.Interval = updateRateMS; //Limited to 1000 updates per second (this is the quickest this type of loop can fire).
            updateTimer.Elapsed += Timer_Elapsed;
            updateTimer.Start();
        }

        ~HookClientHelper() => Dispose();

        public void Dispose()
        {
            shouldExit = true;
            updateTimer?.Stop();
            updateTimer?.Dispose();
            process?.Close();
            process?.Dispose();
            sharedMemory?.Dispose();
        }

        private void StartHookProcess()
        {
            process = new Process();
            process.StartInfo.FileName = inputHookBinary;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = $"--parent-process-id {Process.GetCurrentProcess().Id} --ipc-name {ipcName} --update-rate {updateRateMS}";
            process.Start();
            Task.Run(() =>
            {
                process.WaitForExit();
                if (shouldExit) return;
                Thread.Sleep(1000); //Wait for a moment.
                StartHookProcess();
            });
        }

        private void Timer_Elapsed(object? sender, Timers.ElapsedEventArgs e)
        {
            if (!sharedMemory.MutexRead(out SSharedData sharedData, updateRateMS)) return; //Set the timeout to be no longer than the update rate.

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
