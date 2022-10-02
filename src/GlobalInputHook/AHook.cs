using System;

#nullable enable
namespace GlobalInputHook
{
    internal abstract class AHook<TSerializedData>
    {
		private static HookManager? hookManager;
        
		protected abstract int HOOK_TYPE_ID { get; }
		protected abstract string LIBRARY { get; }
		//protected abstract TSerializedData lastData { get; set; }

		public abstract event Action<TSerializedData>? OnData;
        
        ~AHook() => Unhook();

        public void Hook()
		{
			if (hookManager != null) return;
			hookManager = new HookManager(HookHelper.WrapHookProcVoidCallback(HookCallback), HOOK_TYPE_ID, LIBRARY);
		}

		public void Unhook() => hookManager?.Dispose();

		protected abstract void HookCallback(int code, int wParam, IntPtr lParam);
	}
}
