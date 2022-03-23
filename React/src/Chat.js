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
                        setRoomInfo(
                            _rooms.find((room) => room.id === parseInt(roomId))
                        );
                    });
            });

        console.log("after sent, ROOMS", rooms);
        setInput("");
    };
    const fileChangeHandler = (event) => {
		setSelectedFile(event.target.files[0]);
		setIsFilePicked(true);
	};

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
                    <div
                        // onMouseEnter={() => setShowReplyText(true)}
                        // onMouseLeave={() => setShowReplyText(false)}
                        key={message.id}
                        className={`chat__message ${
                            message.senderId === roomInfo.userChatRoomId &&
                            "chat__receiver"
                            }`}
                    >
                        <div className="chat__name">
                            <span>{message.senderName}</span>
                            <div className="chat_carretParent">
                                <CarretDownIcon
                                    onClick={() => setShowMore(true)}
                                    >
                                        reply
                                    </li>
                                </ul>
                            ) : (
                                ""
                            )}
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
                                                    ).file.name
                                                )}
                                                alt=""
                                            />
                                        
                                    </p>
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

                        {message.body}
                        <span className="chat__timestamp">
                            {dateToLocal(new Date(message.dateSent))}
                        </span>
                    </div>
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

function getMessageById(listOfMessages, id) {
    const message = listOfMessages.filter(message => message.id === id)
    return message[0]
}

export default Chat;
