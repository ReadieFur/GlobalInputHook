using GlobalInputHook.Objects;
using GlobalInputHook.Tools;
using Forms = System.Windows.Forms;

//You must provide a unique IPC name for the program here.
/*You can optinally also provide the path for the hook program here, the binary must however be called `GlobalInputHook.exe`
By default the program will look in the current working directory for the binary.*/
HookClientHelper dataHook = HookClientHelper.GetOrCreateInstance("global_input_hook");

dataHook.keyboardEvent += HookClientHelper_KeyboardEvent;
dataHook.mouseEvent += HookClientHelper_MouseEvent;

void HookClientHelper_KeyboardEvent(SKeyboardEventData keyboardEventData)
{
    Console.WriteLine("HookClientHelper_KeyboardEvent"
        + $" KeyEvent:{Enum.GetName(keyboardEventData.eventType)}"
        + $" KeyCode:{keyboardEventData.keyCode}"
        + $" Key:{(Forms.Keys)keyboardEventData.keyCode}"
    ); //Example output: ...KeyEvent:KEY_DOWN KeyCode:75 Key:K
}

void HookClientHelper_MouseEvent(SMouseEventData mouseEventData)
{
    Console.WriteLine("HookClientHelper_MouseEvent"
        + $" MouseEvent:{Enum.GetName(mouseEventData.eventType)}"
        + $" X:{mouseEventData.cursorPosition.x}"
        + $" Y:{mouseEventData.cursorPosition.y}"
    ); //Example output: ...MouseEvent:MOUSE_MOVE X:674 Y:362
}

Console.ReadLine();
dataHook.Dispose();
