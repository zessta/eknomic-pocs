using System;

namespace SignalRtc.Models
{
    public class UserInfo
    {
        public string UserName { get; set; }
        public string ConnectionId { get; set; }
    }

    public class ChatSession
    {
        public string Id { get; set; }
        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

}
