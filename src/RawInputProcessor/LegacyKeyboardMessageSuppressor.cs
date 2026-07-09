using System;
using System.Collections.Generic;
using RawInputProcessor.Win32;

namespace RawInputProcessor
{
    internal sealed class LegacyKeyboardMessageSuppressor
    {
        private readonly Dictionary<SuppressedKey, int> pendingMessages = new Dictionary<SuppressedKey, int>();

        public void Enqueue(int message, int virtualKey)
        {
            if (!IsLegacyKeyboardMessage(message))
            {
                return;
            }

            var key = new SuppressedKey(message, virtualKey);
            pendingMessages.TryGetValue(key, out int count);
            pendingMessages[key] = count + 1;
        }

        public bool ShouldSuppress(int message, IntPtr wParam)
        {
            if (!IsLegacyKeyboardMessage(message))
            {
                return false;
            }

            var key = new SuppressedKey(message, unchecked((int)wParam.ToInt64()));
            if (!pendingMessages.TryGetValue(key, out int count))
            {
                return false;
            }

            if (count == 1)
            {
                pendingMessages.Remove(key);
            }
            else
            {
                pendingMessages[key] = count - 1;
            }

            return true;
        }

        private static bool IsLegacyKeyboardMessage(int message)
        {
            return message == Win32Consts.WM_KEYDOWN
                || message == Win32Consts.WM_KEYUP
                || message == Win32Consts.WM_SYSKEYDOWN
                || message == Win32Consts.WM_SYSKEYUP;
        }

        private readonly struct SuppressedKey : IEquatable<SuppressedKey>
        {
            private readonly int message;
            private readonly int virtualKey;

            public SuppressedKey(int message, int virtualKey)
            {
                this.message = message;
                this.virtualKey = virtualKey;
            }

            public bool Equals(SuppressedKey other)
            {
                return message == other.message && virtualKey == other.virtualKey;
            }

            public override bool Equals(object obj)
            {
                return obj is SuppressedKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (message * 397) ^ virtualKey;
                }
            }
        }
    }
}
