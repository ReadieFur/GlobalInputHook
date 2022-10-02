using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GlobalInputHook.Objects;
using CSharpTools.Pipes;

#nullable enable
namespace GlobalInputHook.IPC.Tools
{
    public class IPCInstanceHelper : IDisposable
    {
        #region Static
        //public const int UPDATE_RATE_MS = 1; //In milliseconds.
        public static IPCInstanceHelper? instance { get; private set; }

        public static IPCInstanceHelper GetOrCreateInstance(string ipcName, int maxUpdateRateMS = 1, string? inputHookBinaryPath = null)
        {
            if (instance != null) return instance;
            instance = new IPCInstanceHelper(ipcName, maxUpdateRateMS, inputHookBinaryPath);
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

        public Action<SHookData>? OnUpdate;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private IPCInstanceHelper(string ipcName, int maxUpdateRateMS = 1, string? inputHookBinaryPath = null)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            this.maxUpdateRateMS = 1;
            this.ipcName = ipcName;
            inputHookBinary = inputHookBinaryPath ?? (Environment.CurrentDirectory + "\\GlobalInputHook.IPC.exe");

            StartHookProcess();
            StartIPC();
        }

        ~IPCInstanceHelper() => Dispose();

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

            OnUpdate?.Invoke(serializedData);

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
