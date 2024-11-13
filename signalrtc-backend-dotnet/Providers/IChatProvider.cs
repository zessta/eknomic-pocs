using SignalRtc.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalRtc
{
    public interface IChatProvider
    {
        Task SaveSentAllMessage(string message, UserInfo user);
        Task SaveSentMessage(string message, UserInfo sender, UserInfo receiver);
        Task<List<ChatSession>> FetchChatHistory(string fromUser, string toUser);
        Task SaveSentGroupMessage(string message, GroupInfo group, UserInfo sender);
    }
}
