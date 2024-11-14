using Microsoft.EntityFrameworkCore;
using SignalRtc.Models;
using SignalRtc.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRtc
{
    public interface IChatPgStoreProvider : IChatProvider
    {

    }

    public class ChatPgStoreProvider : IChatPgStoreProvider
    {
        private readonly PgdbContext _context;
        public ChatPgStoreProvider(PgdbContext context)
        {
            _context = context;
            
        }
        public async Task<List<ChatSession>> FetchChatHistory(string fromUser, string toUser)
        {
            // Query chat history from RavenDB for a specific pair of users
            var chatHistory = await _context.ChatSessions
                                           .Where(cs => cs.FromUser == fromUser && cs.ToUser == toUser ||
                                                        cs.FromUser == toUser && cs.ToUser == fromUser)
                                           .OrderBy(cs => cs.Timestamp)
                                           .ToListAsync();
            return chatHistory;
        }

        public async Task SaveSentAllMessage(string message, UserInfo user)
        {
            var chatSession = new ChatSession
            {
                FromUser = user.UserName,
                Message = message
            };
            _context.ChatSessions.Add(chatSession);
            await _context.SaveChangesAsync();
        }

        public async Task SaveSentGroupMessage(string message, GroupInfo group, UserInfo sender)
        {
            var chatSession = new ChatSession
            {
                FromUser = sender.UserName,
                Message = message,
                GroupName = group.GroupName
            };
            _context.ChatSessions.Add(chatSession);
            await _context.SaveChangesAsync();
        }

        public async Task SaveSentMessage(string message, UserInfo sender, UserInfo receiver)
        {
            try
            {
                var chatSession = new ChatSession
                {
                    FromUser = sender.UserName,
                    ToUser = receiver.UserName,
                    Message = message
                };
                _context.ChatSessions.Add(chatSession);
                await _context.SaveChangesAsync();
            }
            catch (System.Exception e)
            {
                throw;
            }
        }
    }
}
