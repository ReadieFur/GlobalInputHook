using System;

namespace GlobalInputHook.Objects
{
    [Flags]
    [Serializable]
    public enum EHookEvent
    {
        None            = 2 ^ 0,
        KeyboardKeyUp   = 2 ^ 1,
        KeyboardKeyDown = 2 ^ 2,
        MouseButtonUp   = 2 ^ 3,
        MouseButtonDown = 2 ^ 4,
        MouseMove       = 2 ^ 5,
        //MouseScroll   = 2 ^ 6
    }
}
