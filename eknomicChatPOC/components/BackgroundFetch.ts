// BackgroundFetch.ts
import * as TaskManager from 'expo-task-manager';
import * as BackgroundFetch from 'expo-background-fetch';

const BACKGROUND_FETCH_TASK = 'background-fetch-task';

TaskManager.defineTask(BACKGROUND_FETCH_TASK, async () => {
  try {
    // Make your API call here
    const response = await fetch('https://api.mockae.com/fakeapi/products');
    const data = await response.json();

    // Process the data or store it
    console.log('Fetched data in background:', data);

    // Return a successful result
    return BackgroundFetch.Result.NewData;
  } catch (error) {
    console.error('Background fetch error:', error);
    // If there's an error, return a failed result
    return BackgroundFetch.Result.Failed;
  }
});
