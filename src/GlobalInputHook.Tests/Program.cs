using System;
using GlobalInputHook.Objects;
using GlobalInputHook.Tools;

namespace GlobalInputHook.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //You must provide a unique IPC name for the program here.
            /*You can optinally also provide the path for the hook program here, the binary must however be called `GlobalInputHook.exe`
            By default the program will look in the current working directory for the binary.*/
            HookClientHelperIPC dataHook = HookClientHelperIPC.GetOrCreateInstance("global_input_hook_test");
            dataHook.onData += HookClientHelper_onData;

            Console.ReadLine();
            
            dataHook.Dispose();
        }

        private static void HookClientHelper_onData(SHookData hookData)
        {
            switch (hookData.hookEvent)
            {
                case EHookEvent.KeyboardKeyUp:
                case EHookEvent.KeyboardKeyDown:
                    LogKeyboardEvent(hookData);
                    break;
                case EHookEvent.MouseButtonUp:
                case EHookEvent.MouseButtonDown:
                case EHookEvent.MouseMove:
                    LogMouseEvent(hookData);
                    break;
                default: //EHookEvent.None, this is the default case and will also be set if a data request was sent manually.
                    LogKeyboardEvent(hookData);
                    LogMouseEvent(hookData);
                    break;
            }
        }

        private static void LogKeyboardEvent(SHookData hookData)
        {
            Console.WriteLine($"PRESSED_KEYBOARD_KEYS: {string.Join(", ", hookData.pressedKeyboardKeys)}");
            //Example output: PRESSED_KEYBOARD_KEYS
        }

        private static void LogMouseEvent(SHookData hookData)
        {
            if (hookData.hookEvent == EHookEvent.MouseMove || hookData.hookEvent == EHookEvent.None)
            {
                Console.WriteLine("MOUSE_POSITION: "
                    + $"{hookData.mousePosition.x}"
                    + $", {hookData.mousePosition.y}"
                ); //Example output: MOUSE_POSITION: 674, 362
            }

            if (hookData.hookEvent != EHookEvent.MouseMove)
            {
                Console.WriteLine($"PRESSED_MOUSE_BUTTONS: {string.Join(", ", hookData.pressedMouseButtons)}");
                //Example output: PRESSED_MOUSE_BUTTONS: LeftButton, RightButton
            }
        }
    }
}
