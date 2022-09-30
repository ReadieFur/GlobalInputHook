using System;
using System.Windows.Forms;
using GlobalInputHook.Tools;

namespace GlobalInputHook.Tests2
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Form1 form1 = new();

            HookClientHelperInternal.StartHook();
            HookClientHelperInternal.OnUpdate += form1.HookClientHelper_onData;

            Application.Run(form1);
        }
    }
}
