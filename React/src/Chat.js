import React, { useEffect, useState } from "react";
import { Avatar, IconButton } from "@mui/material";
import { SearchOutlined, AttachFile, MoreVert } from "@mui/icons-material/";
import { InsertEmoticon, Mic } from "@mui/icons-material/";
import { Link, useParams } from "react-router-dom";
import { useStateValue } from "./StateProvider";
import { _fetch, getFileURL, dateToLocal } from "./global";
import "./Chat.css";
import { actionTypes } from "./reducer";

function Chat() {
    const { roomId } = useParams();
    const [input, setInput] = useState("");
    const [roomInfo, setRoomInfo] = useState({});
    const [{ user, messages, rooms }, dispatch] = useStateValue();
    const gotoLastMessageRef = React.useRef(null);
    const [linkedId, setLinkedId] = useState(null);
    const [showMore, setShowMore] = React.useState(false)

    React.useEffect(() => {
        gotoLastMessageRef.current.scrollIntoView({ behavior: "auto" });
    });
    useEffect(() => {
        if (roomId) {
            _fetch(user, "/api/ChatRoom/GetInfo?id=" + roomId)
                .then((response) => response.json())
                .then((roomInfo) => setRoomInfo(roomInfo));

            _fetch(user, "/api/Message/GetMany?chatRoomId=" + roomId)
                .then((response) => response.json())
                .then((message) => {
                    dispatch({
                        type: actionTypes.FETCH_MESSAGES,
                        messages: message,
                    });
                });
        }
    }, [roomId, user]);

    const sendMessage = (e) => {
        e.preventDefault();
        console.debug("You typed >>>", input);

        const body = {
            linkedId: linkedId ?? null,
            senderId: roomInfo.userChatRoomId,
            messageTag: { chatRoomId: roomId },
            body: input,
            dateSent: new Date().toJSON(),
        };

        _fetch(user, "/api/Message", "POST", body)
            .then((response) => response.json())
            .then((message) => {
                setLinkedId(null);

                // update messages in the store
                dispatch({
                    type: actionTypes.FETCH_MESSAGES,
                    messages: [...messages, message],
                });

                // update rooms in the store
                _fetch(user, "/api/ChatRoom/GetAll")
                    .then((response) => response.json())
                    .then((_rooms) => {
                        dispatch({
                            type: actionTypes.SET_ROOMS,
                            rooms: _rooms,
                        });
<<<<<<< HEAD
                        setRoomInfo(
                            _rooms.find((room) => room.id === parseInt(roomId))
                        );
=======
                        setRoomInfo(_rooms.find(room => room.id == parseInt(roomId)))
>>>>>>> parent of 965cb40 (Get linked message and display on the UI as a tagged messge)
                    });
                });

        console.log("after sent, ROOMS", rooms);
        setInput("");
    };
    /* const fileChangeHandler = (event) => {
		setSelectedFile(event.target.files[0]);
		setIsFilePicked(true);
	}; */

    return (
        <div className="chat">
            <div className="chat__header">
                <Avatar src={getFileURL(roomInfo.photoFileName)} />

                <div className="chat__headerInfo">
                    <h3>{roomInfo.name}</h3>
                    <p>
                        {dateToLocal(
                            roomInfo.latestMessage?.id
                                ? roomInfo.latestMessage?.dateSent
                                : null
                        )}
                    </p>
                </div>

                <div className="chat__headerRight">
                    <IconButton>
                        <SearchOutlined />
                    </IconButton>
                    <IconButton>
                        {/* <input style={{display: "none"}} onChange={fileChangeHandler}  type="file" name="file" id="file" /> */}
                        <label htmlFor="file">
                            <AttachFile />
                        </label>
                    </IconButton>
                    <IconButton>
                        <MoreVert />
                    </IconButton>
                </div>
            </div>

            <div className="chat__body">
                {messages.map((message) => (
                    <p
                        key={message.id}
                        className={`chat__message ${
                            message.senderId === roomInfo.userChatRoomId &&
                            "chat__receiver"
                        }`}
                    >
<<<<<<< HEAD
                        <div className="chat__name">
                            <span>{message.senderName}</span>
                            <div className="chat_carretParent">
                                <CarretDownIcon
                                    showMore={showMore}
                                >
                                    <div className="message__options">
                                        {/* <div className="overlay"></div> */}
                                        <ul className="options">
                                            <li
                                                onClick={() =>
                                                    {setLinkedId(message.id)
                                                    setShowMore(false)}
                                                }
                                            >
                                                Reply
                                            </li>
                                            <li>Delete</li>
                                        </ul>
                                    </div>
                                </CarretDownIcon>
                            </div>
                        </div>
                        {messages.linkedId ? (
                            <div className="chat__ref">
                                <span className="chat__refname">
                                    {
                                        getMessageById(
                                            messages,
                                            message.linkedId
                                        )?.senderName
                                    }
                                </span>
                                {getMessageById(messages, message.linkedId)
                                    ?.file ? (
                                    <div className="chat__image">
                                        <div className="chat__imageLink">
                                            <img
                                                src={getFileURL(
                                                    getMessageById(
                                                        messages,
                                                        message.linkedId
                                                    )?.file.name
                                                )}
                                                alt=""
                                            />
                                        </div>
                                    </div>
                                ) : (
                                    ""
                                )}
                                {
                                    getMessageById(messages, message.linkedId)
                                        ?.body
                                }
                            </div>
                        ) : (
                            ""
                        )}
                        {message.file ? (
                            <div className="chat__image">
                              <div className="chat__imageLink">
                                <img
                                    src={getFileURL(message.file.name)}
                                    alt=""
                                />
                            </div>
                            </div>
                        ) : (
                            ""
                        )}

=======
                        <span className="chat__name">{message.senderName}</span>
>>>>>>> parent of 965cb40 (Get linked message and display on the UI as a tagged messge)
                        {message.body}
                        <span className="chat__timestamp">
                            {dateToLocal(new Date(message.dateSent))}
                        </span>
                    </p>
                ))}
                <div ref={gotoLastMessageRef}></div>
            </div>

            <div>
                {linkedId ? (
                    <div className="chat__reply">
                        <p className="">
                            <span className="chat__refname">
                                {getMessageById(messages, linkedId).senderName}
                            </span>
                            {getMessageById(messages, linkedId).body}
                        </p>
                        <p
                            className="chat_refClose"
                            onClick={() => setLinkedId(null)}
                        >
                            X
                        </p>
                    </div>
                ) : (
                    ""
                )}
                <div className="chat__footer">
                    <InsertEmoticon />
                    <form>
                        <input
                            value={input}
                            onChange={(e) => setInput(e.target.value)}
                            type="text"
                            placeholder="Type a message"
                        />
                        <button onClick={sendMessage} type="submit">
                            Send a message
                        </button>
                    </form>
                    <Mic />
                </div>
            </div>
        </div>
    );
}
function CarretDownIcon({children, showMore, ...props}) {
    const [showChildren, setShowChildren] = useState(showMore)
    return (
        <div className="carret" {...props}  onClick={() => setShowChildren(true)}>
            <div className="">
                <span
                    data-testid="down-context"
                    data-icon="down-context"
                    className=""
                >
                    <svg viewBox="0 0 18 18" width="18" height="18">
                        <path
                            fill="currentColor"
                            d="M3.3 4.6 9 10.3l5.7-5.7 1.6 1.6L9 13.4 1.7 6.2l1.6-1.6z"
                        ></path>
                    </svg>
                </span>
            </div>
            {showChildren ? children : ""}
        </div>
    );
  }

<<<<<<< HEAD
function getMessageById(listOfMessages, id) {
    const message = listOfMessages.filter(message => message.id === id)
    return message[0]
}

=======
>>>>>>> parent of 965cb40 (Get linked message and display on the UI as a tagged messge)
export default Chat;
