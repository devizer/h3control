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
    }
}