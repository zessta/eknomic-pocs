using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Raven.Client.Documents;
using SignalRtc.Models;
using SignalRtc.Storage;

namespace SignalRtc.Hubs
{
    public class SignalRtcHub : Hub
    {
        // Track active users
        private static ConcurrentDictionary<string, UserInfo> activeUsers = new ConcurrentDictionary<string, UserInfo>();

        // RavenDB store
        private readonly IDocumentStore _store = RavenDbStore.GetStore();

        // Called when a new user joins the chat
        public async Task NewUser(string username)
        {
            var userInfo = new UserInfo
            {
                UserName = username,
                ConnectionId = Context.ConnectionId
            };

            // Add the new user to the active users list
            activeUsers.TryAdd(Context.ConnectionId, userInfo);

            // Notify all clients about the new list of active users
            await UpdateAllClientsWithUserList();
        }

        // Send message to all users
        public async Task SendMessageToAll(string message)
        {
            var user = activeUsers.GetValueOrDefault(Context.ConnectionId);
            if (user != null)
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

                // Send message to all clients
                await Clients.All.SendAsync("ReceiveMessage", user.UserName, message, null);
            }
        }

        // Send a private message to a specific user
        public async Task SendMessageToUser(string message, string targetConnectionId)
        {
            var sender = activeUsers.GetValueOrDefault(Context.ConnectionId);
            var receiver = activeUsers[targetConnectionId];
            try
            {
                if (sender != null && activeUsers.ContainsKey(targetConnectionId))
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

                    // Send message to the targeted user
                    await Clients.Client(targetConnectionId).SendAsync("ReceiveMessage", sender.UserName, message, receiver.UserName);
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        // Retrieve chat session history between two users
        public async Task<List<ChatSession>> GetChatHistory(string fromUser, string toUser)
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

        // Handle disconnection of users
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (activeUsers.TryRemove(Context.ConnectionId, out UserInfo disconnectedUser))
            {
                // Notify all clients about the updated user list
                await UpdateAllClientsWithUserList();
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Get users list only
        public async Task<string> GetUserList()
        {
            var userListJson = JsonSerializer.Serialize(activeUsers.Values);
            return userListJson;
        }

        // Notify all clients about the updated list of active users
        private async Task UpdateAllClientsWithUserList()
        {
            var userListJson = JsonSerializer.Serialize(activeUsers.Values);
            await Clients.All.SendAsync("UpdateUserList", userListJson); // Broadcast the list to all users
        }
    }
}
