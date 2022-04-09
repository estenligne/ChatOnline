import React, { createContext, useContext, useReducer } from "react";

const StateContext = createContext();

const default_dispatch = () => console.error('Store is NOT ready');

export const store = {
    dispatch: default_dispatch
};

export const StateProvider = ({ children }) => {
    const [state, dispatch] = useReducer(reducer, initialState)

    if (store.dispatch !== dispatch) {
        if (store.dispatch !== default_dispatch) {
            console.error("store.dispatch is different");
        }
        store.dispatch = dispatch;
    }

    return (
        <StateContext.Provider value={[state, dispatch]}>
            {children}
        </StateContext.Provider>
    );
}

export const useStateValue = () => useContext(StateContext);

const initialState = {
    user: null,
    messages: [],
    rooms: [],
};

export const actionTypes = {
    SET_USER: "SET_USER",
    FETCH_MESSAGES: "FETCH_MESSAGES",
    SET_ROOMS: "SET_ROOMS",
    SET_MESSAGE: "SET_MESSAGE",
};

const reducer = (state, action) => {
    console.log(action);
    switch (action.type) {
        case actionTypes.SET_USER:
            return {
                ...state,
                user: action.user,
            };
        case actionTypes.SET_MESSAGE:
            return {
                ...state,
                messages: [...state.messages, action.message]
            };
        case actionTypes.FETCH_MESSAGES:
            return {
                ...state,
                messages: action.messages,
            };
        case actionTypes.SET_ROOMS:
            // sort in descending order of dateSent
            action.rooms.sort((a, b) => {
                const c = a.latestMessage.dateSent;
                const d = b.latestMessage.dateSent;
                return new Date(d) - new Date(c);
            });
            return {
                ...state,
                rooms: action.rooms,
            };
        default:
            return state;
    }
};
