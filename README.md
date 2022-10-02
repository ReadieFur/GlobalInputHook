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

Once you have met the prerequisites, add `GlobalInputHook` to your project references and you will be able to use the helper functions I have provided (under the namespace `GlobalInputHook.Tools`) to work with the program and shared memory object, here is a basic demo of how to use it (This code can also be found in the test project [here](https://github.com/ReadieFur/GlobalInputHook/tree/development/src/GlobalInputHook.Tests)):  
#### Creating an instance of the hook program:  
There are two ways of creating an instance of the hook program.  
One method will use an IPC channel which will run the hook program under a seperate child process (useful for mismatched framework versions). This method requres the `GlobalInputHook.IPC` release files on top of the `GlobalInputHook` release files.  
The other method will run the hook program in the same process as your application.
##### IPC Method:
```cs
using GlobalInputHook.IPC.Tools;

//You must provide a unique IPC name for the program here.
/*You can optinally also provide the path for the hook program here, the binary must however be called `GlobalInputHook.exe`
By default the program will look in the current working directory for the binary.*/
IPCInstanceHelper dataHook = IPCInstanceHelper.GetOrCreateInstance("global_input_hook");
```
##### DLL Method:
```cs
using GlobalInputHook.Tools;

DLLInstanceHelper.StartHookOnNewSTAMessageThread(1); //Optionally specify the maximum update rate in ms.
//OR, If your program is already running on an STA thread WITH a message loop, feel free to use this instead.
DLLInstanceHelper.Hook();
```
#### Getting the hook data:  
The following event will only fire when the data is different from the previous data:  
##### IPC Method:
```cs
dataHook.OnData += HookClientHelper_OnData;
```
##### DLL Method:
```cs
DLLInstanceHelper.OnData += HookClientHelper_OnData;
```
#### Using the event data:  
This hook program will remain mostly basic sending you events of the currently pressed keys and mouse buttons as well as the mouse position.  
It **is safe** to do calculation inside these events as they will not slow down the hook program at all.
```cs
using System;
using GlobalInputHook.Objects;

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
| C# Named Pipes | https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-use-named-pipes-for-network-interprocess-communication |
