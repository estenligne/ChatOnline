export const initialState = {
    user: null,
    messages: [],
    rooms: [],
};

export const actionTypes = {
    SET_USER: "SET_USER",
    FETCH_MESSAGES: "FETCH_MESSAGES",
    SET_ROOMS: "SET_ROOMS",
};

const reducer = (state, action) => {
    console.log(action);
    switch (action.type) {
        case actionTypes.SET_USER:
            return {
                ...state,
                user: action.user,
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

export default reducer;
