namespace GlobalInputHook.Objects
{
    internal class SharedData
    {
        public readonly object mutexObject = new object();

        public SKeyboardEventData keyboardEventData;
        public SMouseEventData mouseEventData;

        public SSharedData Freeze()
        {
            return new SSharedData
            {
                keyboardEventData = keyboardEventData,
                mouseEventData = mouseEventData
            };
        }
    }
}
