using System;

namespace GlobalInputHook.Objects
{
    [Flags]
    [Serializable]
    public enum EHookEvent
    {
        None = 0,
        ManualRequest = 1,
        KeyboardKeyUp = 2,
        KeyboardKeyDown = 4,
        MouseButtonUp = 8,
        MouseButtonDown = 16,
        MouseMove = 32,
        //MouseScroll = 64,
    }
}
