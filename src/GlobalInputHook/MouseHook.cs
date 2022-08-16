using System;
using GlobalInputHook.Objects;

namespace GlobalInputHook
{
    internal class MouseHook : AHook<SMouseEventData>
	{
		public static readonly MouseHook INSTANCE = new MouseHook();

		private SMouseEventData lastData;

		protected override int HOOK_TYPE_ID { get => 14; }
		protected override string LIBRARY { get => "User32"; }

		public override event Action<SMouseEventData>? onData;

		protected override void HookCallback(int code, int wParam, IntPtr lParam)
        {
			SMouseEventData mouseEventData = new SMouseEventData
			{
				eventType = (EMouseEvent)wParam,
				cursorPosition = HookHelper.IntPtrToStruct<SMouseHookData>(lParam).pt
			};

			if (mouseEventData.Equals(lastData)) return;
            lastData = mouseEventData;

            onData?.Invoke(mouseEventData);
		}
    }
}
