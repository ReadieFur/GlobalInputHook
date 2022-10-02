using System;
using System.Windows.Forms;
using GlobalInputHook.Objects;

#nullable enable
namespace GlobalInputHook
{
    internal class KeyboardHook : AHook<SKeyboardEventData>
	{
		private SKeyboardEventData lastData;

		protected override int HOOK_TYPE_ID { get => 13; }
		protected override string LIBRARY { get => "User32"; }

        public override event Action<SKeyboardEventData>? OnData;

		protected override void HookCallback(int code, int wParam, IntPtr lParam)
		{
			SKeyboardEventData keyboardEventData = new SKeyboardEventData
			{
				eventType = (EKeyEvent)wParam,
				key = (Keys)HookHelper.IntPtrToStruct<SKeyboardHookData>(lParam).vkCode
			};

			if (keyboardEventData.Equals(lastData)) return;
            lastData = keyboardEventData;

			OnData?.Invoke(keyboardEventData);
		}
	}
}
