import React, { useEffect, useState } from "react";
import { Avatar, IconButton } from "@mui/material";
import {
    SearchOutlined,
    AttachFile,
    MoreVert,
    Close,
    InsertEmoticon,
    Mic,
    Send,
    Cancel
} from "@mui/icons-material/";
import { useParams } from "react-router-dom";
import { useStateValue, actionTypes } from "./store";
import { _fetch, getFileURL, dateToLocal } from "./global";
import "./ChatRoom.css";
import Message from "./Message";

function ChatRoom() {
    const { roomId } = useParams();
    const [input, setInput] = useState("");
    const [roomInfo, setRoomInfo] = useState({});
    const [{ user, messages, rooms }, dispatch] = useStateValue();
    const gotoLastMessageRef = React.useRef(null);
    const messageInputRef = React.useRef(null);
    const [linkedId, setLinkedId] = useState(null);
    const [file, setFile] = useState(null);

    useEffect(() => {
        if (linkedId) {
            messageInputRef.current.focus();
        }
    }, [linkedId]);

    useEffect(() => {
        gotoLastMessageRef.current.scrollIntoView({ behavior: "auto" });
    }, [messages]);

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

    const deleteMessage = (MessageSentId) => {
        const message = messages.find((m) => m.id === MessageSentId);
        const date = new Date().toJSON();

        let args = `/api/Message?messageSentId=${message.id}&dateDeleted=${date}`;
        _fetch(user, args, "DELETE")
            .then(function (response) {
                console.log(response);
                if (response.ok) {
                    message.dateDeleted = date;
                    dispatch({
                        type: actionTypes.UPDATE_MESSAGE,
                        message,
                    });
                    // get max id of message
                    let maxId = messages[0].id;
                    messages.forEach(m => {
                        if(m.id>maxId)maxId=m.id
                    });
                    let room = rooms.find(r=>r.latestMessage.id==message.id)

                    console.log(maxId, message.id, room.latestMessage)
                    if(maxId==message.id){
                        // updateRoom shortbody
                        room.messageType = 49; //deleted
                        dispatch({
                            type: actionTypes.UPDATE_ROOM,
                            room,
                        });
                        console.log("Updated room:", room)
                    }
                    
                    console.debug("After modification.", message);
                } else {
                    alert(
                        `${response.statusText}: Can't delete message at this time.`
                    );
                }
            })
            .catch((err) => {});
    };

    const sendMessage = async (e) => {
        e.preventDefault();
        console.debug("You typed >>>", input);

        let body = {
            linkedId: linkedId ?? null,
            senderId: roomInfo.userChatRoomId,
            messageTag: { chatRoomId: roomId },
            body: input,
            dateSent: new Date().toJSON(),
        };

        // upload file here. Default fetch api behavior.
        let fileDto = null;
        if (file) {
            const formData = new FormData();
            formData.append("file", file);

            fileDto = await _fetch(user, "/api/File", "POST", null, formData)
                .then((response) => {
                    setFile(null);
                    return response.json();
                })
                .then((response) => {
                    console.log("In upload file", response)
                    body.fileId = response.id;
                    console.log("File in response now", file);
                    return response;
                })
                .catch(error => {
                    setFile(null);
                    console.error(new Error(error));
                    return null;
                });
        }

        _fetch(user, "/api/Message", "POST", body)
            .then((response) => response.json())
            .then((message) => {

                setLinkedId(null);
                setFile(null);
                setInput("");
                message.file = fileDto;

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
            })
            .catch((err) => {
                setFile(null)
                console.error(new Error(err));
            });
    };

    const linked = linkedId ? messages.find((m) => m.id === linkedId) : null;

    return (
        <div className="room">
            <div className="room__header">
                <Avatar src={getFileURL(roomInfo.photoFileName)} />

                <div className="room__headerInfo">
                    <h3>{roomInfo.name}</h3>
                    <p>
                        {dateToLocal(
                            roomInfo.latestMessage?.id
                                ? roomInfo.latestMessage?.dateSent
                                : null
                        )}
                    </p>
                </div>

                <div className="room__headerRight">
                    <IconButton>
                        <SearchOutlined />
                    </IconButton>
                    <IconButton>
                        <label htmlFor="file">
                            <AttachFile />
                        </label>
                    </IconButton>
                    <IconButton>
                        <MoreVert />
                    </IconButton>
                </div>
            </div>

            <div className="room__body">
                {messages.map((message) => (
                    <Message
                        key={message.id}
                        messages={messages}
                        message={message}
                        roomInfo={roomInfo}
                        setLinkedId={setLinkedId}
                        deleteMessage={deleteMessage}
                    />
                ))}
                <div ref={gotoLastMessageRef}></div>
            </div>

            {file && (
                <div className="shareImgContainer">
                    <div>
                        <AttachFile />
                        <span>{file.name}</span>
                    </div>

                    <Cancel
                        className="shareCancelImg"
                        onClick={() => setFile(null)}
                    />
                </div>
            )}

            {linked ? (
                <div className="chat__reply">
                    <p className="">
                        <span className="chat__refname">
                            {linked.senderName}
                        </span>
                        {linked.body}
                    </p>
                    <Close className="" onClick={() => setLinkedId(null)} />
                </div>
            ) : null}

            <div className="room__footer">
                <InsertEmoticon />
                <form>
                    <input
                        ref={messageInputRef}
                        value={input}
                        onChange={(e) => setInput(e.target.value)}
                        type="text"
                        placeholder="Type a message"
                    />
                    <input
                        type="file"
                        id="file"
                        style={{ display: "none" }}
                        onChange={(e) => setFile(e.target.files[0])}
                    />
                    <button onClick={sendMessage} type="submit">
                        Send a message
                    </button>
                </form>
                {input.length > 0 ? <Send onClick={sendMessage} /> : <Mic />}
            </div>
        </div>
    );
}

export default ChatRoom;
