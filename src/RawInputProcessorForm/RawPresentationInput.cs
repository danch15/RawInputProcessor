using RawInputProcessor.Win32;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace RawInputProcessor
{
    public class RawPresentationInput : RawInput
    {
        private readonly LegacyKeyboardMessageSuppressor messageSuppressor = new LegacyKeyboardMessageSuppressor();
        private bool _hasFilter;

        public RawPresentationInput(HwndSource hwndSource, RawInputCaptureMode captureMode, bool addMessageFilter)
            : base(hwndSource.Handle, captureMode, peekMessage: !addMessageFilter)
        {
            if (addMessageFilter)
                AddMessageFilter();
            else
                hwndSource.AddHook(Hook);
        }

        public RawPresentationInput(Visual visual, RawInputCaptureMode captureMode, bool addMessageFilter = true)
            : this(GetHwndSource(visual), captureMode, addMessageFilter)
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
            var result = KeyboardDriver.HandleMessageResult(msg, wparam, lparam);
            if (result.Handled)
            {
                messageSuppressor.Enqueue(result.Message, result.VirtualKey);
            }

            if (messageSuppressor.ShouldSuppress(msg, wparam))
            {
                handled = true;
            }

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
            if (!_hasFilter)
            {
                return;
            }
            ComponentDispatcher.ThreadFilterMessage -= OnThreadFilterMessage;
            _hasFilter = false;
        }

        private void OnThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            var result = KeyboardDriver.HandleMessageResult(msg.message, msg.wParam, msg.lParam);
            if (result.Handled)
            {
                messageSuppressor.Enqueue(result.Message, result.VirtualKey);
            }

            if (messageSuppressor.ShouldSuppress(msg.message, msg.wParam))
            {
                handled = true;
            }
        }
    }
}