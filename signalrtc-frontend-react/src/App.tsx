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

const ChatApp: React.FC = () => {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [message, setMessage] = useState<string>('');
    const [username, setUsername] = useState<string>('');
    const [users, setUsers] = useState<UserInfo[]>([]); // Manage the list of users
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

            // Listen for the updated user list and update users
            connection.on('UpdateUserList', (userListJson: string) => {
                const updatedUsers = JSON.parse(userListJson) as UserInfo[];
                setUsers(updatedUsers.map(user => ({ ...user, Messages: [] }))); // Update the user list in the frontend
            });

            return () => {
                connection.off('ReceiveMessage');
                connection.off('UpdateUserList');
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

    // Call GetChatHistory when a chat with a specific user is opened
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

    const handleUserClick = (user: UserInfo) => {
        loadChatHistory(username, user.UserName); // Load chat history with the selected user
    };

    const updateMessageInput = (msg: string, user: UserInfo) => {
        setUsers(prevUsers =>
            prevUsers.map(u => u.ConnectionId === user.ConnectionId ? { ...u, Message: msg } : u)
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
                </div>
            )}
        </div>
    );
};

export default ChatApp;
