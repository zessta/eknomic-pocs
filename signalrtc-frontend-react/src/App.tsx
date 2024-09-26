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
}

const ChatApp: React.FC = () => {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [messages, setMessages] = useState<ChatMessage[]>([]);
    const [message, setMessage] = useState<string>('');
    const [username, setUsername] = useState<string>('');
    const [users, setUsers] = useState<UserInfo[]>([]); // Manage the list of users
    const [isJoined, setIsJoined] = useState<boolean>(false);
    const [selectedUser, setSelectedUser] = useState<UserInfo | null>(null); // Track the selected user for private messages

    const joinChat = async () => {
        if (!username) return;

        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl('http://localhost:2511/signalrtc') // Change to your backend URL
            .withAutomaticReconnect()
            .build();

        try {
            await newConnection.start();
            console.log('Connected to SignalR server');
            console.log('connectionId',newConnection.connectionId);

            // Invoke NewUser method on the server
            await newConnection.invoke('NewUser', username);


            // when user joins, invoke a method to get the current user list
            const userListJson = await newConnection.invoke('GetUserList');
            const updatedUsers = JSON.parse(userListJson) as UserInfo[];
            setUsers(updatedUsers); // Immediately set the user list in state


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
                setMessages(prevMessages => [...prevMessages, { user, message, receiver }]);
            });

            // Listen for the updated user list and update users
            connection.on('UpdateUserList', (userListJson: string) => {
                const updatedUsers = JSON.parse(userListJson) as UserInfo[];
                setUsers(updatedUsers); // Update the user list in the frontend
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
                setMessages(prevMessages => [...prevMessages, { user: 'Me', message,receiver:'' }]);
                setMessage('');
            } catch (err) {
                console.error('Error sending message: ', err);
            }
        }
    };

    // Send message to a specific user
    const sendMessageToUser = async (targetConnectionId: string) => {
        var message = users.find(t => t.ConnectionId === targetConnectionId)?.Message;
        var sentUser = users.find(t => t.ConnectionId === targetConnectionId);
        if (connection && message && username) {
            try {
                await connection.invoke('SendMessageToUser', message, targetConnectionId);
                setMessages(prevMessages => [...prevMessages, { user: 'Me', message, receiver: '' }]);
                setMessage(''); // Clear input after sending
                if (sentUser)
                    updateMessages('', sentUser)// Clear input after sending
            } catch (err) {
                console.error('Error sending message: ', err);
            }
        }
    };

    // Call GetChatHistory when a chat with a specific user is opened
    const loadChatHistory = async (fromUser: any, toUser: any) => {
        if (connection) {
            const response = await connection.invoke("GetChatHistory", fromUser, toUser);
            var messages = response.map((t: any) => ({ user: t.fromUser, message:t.message, receiver: toUser }));
            console.log('response:',response);
            console.log('messages:',messages);
            setMessages(messages);
        }
    };

    const handleUserClick = (user: UserInfo) => {
        setSelectedUser(user); // Set the selected user
        loadChatHistory(username, user.UserName); // Load chat history with the selected user
    };

    function updateMessages(msg: string, user: UserInfo) {

        var newUser = users.find(t => t.ConnectionId === user.ConnectionId);
        if (newUser)
        newUser.Message = msg;

        setUsers(users);
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

                    {/* Message Input */}
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
                                <li key={index} onClick={() => handleUserClick(user)}>
                                    {user.UserName} (ID: {user.ConnectionId}){' '}
                                    <div>
                                        <input key={user.ConnectionId} type="text"
                                        placeholder="Type a message"
                                        value={user.Message}
                                        onChange={(e) => updateMessages(e.target.value, user)}></input></div>
                                    <button onClick={() => sendMessageToUser(user.ConnectionId)}>
                                        Send Private Message
                                    </button>
                                    <pre>

                                    </pre>
                                </li>
                            ))}
                        </ul>
                    </div>

                    {/* Message List */}
                    <div>
                        <h3>Messages</h3>
                        <ul>
                            {messages.map((msg, index) => (
                                <li key={index}>
                                    <strong>{msg.user}:</strong> {msg.message}
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
