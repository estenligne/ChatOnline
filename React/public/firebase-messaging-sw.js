importScripts('https://www.gstatic.com/firebasejs/8.2.0/firebase-app.js');
importScripts('https://www.gstatic.com/firebasejs/8.2.0/firebase-messaging.js');

const enums = {
    PushNotificationTopic: {
        None: 0,
        MessageSent: 1,
        MessageReceived: 2,
        MessageRead: 3,
        MessageDeleted: 4,
    }
}

const firebaseConfig = {
    apiKey: "AIzaSyBZsYMP0JCT5qYXAf-ptlEWnTVXW2CPhv4",
    authDomain: "rhyscitlema-chatonline.firebaseapp.com",
    projectId: "rhyscitlema-chatonline",
    storageBucket: "rhyscitlema-chatonline.appspot.com",
    messagingSenderId: "310036780903",
    appId: "1:310036780903:web:e65c228dc6a65173bd41c2"
};

firebase.initializeApp(firebaseConfig);

const messaging = firebase.messaging();

// If you would like to customize notifications that are received in the
// background (Web app is closed or not in browser focus) then you should
// implement this optional method.
// Keep in mind that FCM will still show notification messages automatically 
// and you should use data messages for custom notifications.
// For more info see: 
// https://firebase.google.com/docs/cloud-messaging/concept-options

messaging.onBackgroundMessage(function(payload) {
    console.log('Background message received', payload);

    const notification = JSON.parse(payload.data.PushNotificationDTO);

    if (notification.topic === enums.PushNotificationTopic.MessageSent) {

        const notificationOptions = {
            body: notification.body,
            icon: '/logo192.png'
        };

        self.registration.showNotification(notification.title, notificationOptions);
    }
});

self.addEventListener("notificationclick", function (event) {
    console.debug(event);
});
