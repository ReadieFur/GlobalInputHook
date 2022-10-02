using System;
using GlobalInputHook.Objects;

#nullable enable
namespace GlobalInputHook
{
    internal class MouseHook : AHook<SMouseEventData>
	{
		private SMouseEventData lastData;

		protected override int HOOK_TYPE_ID { get => 14; }
		protected override string LIBRARY { get => "User32"; }

		public override event Action<SMouseEventData>? OnData;

		protected override void HookCallback(int code, int wParam, IntPtr lParam)
        {
			SMouseEventData mouseEventData = new SMouseEventData
			{
				eventType = (EMouseEvent)wParam,
				cursorPosition = HookHelper.IntPtrToStruct<SMouseHookData>(lParam).pt
			};

			if (mouseEventData.Equals(lastData)) return;
            lastData = mouseEventData;

            OnData?.Invoke(mouseEventData);
		}
    }
}
