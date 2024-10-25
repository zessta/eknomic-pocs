import React, { useEffect, useRef, useState } from 'react';
import { Bubble, Composer, GiftedChat, IMessage, InputToolbar, Message, Send } from 'react-native-gifted-chat';
import { database } from '../components/firebaseConfig';
import { ref, onValue, push } from 'firebase/database';
import AsyncStorage from '@react-native-async-storage/async-storage';
import NetInfo from '@react-native-community/netinfo';
import { View, StyleSheet, TouchableOpacity, PanResponder, Animated, TouchableWithoutFeedback, Keyboard, Text, Image, Modal } from 'react-native';
import * as ImagePicker from 'expo-image-picker';
import { useLocalSearchParams, useNavigation } from 'expo-router';
import SummaryWidget from '../components/SummaryWidget';
import { FontAwesome, Ionicons } from '@expo/vector-icons';
import ReplyMessageBar from '../components/ReplyMessageBar';
import * as Clipboard from 'expo-clipboard';
import { TextInput } from 'react-native-paper';
import * as Notifications from 'expo-notifications';
import * as Permissions from 'expo-permissions';
import * as BackgroundFetch from 'expo-background-fetch';
import BackgroundFetchScreen from '../components/BackgroundFetch'; // Ensure the task is defined
import { getStorage, ref as storageReference, uploadBytes, getDownloadURL } from 'firebase/storage';

const customInputToolbar = (props: any, handleImagePicker: () => void) => {
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
        <Send {...props} containerStyle={{ alignContent: "center", alignItems: "center", justifyContent: "center" }}>
          <View style={{ 
            justifyContent: "center", 
            alignItems: "center", 
            padding: 10, 
            flexDirection: 'row', 
            gap: 10 
          }}>
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

const ChatScreen: React.FC = () => {
  const navigation = useNavigation();
  const storage = getStorage();
  const { receiverUserId, receiverUserName, senderUserName, senderUserId } = useLocalSearchParams();

  const [messages, setMessages] = useState<IMessage[]>([]);
  const [pendingMessages, setPendingMessages] = useState<IMessage[]>([]);
  const [isConnected, setIsConnected] = useState<boolean>(true);
  const [showSummary, setShowSummary] = useState<boolean>(false);
  const [iconPosition] = useState(new Animated.ValueXY({ x: 100, y: 100 }));
  const [replyMessage, setReplyMessage] = useState<IMessage | null>(null);
  const [isModalVisible, setModalVisible] = useState(false);
  const [selectedImage, setSelectedImage] = useState<string | null>(null);

  const handleImagePress = (uri: string) => {
    setSelectedImage(uri);
    setModalVisible(true);
  };

  const clearReplyMessage = () => setReplyMessage(null);

  const loadMessages = () => {
    const senderMessagesRef = ref(database, `chats/${senderUserId}_${receiverUserId}`);
    const receiverMessagesRef = ref(database, `chats/${receiverUserId}_${senderUserId}`);

    onValue(senderMessagesRef, snapshot => {
      const senderMessages: IMessage[] = snapshot.val() ? Object.values(snapshot.val()) : [];
      onValue(receiverMessagesRef, snapshot => {
        const receiverMessages: IMessage[] = snapshot.val() ? Object.values(snapshot.val()) : [];
        const allMessages: IMessage[] = [...senderMessages.reverse(), ...receiverMessages.reverse()].map(msg => ({
          ...msg,
          createdAt: new Date(msg.createdAt).getTime(), // Ensure createdAt is a timestamp
        })).sort((a, b) => a.createdAt - b.createdAt);

        // Check for unread messages and show notification
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
      trigger: {
        seconds: 10,
      },
    });
  };

  // const requestPermissions = async () => {
  //   const { status } = await Permissions.getAsync(Permissions.NOTIFICATIONS);
  //   if (status !== 'granted') {
  //     alert('Notification permissions are required!');
  //   }
  // };



  useEffect(() => {
    navigation.setOptions({ title: receiverUserName });
  }, [receiverUserName]);

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

  const handleSummaryReceived = async(newSummary: string) => {
    const summaryMessage: IMessage = {
      _id: Math.random().toString(),
      text: newSummary,
      createdAt: new Date().getTime(),
      user: { _id: 1, name: 'Summary' }, // Unique ID for summary
    };
    const messageRef = ref(database,` chats/${senderUserId}_${receiverUserId}`);
    await push(messageRef, {
      _id: summaryMessage._id,
      text: summaryMessage.text,
      createdAt: summaryMessage.createdAt, // Save as ISO string
      user: summaryMessage.user,
    });

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
        await push(messageRef, {
          _id: message._id,
          text: message.text,
          createdAt: new Date().getTime(),
          user: message.user,
        });
      }
      await removeValue();
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
    const message = newMessages[0];
    const updatedMessages = GiftedChat.append(messages, newMessages);
    setMessages(updatedMessages);

    if (isConnected) {
      const messageRef = ref(database, `chats/${senderUserId}_${receiverUserId}`);
      await push(messageRef, {
        _id: message._id,
        text: message.text,
        createdAt: new Date().getTime(),
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
      allowsMultipleSelection: true, // Enable multiple selection
    });
  
    if (!result.canceled) {
      const images = result.assets;
  
      await Promise.all(images.map(async (image) => {
        const message: IMessage = {
          _id: Math.random().toString(),
          text: '',
          createdAt: new Date().getTime(),
          user: { _id: senderUserId, name: senderUserName.toString() },
          image: image.uri,
        };
  
        // Upload image and get URL
        const imageUrl = await uploadImageToFirebase(image.uri); // Implement this function
        message.image = imageUrl;
  
        // Update messages in local state
        const updatedMessages = GiftedChat.append(messages, [message]);
        setMessages(updatedMessages);
  
        // Send image message to Firebase
        const messagesRef = ref(database, `chats/${senderUserId}_${receiverUserId}`);
        await push(messagesRef, {
          _id: message._id,
          text: '',
          createdAt: new Date().getTime(),
          user: message.user,
          image: imageUrl,
        });
      }));
    }
  };

  const uploadImageToFirebase = async (uri: string): Promise<string> => {
    const storage = getStorage();
    const response = await fetch(uri);
    const blob = await response.blob();
    
    const imageRef = storageReference(storage, `chat-images/${new Date().getTime()}.jpg`); // Unique file names
  
    // Upload the image
    await uploadBytes(imageRef, blob);
    
    // Get the download URL
    const downloadURL = await getDownloadURL(imageRef);
    return downloadURL;
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
            avatar: `https://ui-avatars.com/api/?background=000000&color=FFF&name=${senderUserName}`
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
    // backgroundColor: '#000',
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
  modalContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: 'rgba(0, 0, 0, 0.8)',
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
