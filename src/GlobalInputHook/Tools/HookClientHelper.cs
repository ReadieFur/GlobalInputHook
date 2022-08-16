using System;
using System.Threading.Tasks;
using System.Diagnostics;
using GlobalInputHook.Objects;
using CSharpTools.Pipes;

namespace GlobalInputHook.Tools
{
    //Manage the shared memory and raise events on data.
    public class HookClientHelper : IDisposable
    {
        #region Static
        //public const int UPDATE_RATE_MS = 1; //In milliseconds.
        public static HookClientHelper instance { get; private set; }

        public static HookClientHelper GetOrCreateInstance(string ipcName, int maxUpdateRateMS = 1, string? inputHookBinaryPath = null)
        {
            if (instance != null) return instance;
            instance = new HookClientHelper(ipcName, maxUpdateRateMS, inputHookBinaryPath);
            return instance;
        }
        #endregion

        private string ipcName;
        private string inputHookBinary;
        private int maxUpdateRateMS;
        private bool shouldExit = false;
        private PipeClient pipeClient;
        private Process? process;
        private SHookData lastData;

        public Action<SHookData>? onData;

        private HookClientHelper(string ipcName, int maxUpdateRateMS = 1, string? inputHookBinaryPath = null)
        {
            this.maxUpdateRateMS = 1;
            this.ipcName = ipcName;
            inputHookBinary = (inputHookBinaryPath ?? Environment.CurrentDirectory) + "\\GlobalInputHook.exe";
            
            StartHookProcess();
            StartIPC();
        }

        ~HookClientHelper() => Dispose();

        public void Dispose()
        {
            shouldExit = true;
            process?.Close();
            process?.Dispose();
            pipeClient?.Dispose();
            instance = null;
        }

        private async void StartHookProcess()
        {
            process = new Process();
            process.StartInfo.FileName = inputHookBinary;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = $"--parent-process-id {Process.GetCurrentProcess().Id} --ipc-name {ipcName} --max-update-rate-ms {maxUpdateRateMS}";
            process.Start();
            
            await Task.Run(process.WaitForExit);
            pipeClient?.Dispose();

            if (shouldExit) return;
            await Task.Delay(1000); //Wait for a moment.
            StartHookProcess();
        }

        private void StartIPC()
        {
            pipeClient = new PipeClient(ipcName, Helpers.ComputeBufferSizeOf<SHookData>());
            pipeClient.onMessage += PipeClient_onMessage;
            pipeClient.onDispose += PipeClient_onDispose;
        }

        private void PipeClient_onMessage(ReadOnlyMemory<byte> data)
        {
            SHookData serializedData;
            try { serializedData = Helpers.Deserialize<SHookData>(data.ToArray()); }
            catch { return; }
            
            if (serializedData.Equals(lastData)) return;

            onData?.Invoke(serializedData);

            lastData = serializedData;
        }

        private async void PipeClient_onDispose()
        {
            if (shouldExit) return;
            await Task.Delay(1000);
            StartIPC();
        }
    }
}
