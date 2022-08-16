using System;
using System.Collections.Generic;

namespace GlobalInputHook.Objects
{
    [Serializable]
    public struct SHookData
    {
        public EHookEvent hookEvent;
        //I have to use an array here because there are too many values to use botmap masks.
        public IReadOnlyList<EKeyboardKeys> pressedKeyboardKeys;
        public IReadOnlyList<EMouseButtons> pressedMouseButtons;
        public SVector2 mousePosition;
        
        public SHookData()
        {
            hookEvent = EHookEvent.None;
            pressedKeyboardKeys = new List<EKeyboardKeys>(Enum.GetValues(typeof(EKeyboardKeys)).Length);
            pressedMouseButtons = new List<EMouseButtons>(Enum.GetValues(typeof(EMouseButtons)).Length);
            mousePosition = new SVector2();
        }
    }
}
