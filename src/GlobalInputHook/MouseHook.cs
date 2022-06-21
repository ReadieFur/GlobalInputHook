using GlobalInputHook.Objects;

namespace GlobalInputHook
{
    internal class MouseHook
    {
		public static Action<SMouseEventData> mouseEvent;
		public static SMouseEventData lastMouseEventData { get; private set; }

		private const int WH_MOUSE_LL = 14;
        private static HookManager? hookManager;
        private static object mutexObject = new object();

		public static void Hook()
		{
			hookManager = new HookManager(HookHelper.WrapHookProcVoidCallback(HookCallback), WH_MOUSE_LL, "User32");
		}

		public static void Unhook() => hookManager?.Dispose();

		private static void HookCallback(int code, int wParam, IntPtr lParam)
		{
			if (!Monitor.TryEnter(mutexObject, 0)) return;

			SMouseEventData mouseEventData = new SMouseEventData
			{
				eventType = (EMouseEvent)wParam,
				cursorPosition = HookHelper.IntPtrToStruct<SMouseHookData>(lParam).pt
			};

			if (
				lastMouseEventData.eventType != mouseEventData.eventType
				|| lastMouseEventData.cursorPosition.x != mouseEventData.cursorPosition.x
				|| lastMouseEventData.cursorPosition.y != mouseEventData.cursorPosition.y
			)
			{
				Task.Run(() => mouseEvent.Invoke(mouseEventData));
				lastMouseEventData = mouseEventData;
			}

			Monitor.Exit(mutexObject);
		}
	}
}
