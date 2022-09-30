using System;
using System.Windows.Forms;
using Threading = System.Threading;
using GlobalInputHook.Objects;
using System.Diagnostics;
using CSharpTools.Pipes;

namespace GlobalInputHook
{
    internal static class InputHookManager
    {
        private static bool isSetup = false;
        private static DateTime lastUpdateTime;
        private static CapturedData capturedData = new();

        internal static int maxUpdateRateMS;
        internal static Action<SHookData>? updateCallback = null;

        internal static void SetupHooks()
        {
            if (isSetup) return;

            KeyboardHook.INSTANCE.onData += KeyboardHook_KeyboardEvent;
            MouseHook.INSTANCE.onData += MouseHook_MouseEvent;

            KeyboardHook.INSTANCE.Hook();
            MouseHook.INSTANCE.Hook();
        }

        internal static void DisposeHooks()
        {
            if (!isSetup) return;
            KeyboardHook.INSTANCE.Unhook();
            MouseHook.INSTANCE.Unhook();
        }

        internal static SHookData? GetCapturedData(EHookEvent hookEvent = EHookEvent.ManualRequest, int millisecondsTimeout = -1)
            => capturedData.Freeze(hookEvent, millisecondsTimeout);

        private static void Update(EHookEvent hookEvent)
        {
            SHookData? data = capturedData.Freeze(hookEvent);
            if (data == null) return;

            DateTime now = DateTime.Now;
            if (maxUpdateRateMS > -1 && now - lastUpdateTime < TimeSpan.FromMilliseconds(maxUpdateRateMS)) return;
            lastUpdateTime = now;

            updateCallback?.Invoke(capturedData.Freeze(hookEvent).Value);
        }

        private static void KeyboardHook_KeyboardEvent(SKeyboardEventData keyboardEventData)
        {
            EHookEvent hookEvent = EHookEvent.ManualRequest; //This value will get overwritten, it is just here as a placeholder.
            switch (keyboardEventData.eventType)
            {
                case EKeyEvent.SYSKEY_DOWN:
                case EKeyEvent.KEY_DOWN:
                    if (capturedData.pressedKeyboardKeys.Contains((EKeyboardKeys)keyboardEventData.key)) return;
                    capturedData.pressedKeyboardKeys.Add((EKeyboardKeys)keyboardEventData.key);
                    hookEvent = EHookEvent.KeyboardKeyDown;
                    break;
                case EKeyEvent.SYSKEY_UP:
                case EKeyEvent.KEY_UP:
                    if (!capturedData.pressedKeyboardKeys.Contains((EKeyboardKeys)keyboardEventData.key)) return;
                    capturedData.pressedKeyboardKeys.Remove((EKeyboardKeys)keyboardEventData.key);
                    hookEvent = EHookEvent.KeyboardKeyUp;
                    break;
            }
            Update(hookEvent);
        }

        private static void MouseHook_MouseEvent(SMouseEventData mouseEventData)
        {
            EHookEvent hookEvent = EHookEvent.ManualRequest;
            switch (mouseEventData.eventType)
            {
                case EMouseEvent.MOUSEWHEEL:
                    //Skip mouse wheel events for now (this is because I don't know a way to tell if the mousewheel is still being used).
                    return;
                case EMouseEvent.LBUTTON_UP:
                    if (!capturedData.pressedMouseButtons.Contains(EMouseButtons.LeftButton)) return;
                    capturedData.pressedMouseButtons.Remove(EMouseButtons.LeftButton);
                    hookEvent = EHookEvent.MouseButtonUp;
                    break;
                case EMouseEvent.LBUTTON_DOWN:
                    if (capturedData.pressedMouseButtons.Contains(EMouseButtons.LeftButton)) return;
                    capturedData.pressedMouseButtons.Add(EMouseButtons.LeftButton);
                    hookEvent = EHookEvent.MouseButtonDown;
                    break;
                case EMouseEvent.RBUTTON_UP:
                    if (!capturedData.pressedMouseButtons.Contains(EMouseButtons.RightButton)) return;
                    capturedData.pressedMouseButtons.Remove(EMouseButtons.RightButton);
                    hookEvent = EHookEvent.MouseButtonUp;
                    break;
                case EMouseEvent.RBUTTON_DOWN:
                    if (capturedData.pressedMouseButtons.Contains(EMouseButtons.RightButton)) return;
                    capturedData.pressedMouseButtons.Add(EMouseButtons.RightButton);
                    hookEvent = EHookEvent.MouseButtonDown;
                    break;
                case EMouseEvent.MOUSE_MOVE:
                    capturedData.mousePosition = mouseEventData.cursorPosition;
                    hookEvent = EHookEvent.MouseMove;
                    break;
            }
            Update(hookEvent);
        }
    }
}
