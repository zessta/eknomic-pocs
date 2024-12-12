// BackgroundSync.tsx
import * as TaskManager from 'expo-task-manager';
import * as BackgroundFetch from 'expo-background-fetch';
import NetInfo from '@react-native-community/netinfo';
import AsyncStorage from '@react-native-async-storage/async-storage';
import {  database } from './firebaseConfig';
import { firebase } from '@react-native-firebase/database';

// Define a background task name
const BACKGROUND_FETCH_TASK = 'offline-messages-sync';

// Register the background fetch task
TaskManager.defineTask(BACKGROUND_FETCH_TASK, async () => {
  try {
    // Check if the device is connected to the internet
    const netInfo = await NetInfo.fetch();
    if (netInfo.isConnected) {
      // If connected, sync offline messages to Firebase
      const offlineMessages = await AsyncStorage.getItem('offlineMessages');
      if (offlineMessages) {
        const messages = JSON.parse(offlineMessages);
        console.log('messages', messages)
        for (const message of messages) {
          await database.ref('messages').push().set({
            text: message.text,
            createdAt: firebase.database.ServerValue.TIMESTAMP,
            user: message.user,
          });
        }
        // Clear offline messages after sending them to Firebase
        await AsyncStorage.removeItem('offlineMessages');
      }
    }
    return BackgroundFetch.Result.NewData;
  } catch (error) {
    console.error('Background fetch error:', error);
    return BackgroundFetch.Result.Failed;
  }
});

// Initialize background fetch
const initBackgroundFetch = async () => {
  try {
    await BackgroundFetch.registerTaskAsync(BACKGROUND_FETCH_TASK, {
      minimumInterval: 60 * 15, // 15 minutes (you can adjust this)
      stopOnTerminate: false,
      startOnBoot: true,
    });
    console.log('Background task registered successfully');
  } catch (error) {
    console.error('Error registering background task', error);
  }
};

// Call this function in App.js or wherever you need to initialize background sync
export const startBackgroundSync = () => {
  initBackgroundFetch();
};
