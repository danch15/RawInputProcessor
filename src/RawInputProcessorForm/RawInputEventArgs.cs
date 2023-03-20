using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace RawInputProcessor
{
    public sealed class RawInputEventArgs : EventArgs
    {
        public RawKeyboardDevice Device { get; private set; }
        public KeyPressState KeyPressState { get; private set; }
        public uint Message { get; private set; }
        public Keys Key { get; private set; }
        public int VirtualKey { get; private set; }
        public bool Handled { get; set; }

        internal RawInputEventArgs(RawKeyboardDevice device, KeyPressState keyPressState, uint message, Keys key,
            int virtualKey)
        {
            Device = device;
            KeyPressState = keyPressState;
            Message = message;
            Key = key;
            VirtualKey = virtualKey;
        }
    }
}