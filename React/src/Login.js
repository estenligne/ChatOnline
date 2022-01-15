import React from 'react';
import { Button } from '@mui/material/';
import { _fetch, trimObject, getFormValues } from './global';
import { useStateValue } from './StateProvider';
import { actionTypes } from './reducer';
import './Login.css';

function Login() {
    const [{ }, dispatch] = useStateValue();

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

        _fetch(null, "/api/Account/SignIn", "POST", body)
            .then(response => response.json())
            .then(result => dispatch({
                type: actionTypes.SET_USER,
                user: result
            }));
    };

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
