// https://stackoverflow.com/questions/70445014/module-not-found-error-package-path-is-not-exported-from-package/70445098
// https://firebase.google.com/docs/auth/web/google-signin#handle_the_sign-in_flow_with_the_firebase_sdk

import firebase from 'firebase/compat/app';
import 'firebase/compat/auth';
import 'firebase/compat/firestore';
import { GoogleAuthProvider } from "firebase/auth";

const firebaseConfig = {
    apiKey: "AIzaSyCRwGyf4UMLNO0R0m7Op2WYbQQG94_WcQc",
    authDomain: "tests-712.firebaseapp.com",
    projectId: "tests-712",
    storageBucket: "tests-712.appspot.com",
    messagingSenderId: "347722402306",
    appId: "1:347722402306:web:7531be9ea5c4686921f51b"
  };

  const firebaseApp = firebase.initializeApp(firebaseConfig);
  const db = firebaseApp.firestore();
  const auth = firebase.auth();
  const provider = new GoogleAuthProvider();
  
  export { auth, provider };
  export default db;
  