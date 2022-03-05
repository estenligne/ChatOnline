import React, { useEffect, useState } from 'react';
import { Avatar, IconButton } from '@mui/material';
import { SearchOutlined, AttachFile, MoreVert } from '@mui/icons-material/';
import { InsertEmoticon, Mic } from '@mui/icons-material/';
import { useParams } from 'react-router-dom';
import { useStateValue } from './StateProvider';
import firebase from 'firebase/compat/app';
import { _fetch, getFileURL, dateToLocal } from './global';
import './Chat.css';

function Chat() {
    const { roomId } = useParams();
    const [input, setInput] = useState("");
    const [roomInfo, setRoomInfo] = useState({});
    const [messages, setMessages] = useState([]);
    const [{ user }, dispatch] = useStateValue();

    useEffect(() => {
        if (roomId) {
            _fetch(user, "/api/ChatRoom/GetInfo?id=" + roomId)
                .then(response => response.json())
                .then(roomInfo => setRoomInfo(roomInfo));

            _fetch(user, "/api/Message/GetMany?chatRoomId=" + roomId)
                .then(response => response.json())
                .then(messages => setMessages(messages));
        }
    }, [roomId, user]);

    const sendMessage = (e) => {
        e.preventDefault();
        console.debug("You typed >>>", input);

        const body = {
            senderId: roomInfo.userChatRoomId,
            messageTag: { chatRoomId: roomId },
            body: input,
            dateSent: new Date().toJSON(),
        }

        _fetch(user, "/api/Message", "POST", body)
            .then(response => response.json())
            .then(message => setMessages([...messages, message]));

        setInput("");
    }

    return (
        <div className="chat">
            <div className="chat__header">
                <Avatar src={getFileURL(roomInfo.photoFileName)} />

                <div className="chat__headerInfo">
                    <h3>{roomInfo.name}</h3>
                    <p>{dateToLocal(roomInfo.latestMessage?.id ? roomInfo.latestMessage?.dateSent : null)}</p>
                </div>

                <div className="chat__headerRight">
                    <IconButton>
                        <SearchOutlined />
                    </IconButton>
                    <IconButton>
                        <AttachFile />
                    </IconButton>
                    <IconButton>
                        <MoreVert />
                    </IconButton>
                </div>
            </div>

            <div className="chat__body">
                {messages.map(message => (
                    <p key={message.id} className={`chat__message ${message.senderId === roomInfo.userChatRoomId && "chat__receiver"}`}>
                        <span className="chat__name">{message.senderName}</span>
                        {message.body}
                        <span className="chat__timestamp">
                            {dateToLocal(new Date(message.dateSent))}
                        </span>
                    </p>
                ))}
            </div>

            <div className="chat__footer">
                <InsertEmoticon />
                <form>
                    <input value={input} onChange={(e) => setInput(e.target.value)} type="text" placeholder="Type a message" />
                    <button onClick={sendMessage} type="submit">Send a message</button>
                </form>
                <Mic />
            </div>
        </div>
    )
}

export default Chat;
