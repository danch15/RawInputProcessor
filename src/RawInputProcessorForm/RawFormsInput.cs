using RawInputProcessor.Win32;
using System;
using System.Windows.Forms;

namespace RawInputProcessor
{
    public class RawFormsInput : RawInput
    {
        private readonly LegacyKeyboardMessageSuppressor messageSuppressor = new LegacyKeyboardMessageSuppressor();
        private RawInputNativeWindow _window;
        private PreMessageFilter _filter;

        public override void AddMessageFilter()
        {
            if (_filter != null)
            {
                return;
            }
            _filter = new PreMessageFilter(this);
            Application.AddMessageFilter(_filter);
        }

        public override void RemoveMessageFilter()
        {
            if (_filter == null)
            {
                return;
            }
            Application.RemoveMessageFilter(_filter);
            _filter = null;
        }

        public RawFormsInput(IntPtr parentHandle, RawInputCaptureMode captureMode, bool addMessageFilter = true)
            : base(parentHandle, captureMode, peekMessage: !addMessageFilter)
        {
            if (addMessageFilter)
                AddMessageFilter();
            else
                _window = new RawInputNativeWindow(this, parentHandle);
        }

        public RawFormsInput(IWin32Window window, RawInputCaptureMode captureMode, bool addMessageFilter = true)
            : this(window.Handle, captureMode, addMessageFilter)
        {
        }

        private class PreMessageFilter : IMessageFilter
        {
            private readonly RawFormsInput _rawFormsInput;

            public PreMessageFilter(RawFormsInput rawFormsInput)
            {
                _rawFormsInput = rawFormsInput;
            }

            public bool PreFilterMessage(ref Message m)
            {
                var result = _rawFormsInput.KeyboardDriver.HandleMessageResult(m.Msg, m.WParam, m.LParam);
                if (result.Handled)
                {
                    _rawFormsInput.messageSuppressor.Enqueue(result.Message, result.VirtualKey);
                }

                return _rawFormsInput.messageSuppressor.ShouldSuppress(m.Msg, m.WParam);
            }
        }

        private class RawInputNativeWindow : NativeWindow
        {
            private readonly RawFormsInput _rawFormsInput;

            public RawInputNativeWindow(RawFormsInput rawFormsInput, IntPtr parentHandle)
            {
                _rawFormsInput = rawFormsInput;
                AssignHandle(parentHandle);
            }

            protected override void WndProc(ref Message message)
            {
                var result = _rawFormsInput.KeyboardDriver.HandleMessageResult(message.Msg, message.WParam, message.LParam);
                if (result.Handled)
                {
                    _rawFormsInput.messageSuppressor.Enqueue(result.Message, result.VirtualKey);
                }

                if (_rawFormsInput.messageSuppressor.ShouldSuppress(message.Msg, message.WParam))
                {
                    return;
                }

                base.WndProc(ref message);
            }
        }
    }
}