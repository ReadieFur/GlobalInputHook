using System;
using GlobalInputHook.Objects;

#nullable enable
namespace GlobalInputHook
{
    internal class Main
    {
        #region Static
        private static Main? _instance;

        public static Main instance
        {
            get
            {
                if (_instance == null) _instance = new();
                return _instance;
            }
        }

        public static void Dispose()
        {
            if (_instance == null) return;
            _instance = null;
        }
        #endregion

        public delegate void UpdateCallback(SHookData hookData);

        internal int maxUpdateRateMS = 1;
        internal UpdateCallback? updateCallback = null;

        private DateTime lastUpdateTime;
        private CapturedData capturedData = new();
        private KeyboardHook keyboardHook = new();
        private MouseHook mouseHook = new();

        //An unhook is also done in the AHook destructor.
        private Main()
        {
            keyboardHook.OnData += KeyboardHook_KeyboardEvent;
            mouseHook.OnData += MouseHook_MouseEvent;
        }

        public void Hook()
        {
            keyboardHook.Hook();
            mouseHook.Hook();
        }

        /// <summary>
        /// This does not need to be called when disposing of this object.
        /// </summary>
        public void Unhook()
        {
            keyboardHook.Unhook();
            mouseHook.Unhook();
        }

        internal SHookData? GetCapturedData(EHookEvent hookEvent = EHookEvent.ManualRequest, int millisecondsTimeout = -1)
            => capturedData.Freeze(hookEvent, millisecondsTimeout);

        private void Update(EHookEvent hookEvent)
        {
            SHookData? data = capturedData.Freeze(hookEvent);
            if (data == null) return;

            DateTime now = DateTime.Now;
            if (maxUpdateRateMS > -1 && now - lastUpdateTime < TimeSpan.FromMilliseconds(maxUpdateRateMS)) return;
            lastUpdateTime = now;

            updateCallback?.Invoke(capturedData.Freeze(hookEvent)!.Value);
        }

        private void KeyboardHook_KeyboardEvent(SKeyboardEventData keyboardEventData)
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

        private void MouseHook_MouseEvent(SMouseEventData mouseEventData)
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
