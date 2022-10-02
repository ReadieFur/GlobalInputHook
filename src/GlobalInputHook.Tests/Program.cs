using System;
using GlobalInputHook.Objects;
using GlobalInputHook.Tools;
using GlobalInputHook.IPC.Tools;

namespace GlobalInputHook.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if true
            //DLL method.
            DLLInstanceHelper.OnUpdate += HookClientHelper_OnData;
            //DLLInstanceHelper.Hook(1, true); //If your program is already running on an STA thread WITH a message loop, feel free to use this instead.
            DLLInstanceHelper.StartHookOnNewSTAMessageThread(1);
            WaitForEnter();
            DLLInstanceHelper.Unhook();
#else
            //IPC method.
            IPCInstanceHelper instance = IPCInstanceHelper.GetOrCreateInstance("global_input_hook_test");
            instance.OnUpdate += HookClientHelper_OnData;
            WaitForEnter();
            instance.Dispose();
#endif
        }

        private static void WaitForEnter()
        {
            while (true) if (Console.ReadKey(true).Key == ConsoleKey.Enter) break;
        }


        private static void HookClientHelper_OnData(SHookData hookData)
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
