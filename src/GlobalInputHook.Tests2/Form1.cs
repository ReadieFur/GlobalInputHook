using System;
using System.Windows.Forms;
using GlobalInputHook.Objects;

namespace GlobalInputHook.Tests2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public void HookClientHelper_onData(SHookData hookData)
        {
            string data = string.Empty;
            
            switch (hookData.hookEvent)
            {
                case EHookEvent.KeyboardKeyUp:
                case EHookEvent.KeyboardKeyDown:
                    data += LogKeyboardEvent(hookData);
                    break;
                case EHookEvent.MouseButtonUp:
                case EHookEvent.MouseButtonDown:
                case EHookEvent.MouseMove:
                    data += LogMouseEvent(hookData);
                    break;
                default: //EHookEvent.None, this is the default case and will also be set if a data request was sent manually.
                    data += LogKeyboardEvent(hookData);
                    data += LogMouseEvent(hookData);
                    break;
            }

            if (data == string.Empty) return;

            richTextBox1.Text += data + Environment.NewLine;

            //Scroll to the bottom of the textbox.
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private string LogKeyboardEvent(SHookData hookData)
        {
            return $"PRESSED_KEYBOARD_KEYS: {string.Join(", ", hookData.pressedKeyboardKeys)}";
            //Example output: PRESSED_KEYBOARD_KEYS
        }

        private string LogMouseEvent(SHookData hookData)
        {
            string data = string.Empty;

            if (hookData.hookEvent == EHookEvent.MouseMove || hookData.hookEvent == EHookEvent.None)
            {
                data += "MOUSE_POSITION: "
                    + $"{hookData.mousePosition.x}"
                    + $", {hookData.mousePosition.y}";
                //Example output: MOUSE_POSITION: 674, 362
            }

            if (hookData.hookEvent != EHookEvent.MouseMove)
            {
                data += $"PRESSED_MOUSE_BUTTONS: {string.Join(", ", hookData.pressedMouseButtons)}";
                //Example output: PRESSED_MOUSE_BUTTONS: LeftButton, RightButton
            }

            return data;
        }
    }
}
