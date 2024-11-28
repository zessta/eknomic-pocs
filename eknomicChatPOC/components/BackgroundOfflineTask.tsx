import * as BackgroundFetch from 'expo-background-fetch';
import * as TaskManager from 'expo-task-manager';
import * as Network from 'expo-network';
import * as FileSystem from 'expo-file-system';
import { IMessage } from 'react-native-gifted-chat';
import { push, ref, set } from '@react-native-firebase/database';
import { database } from './firebaseConfig'; // Adjust the path accordingly
import AsyncStorage from '@react-native-async-storage/async-storage';
import { ChatMessage } from '@/app/chat';

type NewIMessage = IMessage & {
  receiverUserId : string;
  senderUserId: string}

const BACKGROUND_TASK_NAME = 'send-offline-messages';

// Define the background task
TaskManager.defineTask(BACKGROUND_TASK_NAME, async () => {
  try {
    const status = await Network.getNetworkStateAsync();
    if (status.isConnected) {
      // Retrieve offline messages
      const offlineMessages = await FileSystem.readAsStringAsync(FileSystem.documentDirectory + 'offlineMessages.json').catch(() => '[]');
      const messagesArray: ChatMessage[] = JSON.parse(offlineMessages);

      if (messagesArray.length > 0) {
        messagesArray.forEach((message) => {
          const messageRef = ref(database, `chats/${message.senderUserId}_${message.receiverUserId}`);

          console.log('Sending offline message:', message);  // Simulate sending message
           push(messageRef, {
            _id: message._id,
            text: message.text,
            createdAt: new Date().getTime(),
            user: message.user,
            // receiverUserId,
            sent: true, // Initially false
            pending: false,
            received: false
          });
          // Logic to send messages here, e.g., API call
        });

        // Clear offline messages after sending
        await FileSystem.writeAsStringAsync(FileSystem.documentDirectory + 'offlineMessages.json', '[]');
      }
    return BackgroundFetch.BackgroundFetchResult.NewData;
  }

  } catch (error) {
    console.error('Background task failed', error);
    return BackgroundFetch.BackgroundFetchResult.Failed;
  }
});

// Initialize the background fetch task
const initializeBackgroundFetch = async () => {
  try {
    const status = await BackgroundFetch.getStatusAsync();
    if (status !== BackgroundFetch.BackgroundFetchStatus.Available) {
      console.log('Background fetch not available');
      return;
    }

    await BackgroundFetch.registerTaskAsync(BACKGROUND_TASK_NAME, {
      minimumInterval: 1 * 60, // Fetch every 15 minutes
      stopOnTerminate: false,
      startOnBoot: true,
    });
    console.log('Background send-offline-messages task registered successfully');
  } catch (error) {
    console.error('Error registering background task', error);
  }
};

export { initializeBackgroundFetch };
