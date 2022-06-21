# Global Input Hook  
A simple program for hooking into Windows input.  

## Why did I create this?  
While there are other tools out there like [globalmousekeyhook](https://github.com/gmamaladze/globalmousekeyhook), which does work "fine", in certain C# versions it will cause the system input to become extremely slow which is a VERY big problem for the user experience.  
In my sources list below I have listed two links as to why the problem may occur. In reading this I decided to create this program which makes use of a WinForms application to hook into the system input and then share that data using a shared memory object. This way the program will remain fast and not slowed down by your C# application (assuming you aren't using WinForms).  

## Developer usage:  
### Something to note:
From my testing this program has been able to capture system wide input just fine with no noticable input delay. However the program will not be able to capture input from any application that is running with higher privileges than the hook application. This program is also only built to run on windows.
#### Prerequisites:  
- To use this program your application must be running on Windows and have the program files in your build folder. The latest release of this project can be grabbed [here](github.com/ReadieFur/GlobalInputHook/releases/latest).  

Once you have met the prerequisites, add the `GlobalInputHook.exe` to your project references and you will be able to use the helper functions I have provided (under the namespace `GlobalInputHook.Tools`) to work with the program and shared memory object, here is a basic demo of how to use it (This code can also be found in the test project [here](https://github.com/ReadieFur/GlobalInputHook/tree/development/src/GlobalInputHook.Tests)):  
#### Creating an instance of the hook program:  
```cs
using GlobalInputHook.Tools;

//You must provide a unique IPC name for the program here.
/*You can optinally also provide the path for the hook program here, the binary must however be called `GlobalInputHook.exe`
By default the program will look in the current working directory for the binary.*/
HookClientHelper dataHook = HookClientHelper.GetOrCreateInstance("global_input_hook");
```
#### Getting the hook data:  
The following events will only fire when the data is different from the previous data:  
```cs
dataHook.keyboardEvent += HookClientHelper_KeyboardEvent;
dataHook.mouseEvent += HookClientHelper_MouseEvent;
```
#### Using the event data:  
Becuase this hook program remains basic, it will only give you the data from the hook, it will not keep track of how long keys are pressed, etc. It is up to you on how you would like to use this data once you obtain it. It **is safe** to do calculation inside these events as they will not slow down the hook program at all.
```cs
using GlobalInputHook.Objects;
using Forms = System.Windows.Forms;

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
```
#### Make sure you dispose of the hook program!
Becuase this hook deals with unmanaged memory it is important to make sure you dispose of it when you are done with it.
```cs
dataHook.Dispose();
```

## Sources:  
A list of sources I used to create this program:  
| Used For | URL |
| --- | --- |
| C# Hooks | https://stackoverflow.com/questions/7497024/how-to-detect-mouse-clicks |
| C# Shared Memory | https://docs.microsoft.com/en-us/dotnet/api/system.io.memorymappedfiles.memorymappedfile |
| Understanding why the hooks were slow outside of WinForms | https://docs.microsoft.com/en-us/windows/win32/winauto/out-of-context-hook-functions  https://stackoverflow.com/questions/7233610/hwnd-message-hook-in-winforms |