using System;
using System.Collections.Generic;

namespace GlobalInputHook.Objects
{
    [Serializable]
    public struct SHookData
    {
        public EHookEvent hookEvent;
        //I have to use an array here because there are too many values to use botmap masks.
        public IReadOnlyList<SKeyboardKeys> pressedKeyboardKeys;
        public IReadOnlyList<SMouseButtons> pressedMouseButtons;
        public SVector2 mousePosition;
        
        public SHookData()
        {
            hookEvent = EHookEvent.None;
            pressedKeyboardKeys = new List<SKeyboardKeys>(Enum.GetValues(typeof(SKeyboardKeys)).Length);
            pressedMouseButtons = new List<SMouseButtons>(Enum.GetValues(typeof(SMouseButtons)).Length);
            mousePosition = new SVector2();
        }
    }
}
