using Raven.Client.Documents;
using SignalRtc.Models;
using SignalRtc.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRtc
{
    public interface IChatRavendbProvider : IChatProvider
    {
        
    }

    public class ChatravendbProvider : IChatRavendbProvider
    {
        // RavenDB store
        private readonly IDocumentStore _store = RavenDbStore.GetStore();

        public async Task SaveSentAllMessage(string message, UserInfo user)
        {
            // Store message in RavenDB
            using (var session = _store.OpenAsyncSession())
            {
                var chatSession = new ChatSession
                {
                    FromUser = user.UserName,
                    Message = message
                };
                await session.StoreAsync(chatSession);
                await session.SaveChangesAsync();
            }
        }

        public async Task SaveSentMessage(string message, UserInfo sender, UserInfo receiver)
        {
            // Store private message in RavenDB
            using (var session = _store.OpenAsyncSession())
            {
                var chatSession = new ChatSession
                {
                    FromUser = sender.UserName,
                    ToUser = receiver.UserName,
                    Message = message
                };
                await session.StoreAsync(chatSession);
                await session.SaveChangesAsync();
            }
        }


        public async Task<List<ChatSession>> FetchChatHistory(string fromUser, string toUser)
        {
            using (var session = _store.OpenAsyncSession())
            {
                // Query chat history from RavenDB for a specific pair of users
                var chatHistory = await session.Query<ChatSession>()
                                               .Where(cs => cs.FromUser == fromUser && cs.ToUser == toUser ||
                                                            cs.FromUser == toUser && cs.ToUser == fromUser)
                                               .OrderBy(cs => cs.Timestamp)
                                               .ToListAsync();
                return chatHistory;
            }
        }


        public async Task SaveSentGroupMessage(string message, GroupInfo group, UserInfo sender)
        {
            // Store group message in RavenDB
            using (var session = _store.OpenAsyncSession())
            {
                var chatSession = new ChatSession
                {
                    FromUser = sender.UserName,
                    Message = message,
                    GroupName = group.GroupName
                };
                await session.StoreAsync(chatSession);
                await session.SaveChangesAsync();
            }
        }
    }
}
