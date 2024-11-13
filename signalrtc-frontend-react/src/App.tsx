import React, { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

interface ChatMessage {
    user: string;
    message: string | undefined;
    receiver: string;
}

interface UserInfo {
    ConnectionId: string;
    UserName: string;
    Message: string;
    Messages: ChatMessage[]; // Add this to track messages for each user
}

interface GroupInfo {
    GroupId: string;
    GroupName: string;
    Members: UserInfo[];
    Messages: ChatMessage[]; // Track messages for each group
}

const ChatApp: React.FC = () => {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [message, setMessage] = useState<string>('');
    const [groupName, setGroupName] = useState<string>('');
    const [selectedUsers, setSelectedUsers] = useState<string[]>([]); // Selected users for the group

    const [username, setUsername] = useState<string>('');
    const [users, setUsers] = useState<UserInfo[]>([]); // Manage the list of users
    const [groups, setGroups] = useState<GroupInfo[]>([]); // Track active groups
    const [isJoined, setIsJoined] = useState<boolean>(false);

    const joinChat = async () => {
        if (!username) return;

        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl('http://localhost:2511/signalrtc') // Change to your backend URL
            .withAutomaticReconnect()
            .build();

        try {
            await newConnection.start();
            console.log('Connected to SignalR server');
            console.log('connectionId', newConnection.connectionId);

            // Invoke NewUser method on the server
            await newConnection.invoke('NewUser', username);

            // when user joins, invoke a method to get the current user list
            const userListJson = await newConnection.invoke('GetUserList');
            const updatedUsers = JSON.parse(userListJson) as UserInfo[];
            // Add an empty messages array for each user
            setUsers(updatedUsers.map(user => ({ ...user, Messages: [] })));

            // Set connection in state
            setConnection(newConnection);
            setIsJoined(true);

        } catch (err) {
            console.log('Error connecting to SignalR: ', err);
        }
    };

    useEffect(() => {
        if (connection) {
            // Listen for incoming messages
            connection.on('ReceiveMessage', (user: string, message: string, receiver: string) => {
                setUsers(prevUsers =>
                    prevUsers.map(u =>
                        (u.UserName === user || u.UserName === receiver)
                            ? { ...u, Messages: [...u.Messages, { user, message, receiver }] }
                            : u
                    )
                );
            });

            // Listen for incoming group messages
            connection.on('ReceiveGroupMessage', (user: string, message: string, groupName: string) => {
                setGroups(prevGroups =>
                    prevGroups.map(g =>
                        g.GroupName === groupName
                            ? { ...g, Messages: [...g.Messages, { user, message, receiver: groupName }] }
                            : g
                    )
                );
            });

            // Listen for the updated user list and update users
            connection.on('UpdateUserList', (userListJson: string) => {
                const updatedUsers = JSON.parse(userListJson) as UserInfo[];
                setUsers(updatedUsers.map(user => ({ ...user, Messages: [] }))); // Update the user list in the frontend
            });

            // Listen for group creation
            connection.on('GroupCreated', (groupId: string, groupName: string) => {
                setGroups(prevGroups => [...prevGroups, { GroupId: groupId, GroupName: groupName, Members: [], Messages: [] }]);
            });

            // Listen for group deletion
            connection.on('GroupDeleted', (groupId: string) => {
                setGroups(prevGroups => prevGroups.filter(g => g.GroupId !== groupId));
            });

            return () => {
                connection.off('ReceiveMessage');
                connection.off('ReceiveGroupMessage');
                connection.off('UpdateUserList');
                connection.off('GroupCreated');
                connection.off('GroupDeleted');
            };
        }
    }, [connection]); // Only re-run if the connection changes

    const sendMessageToAll = async () => {
        if (connection && message) {
            try {
                await connection.invoke('SendMessageToAll', message);
                setUsers(prevUsers =>
                    prevUsers.map(u =>
                        u.UserName === username
                            ? { ...u, Messages: [...u.Messages, { user: 'Me', message, receiver: '' }] }
                            : u
                    )
                );
                setMessage('');
            } catch (err) {
                console.error('Error sending message: ', err);
            }
        }
    };

    // Send message to a specific user
    const sendMessageToUser = async (targetConnectionId: string) => {
        const sentUser = users.find(u => u.ConnectionId === targetConnectionId);
        if (connection && sentUser?.Message && username) {
            try {
                await connection.invoke('SendMessageToUser', sentUser.Message, targetConnectionId);
                setUsers(prevUsers =>
                    prevUsers.map(u =>
                        u.ConnectionId === targetConnectionId
                            ? {
                                ...u,
                                Messages: [...u.Messages, { user: 'Me', message: sentUser.Message, receiver: sentUser.UserName }],
                                Message: '' // Clear input after sending
                            }
                            : u
                    )
                );
            } catch (err) {
                console.error('Error sending message: ', err);
            }
        }
    };

    // Create a new group with selected users
    const createGroup = async () => {
        const selectedUserIds = users.filter(u => u.UserName !== username).map(u => u.ConnectionId);
        if (connection && groupName && selectedUserIds.length > 0) {
            try {
                await connection.invoke('CreateGroup', groupName, selectedUserIds);
                setGroupName(''); // Clear the group name input
            } catch (err) {
                console.error('Error creating group: ', err);
            }
        }
    };

    // Send a message to a group
    const sendMessageToGroup = async (groupId: string) => {
        const group = groups.find(g => g.GroupId === groupId);
        if (connection && group?.GroupName && message) {
            try {
                await connection.invoke('SendMessageToGroup', group.GroupId, message);
                setGroups(prevGroups =>
                    prevGroups.map(g =>
                        g.GroupId === groupId
                            ? { ...g, Messages: [...g.Messages, { user: 'Me', message, receiver: group.GroupName }] }
                            : g
                    )
                );
                setMessage(''); // Clear message input after sending
            } catch (err) {
                console.error('Error sending group message: ', err);
            }
        }
    };

    const loadChatHistory = async (fromUser: string, toUser: string) => {
        if (connection) {
            try {
                const response = await connection.invoke("GetChatHistory", fromUser, toUser);
                const history = response.map((t: any) => ({ user: t.fromUser, message: t.message, receiver: toUser }));
                setUsers(prevUsers =>
                    prevUsers.map(u =>
                        u.UserName === toUser
                            ? { ...u, Messages: [...history] }
                            : u
                    )
                );
            } catch (err) {
                console.error('Error loading chat history: ', err);
            }
        }
    };
    // Add members to a group
    const addMembersToGroup = async (groupId: string) => {
        if (connection && selectedUsers.length > 0) {
            try {
                await connection.invoke('AddMembersToGroup', groupId, selectedUsers);
                setSelectedUsers([]); // Clear the selected users after adding
            } catch (err) {
                console.error('Error adding members to group: ', err);
            }
        }
    };
    const handleUserClick = (user: UserInfo) => {
        loadChatHistory(username, user.UserName); // Load chat history with the selected user
    };

    const updateMessageInput = (msg: string, user: UserInfo) => {
        setUsers(prevUsers =>
            prevUsers.map(u => u.ConnectionId === user.ConnectionId ? { ...u, Message: msg } : u)
        );
    };

    const updateGroupMessageInput = (msg: string, groupId: string) => {
        setMessage(msg); // Update message input for the group
    };

    // Select users to add to the group
    const handleUserSelection = (userId: string) => {
        setSelectedUsers(prev =>
            prev.includes(userId) ? prev.filter(id => id !== userId) : [...prev, userId]
        );
    };

    return (
        <div>
            <h2>SignalR Chat</h2>

            {!isJoined ? (
                <div>
                    <input
                        type="text"
                        placeholder="Enter username"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                    />
                    <button onClick={joinChat} disabled={!username}>
                        Join Chat
                    </button>
                </div>
            ) : (
                <div>
                        <p>Welcome, {username}!</p>
                        {/* Create Group */}
                        <input
                            type="text"
                            placeholder="Group Name"
                            value={groupName}
                            onChange={(e) => setGroupName(e.target.value)}
                        />
                        <button onClick={createGroup} disabled={!groupName || selectedUsers.length === 0}>
                            Create Group
                        </button>
                        <pre></pre>

                    {/* Send message to all */}
                    <input
                        type="text"
                        placeholder="Type a message"
                        value={message}
                        onChange={(e) => setMessage(e.target.value)}
                    />
                    <button onClick={sendMessageToAll} disabled={!message}>
                        Send to All
                    </button>

                    {/* List of users for private message */}
                    <div>
                        <h3>Users</h3>
                        <ul>
                            {users.map((user, index) => (
                                <li key={index} onClick={() => handleUserClick(user)} style={{ border: user ? '2px solid lightblue' : '0', margin: 5, width: 500, padding: 10 }}>

                                    <input
                                        type="checkbox"
                                        checked={selectedUsers.includes(user.ConnectionId)}
                                        onChange={() => handleUserSelection(user.ConnectionId)}
                                    />
                                    {user.UserName} (ID: {user.ConnectionId}){' '}
                                    <div>
                                            {/* Messages exchanged with the user */}
                                            <ul style={{ border: user.Messages.length===0?'0': '1px solid grey', margin: 5 }}>
                                            {user.Messages.map((msg, idx) => (
                                                <li key={idx}>
                                                    <strong>{msg.user}:</strong> {msg.message}
                                                </li>
                                            ))}
                                        </ul>

                                            {/* Input to send a message to the user */}
                                            <input style={{ verticalAlign: 'center' }}
                                            key={user.ConnectionId}
                                            type="text"
                                            placeholder="Type a message"
                                            value={user.Message}
                                            onChange={(e) => updateMessageInput(e.target.value, user)}
                                        />
                                    </div>
                                    <button onClick={() => sendMessageToUser(user.ConnectionId)}>
                                        Send Private Message
                                    </button>

                                    <pre>

                                    </pre>
                                </li>
                            ))}
                        </ul>
                        </div>

                        {/* List of groups */}
                        <div>
                            <h3>Groups</h3>
                            <ul>
                                {groups.map((group, index) => (
                                    <li key={index}>
                                        <div>
                                            <h4>{group.GroupName}</h4>
                                            <ul style={{ border: group.Messages.length === 0 ? '0' : '1px solid grey', margin: 5 }}>
                                                {group.Messages.map((msg, idx) => (
                                                    <li key={idx}>
                                                        <strong>{msg.user}:</strong> {msg.message}
                                                    </li>
                                                ))}
                                            </ul>

                                            {/* Add members to the group */}
                                            <button onClick={() => addMembersToGroup(group.GroupId)}>
                                                Add Selected Users to Group
                                            </button>

                                            {/* Input to send a message to the group */}
                                            <input
                                                type="text"
                                                placeholder="Type a message"
                                                value={message}
                                                onChange={(e) => setMessage(e.target.value)}
                                            />
                                            <button onClick={() => sendMessageToGroup(group.GroupId)}>
                                                Send Group Message
                                            </button>
                                        </div>
                                    </li>
                                ))}
                            </ul>
                        </div>
                </div>
            )}
        </div>
    );
};

export default ChatApp;
