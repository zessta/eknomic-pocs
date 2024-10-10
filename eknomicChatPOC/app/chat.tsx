import React, { useEffect, useRef, useState } from 'react';
import { Bubble, Composer, GiftedChat, IMessage, InputToolbar, Send } from 'react-native-gifted-chat';
import { database } from '../components/firebaseConfig';
import { ref, onValue, push } from 'firebase/database';
import AsyncStorage from '@react-native-async-storage/async-storage';
import NetInfo from '@react-native-community/netinfo';
import { Button, View, StyleSheet, TouchableOpacity, PanResponder, Animated, TouchableWithoutFeedback, Keyboard, Platform } from 'react-native';
import * as ImagePicker from 'expo-image-picker';
import { useLocalSearchParams } from 'expo-router';
import SummaryWidget from '../components/SummaryWidget';
import { FontAwesome, Ionicons } from '@expo/vector-icons';
import ReplyMessageBar from '../components/ReplyMessageBar';
import * as Clipboard from 'expo-clipboard';
import { TextInput } from 'react-native-paper';
// import { KeyboardAvoidingView } from 'native-base';
const customtInputToolbar = (props: any) => {
  return (
    <InputToolbar
      {...props}
      containerStyle={{
        backgroundColor: "#232D36",
        borderTopWidth: 0,
        padding: 2,
        marginBottom: 8,
        marginLeft: 4,
        marginRight: 4,
        marginTop: 4,
        borderRadius: 20,
        flexDirection: "column-reverse",
        position: "relative",
      }}
      renderSend={(props) => (
        <Send {...props} containerStyle={{alignContent:"center", alignItems:"center", justifyContent:"center"}}>
          <View style={{
            justifyContent: "center",
            alignItems: "center",
            padding: 10
          }}>
            <Ionicons name="send" size={24}/>
          </View>
        </Send>
      )}
      renderComposer={(props) => ( <Composer {...props} textInputStyle={{ color: "white", marginTop: 4}}/> )}
      accessoryStyle={{height: "auto"}}
    />
  );
};

const ChatScreen = () => {
  const defaultUserId = 3; // Replace with your default user ID
  const { userId } = useLocalSearchParams();
  const [messages, setMessages] = useState<IMessage[]>([]);
  const [pendingMessages, setPendingMessages] = useState<IMessage[]>([]);
  const [isConnected, setIsConnected] = useState<boolean>(true);
  const [showSummary, setShowSummary] = useState<boolean>(false);
  const [iconPosition] = useState(new Animated.ValueXY({ x: 100, y: 100 })); // Initial position
  const [replyMessage, setReplyMessage] = useState<IMessage | null>(null);

  const clearReplyMessage = () => setReplyMessage(null);

  const loadMessages = () => {
    const messagesRef = ref(database, 'chats');
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

  const saveDBMessages = async (messagesList: IMessage[]) => {
    try {
      await AsyncStorage.setItem('firebaseMessages', JSON.stringify(messagesList));
    } catch (error) {
      console.error('Failed to save DB messages:', error);
    }
  };

  const saveOfflineMessages = async (messagesList: IMessage[]) => {
    try {
      await AsyncStorage.setItem('offlineMessages', JSON.stringify(messagesList.reverse()));
    } catch (error) {
      console.error('Failed to save offline messages:', error);
    }
  };

  const syncOfflineMessagesToFirebase = async () => {
    const offlineMessages = await AsyncStorage.getItem('offlineMessages');
    const messagesFromStorage: IMessage[] = JSON.parse(offlineMessages || '[]');

    if (messagesFromStorage.length) {
      const messageRef = ref(database, `chats/${defaultUserId}`);

      for (const message of messagesFromStorage) {
        await push(messageRef, {
          _id: message._id,
          text: message.text,
          createdAt: message.createdAt,
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
        createdAt: message.createdAt,
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

  // const giveFeedback = (messageId: string, feedbackType: string) => {
  //   const feedbackRef = ref(database, `messages/${messageId}/feedback`);
  //   feedbackRef.transaction((currentFeedback) => {
  //     if (!currentFeedback) {
  //       return { like: 0, dislike: 0 };
  //     }
  //     if (feedbackType === 'like') {
  //       currentFeedback.like = (currentFeedback.like || 0) + 1;
  //     } else {
  //       currentFeedback.dislike = (currentFeedback.dislike || 0) + 1;
  //     }
  //     return currentFeedback;
  //   });
  // };

  // const renderBubble = (props: any) => {
  //   return (
  //     <Bubble
  //       {...props}
  //       renderFooter={() => (
  //         <View style={{ flexDirection: 'row', justifyContent: 'space-between' }}>
  //           <Button title="👍" onPress={() => giveFeedback(props.currentMessage._id, 'like')} />
  //           <Button title="👎" onPress={() => giveFeedback(props.currentMessage._id, 'dislike')} />
  //         </View>
  //       )}
  //     />
  //   );
  // };

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

  const onLongPress = (context: any, message: IMessage) => {
    context.actionSheet().showActionSheetWithOptions(
      {
        options: ["reply", "copy", "cancel"],
      },
      (buttonIndex) => {
        switch (buttonIndex) {
          case 0:
            setReplyMessage(message);
            break;
          case 1:
            Clipboard.setStringAsync(message.text);
            break;
        }
      }
    );
  }

  const customBubble = (props: any) => {
    return (
      <Bubble
        {...props}
        wrapperStyle={{
          right: {
            backgroundColor: '#30b091',
          },
          left: {
            backgroundColor: "#232D36",
          }
        }}
      />
    );
  }

  const renderReplyMessageView = (props: any) => {
    if (replyMessage) {
      return (
        <View 
        style={{padding: 2, margin: 1, paddingBottom: 0, backgroundColor:"rgba(52, 52, 52, 0.5)", borderLeftColor: 'white', borderLeftWidth: 2  }}
        >
          <TextInput 
          style={{fontSize: 10, color:"gray"}}
            // isTruncated 
            // fontSize="xs" 
            // color="gray.300"
          >
            {replyMessage.text}
          </TextInput>
        </View>
      );
    }
    return null;
  }

  const renderAccessory = () => {
    if (replyMessage) { 
      return (
        <ReplyMessageBar message={replyMessage} clearReply={clearReplyMessage} />
      );
    }
    return null;
  }
  return (
    <TouchableWithoutFeedback onPress={() => Keyboard.dismiss()}>

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
          _id: defaultUserId, // Replace with dynamic user ID
          name: 'Shivasai Kumar', // Optional: add user name
          avatar:'https://ui-avatars.com/api/?background=000000&color=FFF&name=SHIVASAIKUMAR'
        }}
        renderInputToolbar={props => customtInputToolbar(props)}
        renderUsernameOnMessage={true}
        inverted={true}
        renderBubble={customBubble}
        onLongPress={onLongPress}
        renderCustomView={renderReplyMessageView}
        renderAccessory={renderAccessory}
        minInputToolbarHeight={60}
        keyboardShouldPersistTaps='never'
      />
                {/* {Platform.OS === 'android' && <KeyboardAvoidingView behavior="padding" keyboardVerticalOffset={90} />} */}

      <Animated.View
        style={[styles.floatingButton, { transform: iconPosition.getTranslateTransform() }]}
        {...panResponder.panHandlers}
      >
        <TouchableOpacity onPress={toggleSummary}>
          <FontAwesome name="info-circle" size={24} color="black" />
        </TouchableOpacity>
      </Animated.View>
    </View>
    </TouchableWithoutFeedback>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    padding: 10,
  },
  floatingButton: {
    position: 'absolute',
    backgroundColor: 'white',
    borderRadius: 28,
    padding: 4,
    elevation: 5,
  },
});

export default ChatScreen;



// {
//  messages : [{}]
//  senderId : 3
//  recieverId : 1 
// }

// {messages : ['hi', 'hello', 
//   'can you help with me python code', 'what code you need?', 'i need  function based code', 
//   'how much should be the complexity', 'for beginners', 'okay i will in 10minutes', 'thank you']}
