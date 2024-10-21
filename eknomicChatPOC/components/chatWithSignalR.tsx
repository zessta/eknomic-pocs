import React, { useEffect, useState } from 'react';
import { View, Text, TextInput, Button, FlatList, TouchableOpacity, StyleSheet } from 'react-native';
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
            .withUrl('http://localhost:5000/signalrtc') // Change to your backend URL
            .withAutomaticReconnect()
            .build();

        try {
            await newConnection.start();
            console.log('Connected to SignalR server');
            console.log('connectionId', newConnection.connectionId);

            await newConnection.invoke('NewUser', username);

            const userListJson = await newConnection.invoke('GetUserList');
            const updatedUsers = JSON.parse(userListJson) as UserInfo[];
            setUsers(updatedUsers.map(user => ({ ...user, Messages: [] })));

            setConnection(newConnection);
            setIsJoined(true);

        } catch (err) {
            console.log('Error connecting to SignalR: ', err);
        }
    };

    useEffect(() => {
        if (connection) {
            connection.on('ReceiveMessage', (user: string, message: string, receiver: string) => {
                setUsers(prevUsers =>
                    prevUsers.map(u =>
                        (u.UserName === user || u.UserName === receiver)
                            ? { ...u, Messages: [...u.Messages, { user, message, receiver }] }
                            : u
                    )
                );
            });

            connection.on('UpdateUserList', (userListJson: string) => {
                const updatedUsers = JSON.parse(userListJson) as UserInfo[];
                setUsers(updatedUsers.map(user => ({ ...user, Messages: [] })));
            });

            return () => {
                connection.off('ReceiveMessage');
                connection.off('UpdateUserList');
            };
        }
    }, [connection]);

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
                                Message: ''
                            }
                            : u
                    )
                );
            } catch (err) {
                console.error('Error sending message: ', err);
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

    const handleUserClick = (user: UserInfo) => {
        loadChatHistory(username, user.UserName);
    };

    const updateMessageInput = (msg: string, user: UserInfo) => {
        setUsers(prevUsers =>
            prevUsers.map(u => u.ConnectionId === user.ConnectionId ? { ...u, Message: msg } : u)
        );
    };

    return (
        <View style={styles.container}>
            <Text style={styles.title}>SignalR Chat</Text>

            {!isJoined ? (
                <View>
                    <TextInput
                        style={styles.input}
                        placeholder="Enter username"
                        value={username}
                        onChangeText={setUsername}
                    />
                    <Button title="Join Chat" onPress={joinChat} disabled={!username} />
                </View>
            ) : (
                <View>
                    <Text>Welcome, {username}!</Text>

                    <TextInput
                        style={styles.input}
                        placeholder="Type a message"
                        value={message}
                        onChangeText={setMessage}
                    />
                    <Button title="Send to All" onPress={sendMessageToAll} disabled={!message} />

                    <Text style={styles.userListTitle}>Users</Text>
                    <FlatList
                        data={users}
                        keyExtractor={(item) => item.ConnectionId}
                        renderItem={({ item }) => (
                            <TouchableOpacity onPress={() => handleUserClick(item)} style={styles.userItem}>
                                <Text>{item.UserName} (ID: {item.ConnectionId})</Text>
                                <FlatList
                                    data={item.Messages}
                                    keyExtractor={(msg, idx) => `${msg.user}-${idx}`}
                                    renderItem={({ item: msg }) => (
                                        <Text style={styles.message}>
                                            <Text style={styles.messageUser}>{msg.user}:</Text> {msg.message}
                                        </Text>
                                    )}
                                />
                                <TextInput
                                    style={styles.input}
                                    placeholder="Type a message"
                                    value={item.Message}
                                    onChangeText={(text) => updateMessageInput(text, item)}
                                />
                                <Button title="Send Private Message" onPress={() => sendMessageToUser(item.ConnectionId)} />
                            </TouchableOpacity>
                        )}
                    />
                </View>
            )}
        </View>
    );
};

const styles = StyleSheet.create({
    container: {
        flex: 1,
        padding: 20,
        justifyContent: 'center',
    },
    title: {
        fontSize: 24,
        fontWeight: 'bold',
        marginBottom: 20,
    },
    input: {
        borderWidth: 1,
        borderColor: 'gray',
        padding: 10,
        marginBottom: 10,
        borderRadius: 5,
    },
    userListTitle: {
        fontSize: 18,
        marginTop: 20,
    },
    userItem: {
        borderWidth: 1,
        borderColor: 'lightblue',
        marginVertical: 5,
        padding: 10,
        borderRadius: 5,
    },
    message: {
        marginVertical: 5,
    },
    messageUser: {
        fontWeight: 'bold',
    },
});

export default ChatApp;
