using System;
using System.Reflection;
using GlobalInputHook.Objects;

namespace GlobalInputHook.Tools
{
    public class HookClientHelperInternal
    {
        private static readonly object lockObject = new();

        public static bool isOnSTAThread { get; private set; } = false;
        public static bool isHooking { get; private set; } = false;
        public static bool isDisposed { get; private set; } = false;
        public static int maxUpdateRateMS
        {
            get => InputHookManager.maxUpdateRateMS;
            set => InputHookManager.maxUpdateRateMS = value;
        }
        public static event Action<SHookData>? OnUpdate;
        
        /// <param name="maxUpdateRateMS">If set to -1, the update rate limit is disabled.</param>
        /// <param name="skipSTAThreadCheck">Running on a non-STAThread will cause the system input to be extremely slow,
        /// if you are sure you want to ignore this, set this parameter to true.</param>
        /// <exception cref="CustomAttributeFormatException"></exception>
        public static void StartHook(int maxUpdateRateMS = -1, bool skipSTAThreadCheck = false)
        {
            if (!isOnSTAThread && !skipSTAThreadCheck)
            {
                if (Assembly.GetEntryAssembly().EntryPoint.GetCustomAttribute<STAThreadAttribute>() == null
                    || System.Threading.Thread.CurrentThread.GetApartmentState() != System.Threading.ApartmentState.STA)
                            throw new CustomAttributeFormatException("The executing thread is not an STAThread.");
                isOnSTAThread = true;
            }
            else isOnSTAThread = false; //Assume we are not on an STA thread if the check is skipped.

            lock (lockObject)
            {
                if (isHooking) return;

                InputHookManager.maxUpdateRateMS = maxUpdateRateMS;
                InputHookManager.updateCallback = hookEvent => OnUpdate?.Invoke(hookEvent);
                InputHookManager.SetupHooks();

                isHooking = true;
            }
        }

        public static void Dispose()
        {
            lock (lockObject)
            {
                if (isDisposed) return;
                InputHookManager.DisposeHooks();
                isOnSTAThread = false;
                isHooking = false;
                isDisposed = true;
            }
        }

        public static SHookData? GetCapturedData(EHookEvent hookEvent = EHookEvent.ManualRequest, int millisecondsTimeout = -1)
            => InputHookManager.GetCapturedData(hookEvent, millisecondsTimeout);
    }
}
