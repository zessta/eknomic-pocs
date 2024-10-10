import React, { useEffect, useRef, useState } from 'react';
import { Bubble, GiftedChat, IMessage } from 'react-native-gifted-chat';
import { database } from '../components/firebaseConfig';
import { ref, onValue, push } from 'firebase/database';
import AsyncStorage from '@react-native-async-storage/async-storage';
import NetInfo from '@react-native-community/netinfo';
import { Button, View, StyleSheet, TouchableOpacity, PanResponder, Animated } from 'react-native';
import * as ImagePicker from 'expo-image-picker';
import { useLocalSearchParams } from 'expo-router';
import SummaryWidget from '../components/SummaryWidget'; // Import the summary widget
import { FontAwesome, MaterialIcons } from '@expo/vector-icons'; // Optional: for using icons
const ChatScreen = () => {
  const defaultUserId: number = 3; // Replace with your default user ID
  const { userId } = useLocalSearchParams();
  const [messages, setMessages] = useState<IMessage[]>([]);
  const [pendingMessages, setPendingMessages] = useState<IMessage[]>([]);
  const [isConnected, setIsConnected] = useState<boolean>(true);
  const [showSummary, setShowSummary] = useState<boolean>(false);
  const [iconPosition, setIconPosition] = useState(new Animated.ValueXY({ x: 100, y: 100 })); // Initial position

  // Load messages from Firebase
  const loadMessages = () => {
    const messagesRef = ref(database, `chats`);
    onValue(messagesRef, snapshot => {
      const data = snapshot.val();
      if (data) {
        const parsedMessages: IMessage[] = [];
        Object.keys(data).forEach(chatId => {
          const chatMessages = data[chatId];
          if (chatMessages) {
            Object.keys(chatMessages).forEach(messageId => {
              const msg = chatMessages[messageId];
              if (msg.user._id === Number(userId) || msg.user._id === defaultUserId) {
                console.log('msg.image', msg.image)
                parsedMessages.push({
                  _id: messageId,
                  text: msg.text || '',
                  createdAt: msg.createdAt,
                  user: msg.user,
                  image: msg.image || null,
                });
              }
            });
          }
        });

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
      const messageRef = ref(database, `chats`);

      for (const message of messagesFromStorage) {
        await push(messageRef, {
          _id: message._id,
          text: message.text,
          createdAt: `${message.createdAt}`,
          user: message.user,
        });
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
  }, [userId]);

  useEffect(() => {
    if (isConnected) {
      syncOfflineMessagesToFirebase();
    }
  }, [isConnected]);

  const handleSend = async (newMessages: IMessage[]) => {
    const message = newMessages[0];
    const updatedMessages = GiftedChat.append(messages, newMessages);
    setMessages(updatedMessages);

    if (isConnected) {
      const messageRef = ref(database, `chats/${defaultUserId}`);
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

  const handleImagePicker = async () => {
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      allowsEditing: true,
      aspect: [4, 3],
      quality: 1,
    });

    if (!result.canceled) {
      const message: IMessage = {
        _id: Math.random().toString(),
        text: '',
        createdAt: new Date(),
        user: { _id: Number(userId), name: 'User' }, // Replace with actual user info
        image: result.assets[0].uri, // Get image URI
      };
      await handleSend([message]);

      // Save image URL to Firebase
      const messagesRef = ref(database, `chats/${defaultUserId}`);
      await push(messagesRef, {
        image: result.assets[0].uri,
        createdAt: new Date().toISOString(),
        user: { _id: Number(userId), name: 'User' }, // Replace with actual user info
      });
    }
  };

  const giveFeedback = (messageId: any, feedbackType: any) => {
    const feedbackRef = ref(database, `messages/${messageId}/feedback`);
    feedbackRef.transaction((currentFeedback) => {
      if (!currentFeedback) {
        return { like: 0, dislike: 0 };
      }
      if (feedbackType === 'like') {
        currentFeedback.like = (currentFeedback.like || 0) + 1;
      } else {
        currentFeedback.dislike = (currentFeedback.dislike || 0) + 1;
      }
      return currentFeedback;
    });
  };

  const renderBubble = (props: any) => {
    return (
      <Bubble
        {...props}
        renderFooter={() => (
          <View style={{ flexDirection: 'row', justifyContent: 'space-between' }}>
            <Button title="👍" onPress={() => giveFeedback(props.currentMessage._id, 'like')} />
            <Button title="👎" onPress={() => giveFeedback(props.currentMessage._id, 'dislike')} />
          </View>
        )}
      />
    );
  };

  const toggleSummary = () => {
    setShowSummary(!showSummary);
  };

  const handleYes = () => {
    console.log("User selected Yes");
    toggleSummary(); // Close the summary widget
  };

  const handleNo = () => {
    console.log("User selected No");
    toggleSummary(); // Close the summary widget
  };

  // PanResponder for dragging the icon
  const panResponder = useRef(
    PanResponder.create({
      onStartShouldSetPanResponder: () => true,
      onMoveShouldSetPanResponder: () => true,
      onPanResponderGrant: () => {
        iconPosition.setOffset({
          x: iconPosition.x._value,
          y: iconPosition.y._value,
        });
        iconPosition.setValue({ x: 0, y: 0 });
      },
      onPanResponderMove: Animated.event(
        [
          null,
          {
            dx: iconPosition.x,
            dy: iconPosition.y,
          },
        ],
        { useNativeDriver: false }
      ),
      onPanResponderRelease: () => {
        iconPosition.flattenOffset();
      },
    })
  ).current;

  return (
    <View style={styles.container}>
      {showSummary && (
        <SummaryWidget
          messages={messages}
          onYes={handleYes}
          onNo={handleNo}
        />
      )}
      <GiftedChat
        messages={messages}
        onSend={handleSend}
        user={{
          _id: 3, // Replace with dynamic user ID
        }}
        renderUsernameOnMessage={true}
        inverted={true} // Ensures the latest messages are at the bottom
        renderAccessory={() => (
          <Button title="Upload" onPress={handleImagePicker} />
        )}
        renderBubble={renderBubble}
      />
      <Animated.View
        style={[styles.floatingButton, { transform: iconPosition.getTranslateTransform() }]}
        {...panResponder.panHandlers}
      >
        <TouchableOpacity onPress={toggleSummary}>
          <FontAwesome name="info-circle" size={24} color="black" />
          {/* <MaterialIcons name="summarize" size={24} color="black" /> */}
        </TouchableOpacity>
      </Animated.View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    padding: 10,
  },
  floatingButton: {
    position: 'absolute',
    backgroundColor: 'white', // Customize the button color
    borderRadius: 28,
    padding: 4,
    elevation: 5, // Optional: shadow for Android
  },
});

export default ChatScreen;
