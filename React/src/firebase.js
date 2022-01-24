// https://firebase.google.com/docs/cloud-messaging/js/client

import { initializeApp } from "firebase/app";
import { getMessaging, getToken } from "firebase/messaging";

const firebaseConfig = {
    apiKey: "AIzaSyBZsYMP0JCT5qYXAf-ptlEWnTVXW2CPhv4",
    authDomain: "rhyscitlema-chatonline.firebaseapp.com",
    projectId: "rhyscitlema-chatonline",
    storageBucket: "rhyscitlema-chatonline.appspot.com",
    messagingSenderId: "310036780903",
    appId: "1:310036780903:web:e65c228dc6a65173bd41c2"
};

const firebaseApp = initializeApp(firebaseConfig);
const messaging = getMessaging();

export function getFcmToken(user) {
    const options = {
        vapidKey: "BG4pkThZ2TFPxmpZ7mv9cMFGEBGCtZt6YRGlPLiY3bs68P8MAfGmNhQj4dOffW1vLEGu2Doqhmqxn_lqRolDWYU"
    };

    return getToken(messaging, options)
        .then(fcmToken => {
            if (fcmToken) {
                return fcmToken;
            }
            else {
                console.warn('No registration token available. Request permission to generate one.');
            }
        });
}
