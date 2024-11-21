import React, { useEffect, useRef, useState } from 'react';
import {
  Animated,
  AppState,
  Image,
  Keyboard,
  Modal,
  PanResponder,
  StyleSheet,
  Text,
  TouchableOpacity,
  TouchableWithoutFeedback,
  View,
} from 'react-native';
import {
  Bubble,
  Composer,
  GiftedChat,
  IMessage,
  InputToolbar,
  Message,
  Send,
} from 'react-native-gifted-chat';
// import database from '@react-native-firebase/database';
import storage from '@react-native-firebase/storage';
import AsyncStorage from '@react-native-async-storage/async-storage';
import NetInfo from '@react-native-community/netinfo';
import * as ImagePicker from 'expo-image-picker';
import { useLocalSearchParams, useNavigation } from 'expo-router';
import SummaryWidget from '../components/SummaryWidget';
import { FontAwesome, Ionicons } from '@expo/vector-icons';
import ReplyMessageBar from '../components/ReplyMessageBar';
import * as Clipboard from 'expo-clipboard';
import { TextInput } from 'react-native-paper';
import * as Notifications from 'expo-notifications';
import * as BackgroundFetch from 'expo-background-fetch';
import * as TaskManager from 'expo-task-manager';
import { database } from '../components/firebaseConfig'; // Adjust the path accordingly
import { push, ref, set } from '@react-native-firebase/database';

const customInputToolbar = (props: any, handleImagePicker: () => void)=> {
  return (
    <InputToolbar
      {...props}
      containerStyle={styles.inputToolbar}
      renderSend={(props) => (
        <Send {...props} containerStyle={styles.sendContainer}>
              <View style={styles.sendButtonContainer}>

            {/* Image upload icon always visible */}
            <TouchableOpacity onPress={handleImagePicker} style={{ marginRight: 10 }}>
              <Ionicons name="image" size={24} color="white" />
            </TouchableOpacity>

            <View style={{ justifyContent: "center", alignItems: "center" }}>
              <Ionicons name="send" size={24} />
            </View>
          </View>
        </Send>
      )}
      renderComposer={(props) => (
        <Composer {...props} textInputStyle={{ color: "white", marginTop: 4 }} />
      )}
      accessoryStyle={{ height: "auto" }}
    />
  );
};

TaskManager.defineTask('IMAGE_UPLOAD_TASK', async ({ data, error }: any) => {
  if (error) {
    console.error('Error in upload task', error);
    return;
  }

  const { images }: { images: string[] } = data; // Expecting an array of image URIs
  for (const uri of images) {
    try {
      await uploadImageToFirebase(uri);
      console.log(`Uploaded: ${uri}`);
    } catch (uploadError) {
      console.error(`Failed to upload ${uri}`, uploadError);
    }
  }
});

const uploadImageToFirebase = async (uri: string): Promise<string> => {
  const response = await fetch(uri);
  const blob = await response.blob();
  const imageRef = storage().ref(`chat-images/${new Date().getTime()}.jpg`);

  await imageRef.put(blob);
  const downloadURL = await imageRef.getDownloadURL();
  return downloadURL;
};

const ChatScreen: React.FC = () => {
  const navigation = useNavigation();
  const { receiverUserId, receiverUserName, senderUserId, senderUserName } = useLocalSearchParams() as any;
  const [messages, setMessages] = useState<IMessage[]>([]);
  const [pendingMessages, setPendingMessages] = useState<IMessage[]>([]);
  const [isConnected, setIsConnected] = useState(true);
  const [showSummary, setShowSummary] = useState(false);
  const [iconPosition] = useState(new Animated.ValueXY({ x: 100, y: 100 }));
  const [replyMessage, setReplyMessage] = useState<IMessage | null>(null);
  const [isModalVisible, setModalVisible] = useState(false);
  const [selectedImage, setSelectedImage] = useState<string | null>(null);

  const handleImagePress = (uri: string) => {
    setSelectedImage(uri);
    setModalVisible(true);
  };

  const clearReplyMessage = () => setReplyMessage(null);

  useEffect(() => {
    navigation.setOptions({ title: receiverUserName });
    loadMessages();
    loadOfflineMessages();

    const unsubscribe = NetInfo.addEventListener(state => {
      setIsConnected(state.isConnected);
    });

    return () => unsubscribe();
  }, [receiverUserId]);

  useEffect(() => {
    if (isConnected) {
      syncOfflineMessagesToFirebase();
    }
  }, [isConnected]);

  // useEffect(() => {
  //   // Mark the last message as "seen" when the chat is loaded
  //   const lastMessage = messages[messages.length - 1];
  //   if (lastMessage && !lastMessage.sent) {
  //     console.log('coming inside useeffecr')
  //     updateSeenStatus(lastMessage._id as string);
  //   }
  // }, [messages]); // Run this when messages change

  const updateSeenStatus = async (messageId: string) => {
    const messageRef = ref(database, `chats/${senderUserId}_${receiverUserId}`);
    const snapshot = await messageRef.once('value');
    const messages = snapshot.val();
  
    for (let messageKey in messages) {
      const message = messages[messageKey];
      if (message._id === messageId) {
        break; // Once we reach the messageId, we stop updating previous messages
      }
      // Update the "seen" status
      await set(ref(database, `chats/${senderUserId}_${receiverUserId}/${messageKey}/sent`), true);
    }
  };

  const onNewMessageReceived = async (newMessage: IMessage) => {
    // Only update the received status if it's a new message and it's not already marked as received
    if (newMessage && !newMessage.received) {
      await set(ref(database, `chats/${receiverUserId}_${senderUserId}/${newMessage._id}/received`), true);
      console.log('Message received status updated in Firebase:', newMessage._id);
    }
  };

  const updateReceivedStatus = async (messageId: string) => {
    const messageRef = ref(database, `chats/${senderUserId}_${receiverUserId}`);
    const snapshot = await messageRef.once('value');
    const messages = snapshot.val();
  
    for (let messageKey in messages) {
      const message = messages[messageKey];
      if (message._id === messageId) {
        break; // Once we reach the messageId, we stop updating previous messages
      }
      // Update the "seen" status
      await set(ref(database, `chats/${senderUserId}_${receiverUserId}/${messageKey}/received`), true);
    }
  };

  const loadMessages = () => {
    console.log('coming here on every new message')
    const senderMessagesRef = ref(database, `chats/${senderUserId}_${receiverUserId}`);
    const receiverMessagesRef = ref(database, `chats/${receiverUserId}_${senderUserId}`);

    senderMessagesRef.on('value', snapshot => {
      const senderMessages = snapshot.val() ? Object.values(snapshot.val()) : [];
      receiverMessagesRef.on('value', snapshot => {
        const receiverMessages = snapshot.val() ? Object.values(snapshot.val()) : [];
        const allMessages = [...senderMessages.reverse(), ...receiverMessages.reverse()].map(msg => ({
          ...msg,
          createdAt: new Date(msg.createdAt).getTime(),
        })).sort((a, b) => a.createdAt - b.createdAt);
        const newMessages = allMessages.filter(msg => !messages.some(existingMsg => existingMsg._id === msg._id));
        if (newMessages.length > 0) {
          showNotification(newMessages);
        }

        setMessages(allMessages);
        saveDBMessages(allMessages);
      });
    });
  };


  const showNotification = async (newMessages: IMessage[]) => {
    const notificationMessage = newMessages.length === 1
      ? `New message from ${newMessages[0].user.name}: ${newMessages[0].text}`
      : `You have ${newMessages.length} new messages.`;

    await Notifications.scheduleNotificationAsync({
      content: {
        title: "New Message",
        body: notificationMessage,
      },
      trigger: { seconds: 10 },
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

  const handleSummaryReceived = async (newSummary: string) => {
    const summaryMessage: IMessage = {
      _id: Math.random().toString(),
      text: newSummary,
      createdAt: new Date().getTime(),
      user: { _id: 1, name: 'Summary' },
    };
    const messageRef = ref(database, `chats/${senderUserId}_${receiverUserId}`);
    // await messageRef.push(summaryMessage);
    await push(messageRef, {
      _id: summaryMessage._id,
      text: summaryMessage.text,
      createdAt: summaryMessage.createdAt, // Save as ISO string
      user: summaryMessage.user,
      sent: false, // Initially false
    });

    setMessages(prev => GiftedChat.append(prev, [summaryMessage]));

    const updatedMessagesWithSummary = GiftedChat.append(messages, [summaryMessage]);
    setPendingMessages(updatedMessagesWithSummary);
    console.log('Received Summary:', newSummary);
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
      const messageRef = ref(database, `chats/${senderUserId}_${receiverUserId}`);

      for (const message of messagesFromStorage) {
        // await messageRef.push(message);
        await push(messageRef, {
          _id: message._id,
          text: message.text,
          createdAt: new Date().getTime(),
          user: message.user,
          receiverUserId,
          sent: true, // Initially false
          pending: false,
          received: false
        });
      }
      await AsyncStorage.removeItem('offlineMessages');
    }
  };

  useEffect(() => {
    const subscription = AppState.addEventListener('change', nextAppState => {
      if (nextAppState === 'active') {
        // Cleanup or re-register any tasks if necessary
        BackgroundFetch.registerTaskAsync('IMAGE_UPLOAD_TASK');
      }
    });
  
    return () => {
      subscription.remove();
      BackgroundFetch.unregisterTaskAsync('IMAGE_UPLOAD_TASK');
      TaskManager.unregisterTaskAsync('IMAGE_UPLOAD_TASK');
    };
  }, []);

  useEffect(() => {
    loadMessages();
    loadOfflineMessages();

    const unsubscribe = NetInfo.addEventListener(state => {
      setIsConnected(state.isConnected!);
    });

    return () => unsubscribe();
  }, [receiverUserId]);

  useEffect(() => {
    if (isConnected) {
      syncOfflineMessagesToFirebase();
    }
  }, [isConnected]);

  const handleSend = async (newMessages: IMessage[]) => {
    console.log('newMessages', newMessages)
    // event.persist(); // Persist the event to prevent it from being released

  // Log the event to make sure it's still available after persisting it
    const message = newMessages[0];
    newMessages[0].pending = true;
    newMessages[0].sent = false;
    newMessages[0].received = false;
    const updatedMessages = GiftedChat.append(messages, newMessages);
    setMessages(updatedMessages);

    if (isConnected) {
      const messageRef = ref(database, `chats/${senderUserId}_${receiverUserId}`);
      // await messageRef.push(message);
      await push(messageRef, {
        _id: message._id,
        text: message.text,
        createdAt: new Date().getTime(),
        user: message.user,
        receiverUserId,
        sent: true, // Initially false
        pending: false,
        received: false
      });
    } else {
      const updatedPendingMessages = GiftedChat.append(messages, newMessages);
      setPendingMessages(updatedPendingMessages);
      await AsyncStorage.setItem('offlineMessages', JSON.stringify(updatedPendingMessages));
    }
  };

  const handleImagePicker = async () => {
    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      allowsEditing: true,
      aspect: [4, 3],
      quality: 1,
      allowsMultipleSelection: true,
    });

    if (!result.canceled) {
      const images = result.assets;
      const uploadPromises = images.map(async (image) => {
        const message: IMessage = {
          _id: Math.random().toString(),
          text: '',
          createdAt: new Date().getTime(),
          user: { _id: senderUserId, name: senderUserName },
          image: image.uri,
        };
        const appState = await AppState.currentState;
        if (appState === 'background') {
          // Register the background task for each image
          await BackgroundFetch.registerTaskAsync('IMAGE_UPLOAD_TASK', {
            minimumInterval: 30, // Minimum interval (in seconds) for background task
            stopOnTerminate: false,
          });
          await BackgroundFetch.fetchAsync('IMAGE_UPLOAD_TASK', {
            uri: image.uri,
            userId: senderUserId,
            userName: senderUserName,
            chatId: `${senderUserId}_${receiverUserId}`,
          });
        } else {
        const imageUrl = await uploadImageToFirebase(image.uri);
        message.image = imageUrl;

        const updatedMessages = GiftedChat.append(messages, [message]);
        setMessages(updatedMessages);

        const messagesRef = ref(database, `chats/${senderUserId}_${receiverUserId}`);
        await messagesRef.push({ ...message, image: imageUrl });
        }
      });


      await Promise.all(uploadPromises);
    }
  };

  const handleReply = (message: IMessage) => {
    setReplyMessage(message);
    setModalVisible(true);
  };

  const handleCopyToClipboard = async (text: string) => {
    await Clipboard.setStringAsync(text);
    alert('Copied to clipboard!');
  };

  const handleModalClose = () => {
    setModalVisible(false);
    setReplyMessage(null);
  };


  
  

  const toggleSummary = () => {
    setShowSummary(!showSummary);
  };

  const handleYes = () => {
    console.log("User selected Yes");
    toggleSummary();
  };

  const handleNo = () => {
    console.log("User selected No");
    toggleSummary();
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
  };

  const customBubble = (props: any) => {
    return (
      <Bubble
        {...props}
        wrapperStyle={{
          right: {
            backgroundColor: '#30b091',
          },
          left: {
            backgroundColor: "grey",
          }
        }}
        textStyle={{
          right: {
            color: 'white',
          },
          left: {
            color: 'white',
          },
        }}
        renderMessageImage={(imageProps) => (
          <TouchableWithoutFeedback onPress={() => handleImagePress(imageProps.currentMessage.image!)}>
        <Image
          source={{ uri: imageProps.currentMessage.image }}
          style={{ width: 200, height: 200, borderRadius: 15 }}
        />
      </TouchableWithoutFeedback>
        )}
        renderFooter={(props) => {
          if (props.currentMessage.sent) {
            return <Text style={{ color: 'black', fontSize: 12 }}>Seen</Text>;
          } else {
            return <Text style={{ color: 'black', fontSize: 12 }}>Sent</Text>;
          }
        }}
      />
    );
  };
  
  
  const renderReplyMessageView = (props: any) => {
    if (replyMessage) {
      return (
        <View 
          style={{ padding: 2, margin: 1, paddingBottom: 0, backgroundColor: "rgba(52, 52, 52, 0.5)", borderLeftColor: 'white', borderLeftWidth: 2 }}
        >
          <TextInput 
            style={{ fontSize: 10, color: "gray" }}
            editable={false}
          >
            {replyMessage.text}
          </TextInput>
        </View>
      );
    }
    return null;
  };

  const renderAccessory = () => {
    if (replyMessage) { 
      return (
        <ReplyMessageBar message={replyMessage} clearReply={clearReplyMessage} />
      );
    }
    return null;
  };

  const handleButtonAction = (option: string) => {
    const responseMessage: IMessage = {
      _id: Math.random().toString(),
      text: `You selected: ${option}`,
      createdAt: new Date().getTime(),
      user: { _id: 1, name: 'User' },
      sent: false,
      received: false,
      pending: true
    };
    handleSend([responseMessage]);
  };

  const renderAutomatedMessage = (message: IMessage) => {
    return (
      <View style={styles.automatedMessageContainer}>
        <Text style={styles.automatedMessageText}>{message.text}</Text>
        <View style={styles.buttonContainer}>
          <TouchableOpacity style={styles.button} onPress={() => handleButtonAction('Option 1')}>
            <Text style={styles.buttonText}>Option 1</Text>
          </TouchableOpacity>
          <TouchableOpacity style={styles.button} onPress={() => handleButtonAction('Option 2')}>
            <Text style={styles.buttonText}>Option 2</Text>
          </TouchableOpacity>
        </View>
      </View>
    );
  };

  const renderMessage = (props: any) => {
    return <Message {...props} />;
  };

  const renderMessageImage = (imageProps: any) => {
    return (
      <TouchableWithoutFeedback onPress={() => handleImagePress(imageProps.currentMessage.image)}>
        <Image
          source={{ uri: imageProps.currentMessage.image }}
          style={{ width: 200, height: 200, borderRadius: 15 }}
        />
      </TouchableWithoutFeedback>
    );
  };

  return (
    <TouchableWithoutFeedback onPress={() => Keyboard.dismiss()}>
      <View style={styles.container}>
        {showSummary && (
          <SummaryWidget
            messages={messages}
            onYes={handleYes}
            onNo={handleNo}
            onReceiveSummary={handleSummaryReceived}
            senderUserId={Number(senderUserId)}
            receiverUserId={Number(receiverUserId)}
          />
        )}
        <GiftedChat
          messages={messages}
          onSend={handleSend} 
                   user={{
            _id: senderUserId,
            name: senderUserName?.toString?.()!,
            avatar: `https://ui-avatars.com/api/?background=000000&color=FFF&name=${senderUserName}`,
          
          }}
          renderInputToolbar={props => customInputToolbar(props, handleImagePicker)}
          renderUsernameOnMessage={true}
          inverted={false}
          renderBubble={customBubble}
          onLongPress={onLongPress}
          renderCustomView={renderReplyMessageView}
          renderAccessory={renderAccessory}
          minInputToolbarHeight={60}
          keyboardShouldPersistTaps='never'
          renderMessage={renderMessage}
          scrollToBottom
          renderMessageImage={renderMessageImage}
        />
          <Animated.View
            style={[styles.floatingButton, { transform: iconPosition.getTranslateTransform() }]}
            {...panResponder.panHandlers}
          >
            <TouchableOpacity onPress={toggleSummary}>
              <FontAwesome name="info-circle" size={24} color="black" />
            </TouchableOpacity>
          </Animated.View>
        <Modal visible={isModalVisible} transparent={true} onRequestClose={() => setModalVisible(false)}>
          <View style={styles.modalContainer}>
            <Image source={{ uri: selectedImage! }} style={styles.fullScreenImage} resizeMode="contain" />
            <TouchableWithoutFeedback onPress={() => setModalVisible(false)}>
              <View style={styles.closeButton}>
                <Text style={{ color: 'white' }}>Close</Text>
              </View>
            </TouchableWithoutFeedback>
          </View>
        </Modal>
      </View>
    </TouchableWithoutFeedback>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    padding: 10,
  },
  innerContainer: {
    flex: 1,
    padding: 10,
  },
  inputToolbar: {
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
  },
  sendContainer: {
    alignContent: "center", alignItems: "center", justifyContent: "center" 
  },
  sendButtonContainer: {
    justifyContent: "center", 
    alignItems: "center", 
    padding: 10, 
    flexDirection: 'row', 
    gap: 10 
  },
  textInput: {
    color: "white", marginTop: 4
  },
  modalContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: 'rgba(0, 0, 0, 0.8)',
  },
  floatingButton: {
    position: 'absolute',
    backgroundColor: 'white',
    borderRadius: 28,
    padding: 4,
    elevation: 5,
  },
  automatedMessageContainer: {
    padding: 10,
    backgroundColor: '#f0f0f0',
    borderRadius: 8,
    marginVertical: 5,
  },
  automatedMessageText: {
    fontSize: 16,
    marginBottom: 5,
  },
  buttonContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  button: {
    backgroundColor: '#007BFF',
    padding: 10,
    borderRadius: 5,
    flex: 1,
    marginHorizontal: 5,
  },
  buttonText: {
    color: '#fff',
    textAlign: 'center',
  },
  fullScreenImage: {
    width: '100%',
    height: '100%',
  },
  closeButton: {
    position: 'absolute',
    top: 50,
    right: 20,
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
    padding: 10,
    borderRadius: 5,
  },
});

export default ChatScreen;
