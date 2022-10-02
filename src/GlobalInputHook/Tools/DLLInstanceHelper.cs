using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using GlobalInputHook.Objects;

#nullable enable
namespace GlobalInputHook.Tools
{
    public static class DLLInstanceHelper
    {        
        public static bool isOnValidatedThread { get; private set; } = false;
        public static bool isHooking { get; private set; } = false;
        public static bool isDisposed { get; private set; } = false;
        public static int maxUpdateRateMS
        {
            get => Main.instance.maxUpdateRateMS;
            set => Main.instance.maxUpdateRateMS = value;
        }
        public static event Action<SHookData>? OnUpdate;

        private static readonly object lockObject = new();
        private static Thread? privateSTAThread = null;

        /// <param name="maxUpdateRateMS">If set to -1, the update rate limit is disabled.</param>
        /// <param name="skipThreadCheck">Running on a non-STAThread will cause the system input to be extremely slow,
        /// if you are sure you want to ignore this, set this parameter to true.</param>
        /// <exception cref="CustomAttributeFormatException"></exception>
        public static void Hook(int maxUpdateRateMS = -1, bool skipThreadCheck = false)
        {
            if (!isOnValidatedThread && !skipThreadCheck)
            {
                if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA/* || !Application.MessageLoop*/)
                    //throw new CustomAttributeFormatException("The executing thread is not an STAThread or does not have a running message loop.");
                    throw new CustomAttributeFormatException("The executing thread is not an STAThread.");
                isOnValidatedThread = true;
            }
            else isOnValidatedThread = false; //Assume we are not on an STA thread if the check is skipped.

            lock (lockObject)
            {
                if (isHooking) return;

                Main.instance.maxUpdateRateMS = maxUpdateRateMS;
                Main.instance.updateCallback = hookEvent => OnUpdate?.Invoke(hookEvent);
                Main.instance.Hook();

                isHooking = true;
            }
        }

        public static void StartHookOnNewSTAMessageThread(int maxUpdateRateMS = -1)
        {
            lock (lockObject)
            {
                if (privateSTAThread != null) return;
                try
                {
                    privateSTAThread = new Thread(() =>
                    {
                        Hook(maxUpdateRateMS);
                        Application.Run(); //This is required to handle the message loop and in turn listen to and fire the hook callbacks.
                    });
                    privateSTAThread.SetApartmentState(ApartmentState.STA);
                    privateSTAThread.Start();
                }
                catch (ThreadAbortException) {}
            }
        }

        public static void Unhook()
        {
            lock (lockObject)
            {
                if (isDisposed) return;
                Main.instance.Unhook();
                privateSTAThread?.Abort();
                privateSTAThread = null;
                isOnValidatedThread = false;
                isHooking = false;
                isDisposed = true;
            }
        }

        public static SHookData? GetCapturedData(EHookEvent hookEvent = EHookEvent.ManualRequest, int millisecondsTimeout = -1)
            => Main.instance.GetCapturedData(hookEvent, millisecondsTimeout);
    }
}
