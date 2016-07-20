namespace H3Control
{
    public class MemInfo_OnLinix
    {
        public long Total { get; set; }
        public long Free { get; set; }
        public long BuffersAndCache { get; set; }
        public long SwapTotal { get; set; }
        public long SwapFree { get; set; }
        public long Buffers { get; set; }

        public override string ToString()
        {
            return string.Format("Total: {0}, Free: {1}, BuffersAndCache: {2}, SwapTotal: {3}, SwapFree: {4}, Buffers: {5}", Total, Free, BuffersAndCache, SwapTotal, SwapFree, Buffers);
        }
    }
}