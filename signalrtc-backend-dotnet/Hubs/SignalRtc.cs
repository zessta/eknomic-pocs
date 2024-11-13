using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using SignalRtc.Models;
using SignalRtc.Storage;

namespace SignalRtc.Hubs
{
    public class SignalRtcHub : Hub
    {
        // Track active users
        private static ConcurrentDictionary<string, UserInfo> activeUsers = new ConcurrentDictionary<string, UserInfo>();

        // Track groups
        private static ConcurrentDictionary<string, GroupInfo> activeGroups = new ConcurrentDictionary<string, GroupInfo>();

        //private readonly IServiceProvider serviceProvider;
       
        private readonly IChatProvider _chatService;
        public SignalRtcHub(IServiceProvider serviceProvider, IConfiguration configuration,LocalDbFactory localDbFactory)
        {
            var dbKey = configuration.GetRequiredSection("DatabaseToUse").Value;

            //_chatService = serviceProvider.GetRequiredService<IChatravendbProvider>();
            _chatService = localDbFactory.GetLocalDb(dbKey);
        }
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
                await _chatService.SaveSentAllMessage(message, user);

                // Send message to all clients
                await Clients.All.SendAsync("ReceiveMessage", user.UserName, message, null);
            }
        }


        // Send a private message to a specific user
        public async Task SendMessageToUser(string message, string targetConnectionId)
        {
            var sender = activeUsers.GetValueOrDefault(Context.ConnectionId);
            var receiver = activeUsers.GetValueOrDefault(targetConnectionId);
            if (sender != null && receiver != null)
            {
                await _chatService.SaveSentMessage(message, sender, receiver);

                // Send message to the targeted user
                await Clients.Client(targetConnectionId).SendAsync("ReceiveMessage", sender.UserName, message, receiver.UserName);
            }
        }

       

        // Retrieve chat session history between two users
        public async Task<List<ChatSession>> GetChatHistory(string fromUser, string toUser)
        {
            return await _chatService.FetchChatHistory(fromUser, toUser);
        }

        // Create a new group
        public async Task CreateGroup(string groupName, List<string> memberConnectionIds)
        {
            var creator = activeUsers.GetValueOrDefault(Context.ConnectionId);
            if (creator != null)
            {
                var groupInfo = new GroupInfo
                {
                    GroupId = Guid.NewGuid().ToString(),
                    GroupName = groupName,
                    CreatorConnectionId = Context.ConnectionId,
                    Members = new List<UserInfo> { creator }
                };

                // Add members to the group
                foreach (var memberId in memberConnectionIds)
                {
                    if (activeUsers.TryGetValue(memberId, out UserInfo user))
                    {
                        groupInfo.Members.Add(user);
                    }
                }

                // Add group to active groups
                activeGroups.TryAdd(groupInfo.GroupId, groupInfo);

                // Notify group members about the new group
                foreach (var member in groupInfo.Members)
                {
                    await Clients.Client(member.ConnectionId).SendAsync("GroupCreated", groupInfo.GroupId, groupInfo.GroupName);
                }
            }
        }

        // Send message to a group
        public async Task SendMessageToGroup(string groupId, string message)
        {
            if (activeGroups.TryGetValue(groupId, out GroupInfo group))
            {
                var sender = activeUsers.GetValueOrDefault(Context.ConnectionId);
                if (sender != null && group.Members.Any(m => m.ConnectionId == Context.ConnectionId))
                {
                    await _chatService.SaveSentGroupMessage(message, group, sender);

                    // Send message to all group members
                    foreach (var member in group.Members)
                    {
                        await Clients.Client(member.ConnectionId).SendAsync("ReceiveGroupMessage", sender.UserName, message, group.GroupName);
                    }
                }
            }
        }

        // Add a member to a group
        public async Task AddMemberToGroup(string groupId, string newMemberConnectionId)
        {
            if (activeGroups.TryGetValue(groupId, out GroupInfo group))
            {
                var creator = activeUsers.GetValueOrDefault(group.CreatorConnectionId);
                if (creator != null && creator.ConnectionId == Context.ConnectionId)
                {
                    if (activeUsers.TryGetValue(newMemberConnectionId, out UserInfo newMember))
                    {
                        group.Members.Add(newMember);

                        // Notify the new member and the group about the addition
                        await Clients.Client(newMember.ConnectionId).SendAsync("AddedToGroup", groupId, group.GroupName);
                        foreach (var member in group.Members)
                        {
                            await Clients.Client(member.ConnectionId).SendAsync("GroupMemberAdded", groupId, newMember.UserName);
                        }
                    }
                }
            }
        }

        // Delete a group (only the creator can delete)
        public async Task DeleteGroup(string groupId)
        {
            if (activeGroups.TryRemove(groupId, out GroupInfo group))
            {
                var creator = activeUsers.GetValueOrDefault(group.CreatorConnectionId);
                if (creator != null && creator.ConnectionId == Context.ConnectionId)
                {
                    // Notify all group members that the group has been deleted
                    foreach (var member in group.Members)
                    {
                        await Clients.Client(member.ConnectionId).SendAsync("GroupDeleted", groupId, group.GroupName);
                    }
                }
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
