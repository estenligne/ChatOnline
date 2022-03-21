import React, { createContext, useContext, useReducer } from "react";

export const StateContext = createContext();

const default_dispatch = () => console.error('Store is NOT ready');

export const store = {
    dispatch: default_dispatch
};

export const StateProvider = ({ reducer, initialState, children }) => {
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
