using Microsoft.EntityFrameworkCore;
using SignalRtc.Models;
using SignalRtc.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace SignalRtc
{
    public interface IChatPgJsonStoreProvider : IChatProvider
    {
    }

    public class ChatPgJsonStoreProvider : IChatPgJsonStoreProvider
    {
        private readonly PgdbJsonContext _context;
        public ChatPgJsonStoreProvider(PgdbJsonContext context)
        {
            _context = context;
        }

        public async Task<List<ChatSession>> FetchChatHistory(string fromUser, string toUser)
        {
            var models = await _context.ChatJsonModels.Select(t => t.ChatSession).ToListAsync();

            var filteredChatHistory = models
                .Where(cs => cs.RootElement.GetProperty("FromUser").GetString() == fromUser &&
                             cs.RootElement.GetProperty("ToUser").GetString() == toUser ||
                             cs.RootElement.GetProperty("FromUser").GetString() == toUser &&
                             cs.RootElement.GetProperty("ToUser").GetString() == fromUser)
                .OrderBy(cs => cs.RootElement.GetProperty("Timestamp").GetDateTime())
                .ToList();

            var chatHistory = filteredChatHistory.Select(t => t.Deserialize<ChatSession>()).ToList();
            return chatHistory;
        }

        public async Task SaveSentAllMessage(string message, UserInfo user)
        {
            var chatSession = new ChatSession
            {
                FromUser = user.UserName,
                Message = message
            };

            _context.ChatJsonModels.Add(ConvertToJsonDoc(chatSession));
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

            _context.ChatJsonModels.Add(ConvertToJsonDoc(chatSession));
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
                _context.ChatJsonModels.Add(ConvertToJsonDoc(chatSession));
                await _context.SaveChangesAsync();
            }
            catch (System.Exception e)
            {
                throw;
            }
        }

        private ChatJsonModel ConvertToJsonDoc(ChatSession chatSession)
        {
            var chatJsonModel = new ChatJsonModel()
            {
                ChatSession = JsonSerializer.SerializeToDocument(chatSession)
            };
            return chatJsonModel;
        }
    }
}
