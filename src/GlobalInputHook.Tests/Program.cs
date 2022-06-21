using GlobalInputHook.Objects;
using GlobalInputHook.Tools;
using Forms = System.Windows.Forms;

HookClientHelper dataHook = HookClientHelper.GetOrCreateInstance("global_input_hook");
dataHook.keyboardEvent += HookClientHelper_KeyboardEvent;
dataHook.mouseEvent += HookClientHelper_MouseEvent;

void HookClientHelper_KeyboardEvent(SKeyboardEventData keyboardEventData)
{
    Console.WriteLine("HookClientHelper_KeyboardEvent"
        + $" KeyEvent:{Enum.GetName(keyboardEventData.eventType)}"
        + $" KeyCode:{keyboardEventData.keyCode}"
        + $" Key:{(Forms.Keys)keyboardEventData.keyCode}"
    );
}

void HookClientHelper_MouseEvent(SMouseEventData mouseEventData)
{
    Console.WriteLine("HookClientHelper_MouseEvent"
        + $" MouseEvent:{Enum.GetName(mouseEventData.eventType)}"
        + $" X:{mouseEventData.cursorPosition.x}"
        + $" Y:{mouseEventData.cursorPosition.y}"
    );
}

Console.ReadLine();
dataHook.Dispose();
