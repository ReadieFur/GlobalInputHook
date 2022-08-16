using System;
using System.Collections.Generic;
using System.Threading;
using GlobalInputHook.Objects;

namespace GlobalInputHook
{
    internal class CapturedData
    {
        private readonly object lockObject = new object();

        public SKeyboardEventData keyboardEventData;
        public List<EKeyboardKeys> pressedKeyboardKeys = new(Enum.GetValues(typeof(EKeyboardKeys)).Length);
        public List<EMouseButtons> pressedMouseButtons = new(Enum.GetValues(typeof(EMouseButtons)).Length);
        public SVector2 mousePosition;

        public SHookData? Freeze(EHookEvent hookEvent = EHookEvent.None, int millisecondsTimeout = -1)
        {
            if (!Monitor.TryEnter(lockObject, millisecondsTimeout)) return null;

            SHookData data = new()
            {
                hookEvent = hookEvent,
                pressedKeyboardKeys = pressedKeyboardKeys,
                pressedMouseButtons = pressedMouseButtons,
                mousePosition = mousePosition
            };

            Monitor.Exit(lockObject);

            return data;
        }
    }
}
