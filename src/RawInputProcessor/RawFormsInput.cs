using RawInputProcessor.Win32;
using System;
using System.Windows.Forms;

namespace RawInputProcessor
{
    public class RawFormsInput : RawInput
    {
        // ReSharper disable once NotAccessedField.Local
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
        }

        public RawFormsInput(IntPtr parentHandle, RawInputCaptureMode captureMode, bool addMessageFilter = true)
            : base(parentHandle, captureMode)
        {
            if (addMessageFilter) //Windows 10及以上系统走OnThreadFilterMessage
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
            /// <summary>
            /// Gets or sets a value indicating whether or not the next keydown message
            /// should be filtered.
            /// </summary>
            private bool filterNext;

            private readonly RawFormsInput _rawFormsInput;

            public PreMessageFilter(RawFormsInput rawFormsInput)
            {
                _rawFormsInput = rawFormsInput;
            }

            public bool PreFilterMessage(ref Message m)
            {
                //return _rawFormsInput.KeyboardDriver.HandleMessage(m.Msg, m.WParam, m.LParam);

                if (m.Msg == Win32Consts.WM_INPUT)
                {
                    if (_rawFormsInput.KeyboardDriver.HandleMessage(m.Msg, m.WParam, m.LParam))
                        this.filterNext = true;
                    else
                        this.filterNext = false;
                }

                if (m.Msg == Win32Consts.WM_KEYDOWN && this.filterNext)
                {
                    return true;
                }

                if (m.Msg == Win32Consts.WM_KEYUP && this.filterNext)
                {
                    this.filterNext = false;
                }

                if (m.Msg == Win32Consts.WM_INPUT_DEVICE_CHANGE)
                {
                    _rawFormsInput.KeyboardDriver.HandleMessage(m.Msg, m.WParam, m.LParam);
                }

                return false;
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
                _rawFormsInput.KeyboardDriver.HandleMessage(message.Msg, message.WParam, message.LParam);
                base.WndProc(ref message);
            }
        }
    }
}