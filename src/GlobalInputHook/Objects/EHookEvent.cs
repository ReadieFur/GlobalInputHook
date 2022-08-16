using System;

namespace GlobalInputHook.Objects
{
    [Serializable]
    public enum EHookEvent
    {
        None,
        KeyboardKeyUp,
        KeyboardKeyDown,
        MouseButtonUp,
        MouseButtonDown,
        MouseMove
        //MouseScroll
    }
}
