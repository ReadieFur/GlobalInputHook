//#define DEBUG_OVERRIDE

using System;
using System.Diagnostics;
using System.Windows.Forms;
using Threading = System.Threading;
using GlobalInputHook.Objects;
using GlobalInputHook.Tools;
using CSharpTools.Pipes;

#nullable enable
namespace GlobalInputHook.IPC
{
    //TODO: Make this program a singleton which requires an owner at all times, if the host process ends, signal another program to take over (if applicable).

    internal static class Program
    {
        public static readonly string[] args = Environment.GetCommandLineArgs();

        private static Threading.Timer parentProcessWatchTimer;
        private static PipeServerManager pipeServerManager;

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

            int maxUpdateRateMSArgIndex = Array.FindIndex(args, itm => itm == "--max-update-rate-ms");
            if (maxUpdateRateMSArgIndex == -1 || ++maxUpdateRateMSArgIndex >= args.Length) Environment.Exit((int)EExitCode.InvalidMaxUpdateRateArgument);
            if (!int.TryParse(args[maxUpdateRateMSArgIndex], out int maxUpdateRateMS)) Environment.Exit((int)EExitCode.InvalidMaxUpdateRateArgument);

            DLLInstanceHelper.OnUpdate += hookData => pipeServerManager.BroadcastMessage(Helpers.Serialize(hookData));
            DLLInstanceHelper.Hook(maxUpdateRateMS);

            Application.ApplicationExit += Application_ApplicationExit;

            Application.Run();
        }

        private static void Application_ApplicationExit(object? sender, EventArgs e)
        {
            parentProcessWatchTimer.Dispose();
            pipeServerManager.Dispose();
        }

        private static void SetupParentWatch()
        {
            int parentProcessIDArgIndex = Array.FindIndex(args, itm => itm == "--parent-process-id");
            if (parentProcessIDArgIndex == -1 || ++parentProcessIDArgIndex >= args.Length) Environment.Exit((int)EExitCode.InvalidParentProcessID);

            int parentProcessID;
            if (!int.TryParse(args[parentProcessIDArgIndex], out parentProcessID)) Environment.Exit((int)EExitCode.InvalidParentProcessID);

            Process? parentProcess = null;
            try { parentProcess = Process.GetProcessById(parentProcessID); }
            catch { Environment.Exit((int)EExitCode.InvalidParentProcessID); }

            parentProcessWatchTimer = new Threading.Timer((_) =>
            {
                if (parentProcess!.HasExited) Environment.Exit((int)EExitCode.ParentProcessExited);
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private static void SetupIPC()
        {
#if DEBUG && DEBUG_OVERRIDE
            string ipcName = "global_input_hook";
#else
            int mapArgIndex = Array.FindIndex(args, itm => itm == "--ipc-name");
            if (mapArgIndex == -1 || ++mapArgIndex >= args.Length) Environment.Exit((int)EExitCode.InvalidMapArgument);
            string ipcName = args[mapArgIndex];
#endif

            pipeServerManager = new PipeServerManager(ipcName, Helpers.ComputeBufferSizeOf<SHookData>());
            pipeServerManager.onMessage += PipeServerManager_onMessage;
        }

        //For manual request of data.
        private static void PipeServerManager_onMessage(Guid id, ReadOnlyMemory<byte> data) =>
            pipeServerManager.SendMessage(id, Helpers.Serialize(DLLInstanceHelper.GetCapturedData()!.Value));
    }
}
