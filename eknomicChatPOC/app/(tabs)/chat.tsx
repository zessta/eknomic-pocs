// ChatScreen.tsx
import React, { useEffect, useState } from 'react';
import { GiftedChat, IMessage } from 'react-native-gifted-chat';
import { database } from '../../components/firebaseConfig';
import { ref, onValue, push } from 'firebase/database';
import AsyncStorage from '@react-native-async-storage/async-storage';
import NetInfo from '@react-native-community/netinfo';

const ChatScreen: React.FC = () => {
  const [messages, setMessages] = useState<IMessage[]>([]);
  const [pendingMessages, setPendingMessages] = useState<IMessage[]>([]);
  const [isConnected, setIsConnected] = useState<boolean>(true);

  // Load messages from Firebase
  const loadMessages = () => {
    const messagesRef = ref(database, 'messages');

    onValue(messagesRef, snapshot => {
      const data = snapshot.val();
      if (data) {
        const parsedMessages = Object.keys(data).map(key => ({
          _id: key,
          text: data[key].text,
          createdAt: data[key]?.createdAt,
          user: data[key].user,
        }));
        setMessages(parsedMessages);
      }
    });
  };

  // Load offline messages from AsyncStorage
  const loadOfflineMessages = async () => {
    try {
      const storedMessages = await AsyncStorage.getItem('offlineMessages');
      if (storedMessages) {
        const messagesFromStorage: IMessage[] = JSON.parse(storedMessages);
        // Only show messages that are not already in Firebase
        const newMessages = messagesFromStorage.filter(
          offlineMessage => !messages.some(msg => msg._id === offlineMessage._id)
        );
        setMessages(prevMessages => [...prevMessages, ...newMessages]);
      }
    } catch (error) {
      console.error('Failed to load offline messages:', error);
    }
  };

  // Save offline messages to AsyncStorage
  const saveOfflineMessages = async (testmessages: IMessage[]) => {
    console.log('testmessages', testmessages)
    try {
      await AsyncStorage.setItem('offlineMessages', JSON.stringify(testmessages));
    } catch (error) {
      console.error('Failed to save offline messages:', error);
    }
  };

  // Sync offline messages to Firebase when connected
  const syncOfflineMessagesToFirebase = async () => {
    const offlineMessages = await AsyncStorage.getItem('offlineMessages');
    const messagesFromStorage: IMessage[] = JSON.parse(offlineMessages);
    console.log('messagesFromStorage', messagesFromStorage)
    const messagesRef = ref(database, 'messages');
    console.log('messagesRef', messagesRef)
    let parsedMessages;
    let newMessages;
     onValue(messagesRef, snapshot => {
      const data = snapshot.val();
      if (data) {
        console.log('data', data)
         parsedMessages = Object.keys(data).map(key => ({
          _id: data[key]._id,
          text: data[key].text,
          createdAt: data[key]?.createdAt,
          user: data[key].user,
        }));
        console.log('parsedMessages', parsedMessages)
        newMessages = messagesFromStorage?.filter(
          offlineMessage => !parsedMessages?.some(msg => msg._id === offlineMessage._id)
        );
        console.log('inside if', newMessages)
      } else {
        console.log('coming here')
        newMessages = messagesFromStorage
      }
    });
    console.log('newMessages', newMessages)
   
    if (newMessages?.length) {
      // const messagesArray = JSON.parse(offlineMessages);
      const messageRef = ref(database, 'messages');

      for (const message of newMessages) {
        await push(messageRef, {
          _id: message._id,
          text: message.text,
          createdAt: message?.createdAt,
          user: message.user,
        });
      }
  }
      // Clear offline messages after syncing
      // await AsyncStorage.removeItem('offlineMessages');
    
  };

  useEffect(() => {
    console.log('test')
    loadMessages();
    loadOfflineMessages();

    const unsubscribe = NetInfo.addEventListener(state => {
      setIsConnected(state.isConnected);

      // Sync offline messages to Firebase when connected
      if (state.isConnected) {
        syncOfflineMessagesToFirebase();
      }
    });

    return () => unsubscribe();
  }, []);

  const handleSend = async (newMessages: IMessage[]) => {
    const message = newMessages[0];

    // Update local state immediately
    const updatedPendingMessages = GiftedChat.append(messages, newMessages);
    // setMessages(previousMessages => GiftedChat.append(previousMessages, newMessages));
    console.log("updatedPendingMessages", updatedPendingMessages)
    setMessages(updatedPendingMessages);
    await saveOfflineMessages(updatedPendingMessages);
    // if (isConnected) {
      // Store the new message in Firebase
      const messageRef = ref(database, 'messages');
      await push(messageRef, {
        _id: message._id,
        text: message.text,
        createdAt: message?.createdAt,
        user: message.user,
      });
    // } else {
    //   // Save the message temporarily for offline use
    //   const updatedPendingMessages = GiftedChat.append(pendingMessages, newMessages);
    //   setPendingMessages(updatedPendingMessages);
    //   await saveOfflineMessages(updatedPendingMessages);
    // }
  }
  return (
    <GiftedChat
      messages={messages}
      onSend={handleSend}
      user={{
        _id: 1, // Replace with dynamic user ID
      }}
    />
  );
};

export default ChatScreen;
