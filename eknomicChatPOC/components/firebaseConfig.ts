import { initializeApp } from 'firebase/app';
import { getDatabase } from 'firebase/database';

// Optionally import the services that you want to use
// import {...} from "firebase/auth";
// import {...} from "firebase/database";
// import {...} from "firebase/firestore";
// import {...} from "firebase/functions";
// import {...} from "firebase/storage";

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

const app = initializeApp(firebaseConfig);
const database = getDatabase(app);

export { database };// For more information on how to access Firebase in your project,
// see the Firebase documentation: https://firebase.google.com/docs/web/setup#access-firebase
// import { database } from '../../components/firebaseConfig';
