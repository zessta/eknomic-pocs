import * as TaskManager from 'expo-task-manager';
import { getStorage, ref as storageReference, uploadBytesResumable, getDownloadURL } from 'firebase/storage';

const BACKGROUND_UPLOAD_TASK = 'BACKGROUND_UPLOAD_TASK';

TaskManager.defineTask(BACKGROUND_UPLOAD_TASK, async ({ data, error }: any) => {
  if (error) {
    console.error(error);
    return;
  }

  const { images, userId, userName } = data;

  const storage = getStorage();
  const uploadPromises = images.map(async (image: any, index: number) => {
    const response = await fetch(image.uri);
    const blob = await response.blob();
    const imageRef = storageReference(storage, `chat-images/${Date.now()}_${index}.jpg`);

    const uploadTask = uploadBytesResumable(imageRef, blob);

    // Track upload progress
    uploadTask.on('state_changed', (snapshot) => {
      const progress = (snapshot.bytesTransferred / snapshot.totalBytes) * 100;
      console.log(`Upload is ${progress}% done`);
    });

    await uploadTask;

    const downloadURL = await getDownloadURL(imageRef);
    return downloadURL;
  });

  const downloadURLs = await Promise.all(uploadPromises);
  console.log('Uploaded images:', downloadURLs);
});
