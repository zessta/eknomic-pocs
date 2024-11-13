using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SignalRtc.Models
{
    public class UserInfo
    {
        public string UserName { get; set; }

        [Key]
        public string ConnectionId { get; set; }
    }

    public class ChatSession
    {
        [Key]
        public string ChatSessionId { get; set; }
        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string GroupName { get; set; }
    }

    public class GroupInfo
    {
        [Key]
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string CreatorConnectionId { get; set; }
        public List<UserInfo> Members { get; set; }
    }
}
