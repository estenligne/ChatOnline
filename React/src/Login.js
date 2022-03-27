import React, { useEffect } from 'react';
import { Button } from '@mui/material/';
import { getFcmToken } from './firebase';
import { _fetch, AccountBaseURL, trimObject, getFormValues } from './global';
import { useStateValue } from './StateProvider';
import { actionTypes } from './reducer';
import './Login.css';

function Login() {
    const [{ }, dispatch] = useStateValue();

    useEffect(() => {
        const userData = window.localStorage.getItem('userData');
        if (userData) {
            dispatch({
                type: actionTypes.SET_USER,
                user: JSON.parse(userData)
            });
        }
    }, [dispatch]);

    const signIn = (e) => {
        e.preventDefault();

        const inputIds = ["emailAddress", "phoneNumber", "password"];
        const values = getFormValues(inputIds);
        if (!values) // if one of the inputs is invalid
            return;

        const body = trimObject({
            "email": values.emailAddress,
            "phoneNumber": values.phoneNumber,
            "password": values.password,
        });

        _fetch(null, AccountBaseURL + "Authenticate", "POST", body)
            .then(response => response.json())
            .then(result => {
                let user = {
                    account: result
                };

                let args = "?platform=WebApp";
                args += "&language=" + window.navigator.language;
                args += "&timezone=" + Intl.DateTimeFormat().resolvedOptions().timeZone;

                _fetch(user, "/api/DeviceUsed" + args, "PUT")
                    .then(response => response.json())
                    .then(deviceUsed => {
                        user = {
                            ...user,
                            ...deviceUsed.userProfile,
                            deviceUsedId: deviceUsed.id
                        }
                        registerFcmToken(user);

                        dispatch({
                            type: actionTypes.SET_USER,
                            user: user
                        });
                        window.localStorage.setItem('userData', JSON.stringify(user));
                    })
            }).catch(error => console.error(error));
    };

    function registerFcmToken(user) {
        registerServiceWorker();

        getFcmToken().then(fcmToken => {
            if (fcmToken) {
                let endpoint = "/api/DeviceUsed/RegisterFcmToken";
                endpoint += "?deviceUsedId=" + user.deviceUsedId;
                endpoint += "&fcmToken=" + encodeURIComponent(fcmToken);
                _fetch(user, endpoint, "PATCH");
            }
            else console.warn('No registration token available.');
        });
    }

    function registerServiceWorker() {
        if ("serviceWorker" in navigator) {
            navigator.serviceWorker
                .register("/firebase-messaging-sw.js")
                .then(function (registration) {
                    console.log("Registration successful, scope is:", registration.scope);
                })
                .catch(function (err) {
                    console.error("Service worker registration failed, error:", err);
                });
        }
    }

    return (
        <div className="login">
            <form className="login__container">

                <p style={{ color: 'brown' }}>Use either email or phone</p>
                <br />
                <p><label htmlFor="emailAddress">Email Address</label></p>
                <p><input type="email" id="emailAddress" placeholder="user@example.com" /></p>
                <br />
                <p><label htmlFor="phoneNumber">Phone Number</label></p>
                <p><input type="tel" id="phoneNumber" placeholder="+237123456789" /></p>
                <br />
                <p><label htmlFor="password">Password</label></p>
                <p><input type="password" id="password" placeholder="password" minLength="6" required /></p>

                <Button type="submit" onClick={signIn}>
                    Sign In to EstEnLigne
                </Button>
            </form>
        </div>
    )
}

export default Login;
