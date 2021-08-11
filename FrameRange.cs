namespace IracingReplayCapture
{
    public struct FrameRange
    {
        public uint from;
        public uint? to;

        public override string ToString()
        {
            var suffix = to == null ? "the end" : $"frame {to}";
            return $"From frame {from} to {suffix}";
        }
    }
}
