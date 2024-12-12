import React, { useEffect, useState } from 'react';
import { GiftedChat, IMessage } from 'react-native-gifted-chat';
import * as Network from 'expo-network';
import * as FileSystem from 'expo-file-system';
import { initializeBackgroundFetch } from '../../components/BackgroundOfflineTask'; // Import background task

const ChatScreen: React.FC = () => {
  const [messages, setMessages] = useState<IMessage[]>([]);
  const [isOnline, setIsOnline] = useState<boolean>(true);

  useEffect(() => {
    // Monitor network connectivity
    const checkNetworkStatus = async () => {
      const status = await Network.getNetworkStateAsync();
      setIsOnline(status.isConnected!);
    };

    const interval = setInterval(checkNetworkStatus, 1000); // Check every 1 second
    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    initializeBackgroundFetch(); // Register background fetch task
  }, []);

  // Store offline messages locally
  const storeMessageLocally = async (newMessage: IMessage) => {
    const offlineMessages = await FileSystem.readAsStringAsync(FileSystem.documentDirectory + 'offlineMessages.json').catch(() => '[]');
    const messagesArray: IMessage[] = JSON.parse(offlineMessages);
    messagesArray.push(newMessage);
    await FileSystem.writeAsStringAsync(FileSystem.documentDirectory + 'offlineMessages.json', JSON.stringify(messagesArray));
  };

  const sendMessage = (newMessage: IMessage) => {
    setMessages((prevMessages) => GiftedChat.append(prevMessages, newMessage));
  };

  const handleSend = async (newMessages: IMessage[]) => {
    const newMessage = newMessages[0];
    if (isOnline) {
      sendMessage(newMessage);
    } else {
      await storeMessageLocally(newMessage);
    }
  };

  return (
    <GiftedChat
      messages={messages}
      onSend={(newMessages) => handleSend(newMessages)}
      user={{
        _id: 1,
        name: 'User',
      }}
    />
  );
};

export default ChatScreen;
