namespace SignalRtc.Hubs
{
    public class MediaStream
    {
        public byte[] VideoData { get; set; }

        public MediaStream(byte[] videoData)
        {
            VideoData = videoData;
        }
    }
}