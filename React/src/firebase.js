// https://firebase.google.com/docs/cloud-messaging/js/client

import { initializeApp } from "firebase/app";
import { getMessaging, getToken, onMessage } from "firebase/messaging";

import { store, actionTypes } from "./store";

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

export function getFcmToken() {
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
        }).catch(error => console.error(error));
}

const enums = {
    PushNotificationTopic: {
        None: 0,
        MessageSent: 1,
        MessageReceived: 2,
        MessageRead: 3,
        MessageDeleted: 4,
    }
}

onMessage(messaging, (payload) => {
    console.log('Message received', payload);

    const notification = JSON.parse(payload.data.PushNotificationDTO);

    if (notification.topic === enums.PushNotificationTopic.MessageSent) {

        store.dispatch({
            type: actionTypes.SET_MESSAGE,
            message: notification.messageSent,
        });
    }
});
