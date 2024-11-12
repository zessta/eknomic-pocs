import firebase from '@react-native-firebase/app';
import { FirebaseApp, getDatabase } from '@react-native-firebase/database';
// import { getStorage } from '@react-native-firebase/storage';

// Initialize Firebase
const firebaseConfig = {
  apiKey: 'AIzaSyB84_pTkchPboj-mDCwk0xpYQY9kGubTRU',
  authDomain: 'eknomicschatpoc.firebaseapp.com',
  databaseURL: 'https://eknomicschatpoc-default-rtdb.asia-southeast1.firebasedatabase.app/',
  projectId: 'eknomicschatpoc',
  storageBucket: 'eknomicschatpoc.appspot.com',
  messagingSenderId: '789203999300',
  appId: '1:789203999300:android:d456bcdaf25cc5f7023a11',
//   measurementId: 'G-measurement-id',
};

let app: FirebaseApp;
if (firebase.apps.length === 0) {
    app = firebase.initializeApp(firebaseConfig )
} else {
    app = firebase.app()
}

// const database = firebase.database();
// const storage = firebase.;

// const auth = firebase.auth();
// const app = !getApps().length ? initializeApp(firebaseConfig) : getApp();

const database = getDatabase(app);
// const storage = getStorage(app)
// const app = initializeApp(firebaseConfig);
// Get a reference to the database
// const database = getDatabase(app);

export { app, database,  };
// export { database, storage };
// For more information on how to access Firebase in your project,
// see the Firebase documentation: https://firebase.google.com/docs/web/setup#access-firebase
// import { database } from '../../components/firebaseConfig';
