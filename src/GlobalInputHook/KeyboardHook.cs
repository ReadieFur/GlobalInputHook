using GlobalInputHook.Objects;

namespace GlobalInputHook
{
    internal static class KeyboardHook
    {
		public static Action<SKeyboardEventData> keyboardEvent;
		public static SKeyboardEventData lastKeyboardEventData { get; private set; }

		private const int WH_KEYBOARD_LL = 13;
		private static HookManager? hookManager;
		private static object mutexObject = new object();

		public static void Hook()
		{
			hookManager = new HookManager(HookHelper.WrapHookProcVoidCallback(HookCallback), WH_KEYBOARD_LL, "User32");
		}

		public static void Unhook() => hookManager?.Dispose();

		private static void HookCallback(int code, int wParam, IntPtr lParam)
		{
			if (!Monitor.TryEnter(mutexObject, 0)) return;

			SKeyboardEventData keyboardEventData = new SKeyboardEventData
			{
				eventType = (EKeyEvent)wParam,
				keyCode = HookHelper.IntPtrToStruct<SKeyboardHookData>(lParam).vkCode
			};

			if (
				lastKeyboardEventData.eventType != keyboardEventData.eventType
				|| lastKeyboardEventData.keyCode != keyboardEventData.keyCode
			)
            {
				Task.Run(() => keyboardEvent.Invoke(keyboardEventData));
				//foreach (Action<SKeyboardEventData> action in newKeyboardEvent.GetInvocationList())
					//Task.Run(() => action.Invoke(keyboardEventData));

				lastKeyboardEventData = keyboardEventData;
			}

			Monitor.Exit(mutexObject);
		}
	}
}
