import React, { useEffect, useState } from 'react';
import { GiftedChat, IMessage } from 'react-native-gifted-chat';
import { database } from '../../components/firebaseConfig';
import { ref, onValue, push } from 'firebase/database';
import AsyncStorage from '@react-native-async-storage/async-storage';
import NetInfo from '@react-native-community/netinfo';
import * as Device from 'expo-device';

const ChatScreen: React.FC = () => {
  const [messages, setMessages] = useState<IMessage[]>([]);
  const [pendingMessages, setPendingMessages] = useState<IMessage[]>([]);
  const [isConnected, setIsConnected] = useState<boolean>(true);

  // Load messages from Firebase
  const loadMessages = () => {
    const messagesRef = ref(database, Device.modelName);

    onValue(messagesRef, snapshot => {
      const data = snapshot.val();
      if (data) {
        const parsedMessages = Object.keys(data).map(key => ({
          _id: key,
          text: data[key].text,
          createdAt: data[key]?.createdAt,
          user: data[key].user,
        }));

        // Update messages state and save to AsyncStorage
        setMessages(parsedMessages.reverse());
        saveDBMessages(parsedMessages);
      }
    });
  };

  // Load offline messages from AsyncStorage
  const loadOfflineMessages = async () => {
    try {
      const storedMessages = await AsyncStorage.getItem('offlineMessages');
      if (storedMessages) {
        const messagesFromStorage: IMessage[] = JSON.parse(storedMessages);
        setMessages(prevMessages => [...prevMessages, ...messagesFromStorage]);
      }
    } catch (error) {
      console.error('Failed to load offline messages:', error);
    }
  };

  // Save offline messages to AsyncStorage
  const saveOfflineMessages = async (messagesList: IMessage[]) => {
    try {
      await AsyncStorage.setItem('offlineMessages', JSON.stringify(messagesList.reverse()));
    } catch (error) {
      console.error('Failed to save offline messages:', error);
    }
  };

  // Save DB messages to AsyncStorage
  const saveDBMessages = async (messagesList: IMessage[]) => {
    try {
      await AsyncStorage.setItem('firebaseMessages', JSON.stringify(messagesList));
    } catch (error) {
      console.error('Failed to save DB messages:', error);
    }
  };

  // Sync offline messages to Firebase when connected
  const syncOfflineMessagesToFirebase = async () => {
    const offlineMessages = await AsyncStorage.getItem('offlineMessages');
    const messagesFromStorage: IMessage[] = JSON.parse(offlineMessages || '[]');

    if (messagesFromStorage.length) {
      const messageRef = ref(database, Device.modelName);

      // Set to keep track of sent message IDs
      const sentMessageIds = new Set<string>();

      for (const message of messagesFromStorage) {
        // if (!sentMessageIds.has(message._id)) {
          await push(messageRef, {
            _id: message._id,
            text: message.text,
            createdAt: `${message.createdAt}`,
            user: message.user,
          });
          // sentMessageIds.add(message._id); // Mark this message as sent
        // }
      }
      await removeValue(); // Clear offline messages after syncing
    }
  };

  const removeValue = async () => {
    try {
      await AsyncStorage.removeItem('offlineMessages');
    } catch (error) {
      console.error('Failed to remove offline messages:', error);
    }
  };

  useEffect(() => {
    loadMessages();
    loadOfflineMessages();

    const unsubscribe = NetInfo.addEventListener(state => {
      setIsConnected(state.isConnected);
    });

    return () => unsubscribe();
  }, []);

  useEffect(() => {
    if (isConnected) {
      syncOfflineMessagesToFirebase();
    }
  }, [isConnected])

  const handleSend = async (newMessages: IMessage[]) => {
    const message = newMessages[0];
    
    // Update local state with the new message
    const updatedMessages = GiftedChat.append(messages, newMessages);
    setMessages(updatedMessages);

    if (isConnected) {
      const messageRef = ref(database, Device.modelName);
      await push(messageRef, {
        _id: message._id,
        text: message.text,
        createdAt: `${message.createdAt}`,
        user: message.user,
      });
    } else {
      const updatedPendingMessages = GiftedChat.append(pendingMessages, newMessages);
      setPendingMessages(updatedPendingMessages);
      await saveOfflineMessages(updatedPendingMessages);
    }
  };

  return (
    <GiftedChat
      messages={messages}
      onSend={handleSend}
      user={{
        _id: 1, // Replace with dynamic user ID
      }}
      inverted={true} // Ensures the latest messages are at the bottom
    />
  );
};

export default ChatScreen;
