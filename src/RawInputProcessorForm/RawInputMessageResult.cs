namespace RawInputProcessor
{
    internal readonly struct RawInputMessageResult
    {
        public RawInputMessageResult(bool handled, int message, int virtualKey)
        {
            Handled = handled;
            Message = message;
            VirtualKey = virtualKey;
        }

        public bool Handled { get; }
        public int Message { get; }
        public int VirtualKey { get; }
    }
}
