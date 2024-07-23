using RawInputProcessor.Win32;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace RawInputProcessor
{
    public class RawPresentationInput : RawInput
    {

        /// <summary>
        /// Gets or sets a value indicating whether or not the next keydown message
        /// should be filtered.
        /// </summary>
        private bool filterNext;

        private bool _hasFilter;

        public RawPresentationInput(HwndSource hwndSource, RawInputCaptureMode captureMode, bool addMessageFilter)
            : base(hwndSource.Handle, captureMode, !addMessageFilter)
        {
            if (addMessageFilter) //Windows 10及以上系统走OnThreadFilterMessage
                AddMessageFilter();
            else
                hwndSource.AddHook(Hook); //Windows 10以下系统走Hook
        }

        public RawPresentationInput(Visual visual, RawInputCaptureMode captureMode, bool addMessageFilter = true)
            : this(GetHwndSource(visual), captureMode, !addMessageFilter)
        {
        }

        private static HwndSource GetHwndSource(Visual visual)
        {
            var source = PresentationSource.FromVisual(visual) as HwndSource;
            if (source == null)
            {
                throw new InvalidOperationException("Cannot find a valid HwndSource");
            }
            return source;
        }

        private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            KeyboardDriver.HandleMessage(msg, wparam, lparam);
            return IntPtr.Zero;
        }

        public override void AddMessageFilter()
        {
            if (_hasFilter)
            {
                return;
            }
            ComponentDispatcher.ThreadFilterMessage += OnThreadFilterMessage;
            _hasFilter = true;
        }

        public override void RemoveMessageFilter()
        {
            ComponentDispatcher.ThreadFilterMessage -= OnThreadFilterMessage;
            _hasFilter = false;
        }

        // ReSharper disable once RedundantAssignment
        private void OnThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            //handled = KeyboardDriver.HandleMessage(msg.message, msg.wParam, msg.lParam); //Windows 10以下系统才能PeekMessage
            
            if (msg.message == Win32Consts.WM_INPUT)
            {
                if (KeyboardDriver.HandleMessage(msg.message, msg.wParam, msg.lParam))
                    this.filterNext = true;
                else
                    this.filterNext = false;
            }

            if (msg.message == Win32Consts.WM_KEYDOWN && this.filterNext)
            {
                handled = true;
            }

            if (msg.message == Win32Consts.WM_KEYUP && this.filterNext)
            {
                this.filterNext = false;
            }

            if (msg.message == Win32Consts.WM_INPUT_DEVICE_CHANGE)
            {
                KeyboardDriver.HandleMessage(msg.message, msg.wParam, msg.lParam);
            }
        }
    }
}