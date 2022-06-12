import React from "react";
import { AttachFileRounded, KeyboardArrowDown } from "@mui/icons-material";
import { getFileURL, dateToLocal } from "./global";
import OptionsButton from "./OptionsButton";
import "./Message.css";

function Message({ messages, message, roomInfo, setLinkedId }) {
    const isSender = message.senderId === roomInfo.userChatRoomId;
    const showName = !isSender && roomInfo.type === 2; // if a group

    const options = [
        { name: "Reply", callback: () => setLinkedId(message.id) },
        { name: "Delete", callback: () => alert("Not yet implemented!") },
    ];

    const linked = messages.find((m) => m.id === message.linkedId);
    const extensions = /.png|.jpg|.jpeg|.svg|.jfif|.jpe|.jif|.jfi|.gif|.webp|.bmp|.dib|.heif|.heic?/gi;

    return (
        <div
            key={message.id}
            className={"chat__message" + (isSender ? " chat__sender" : "")}
        >
            <div className="chat__name">
                <span>{showName ? message.senderName : ""}</span>

                <OptionsButton options={options}>
                    <KeyboardArrowDown />
                </OptionsButton>
            </div>

            {linked ? (
                <p className="chat__linked">
                    <span className="chat__refname">{linked.senderName}</span>
                    {linked.body}
                </p>
            ) : null}

            {!message.file ? (
                null
            ) :
                message.file.name.match(extensions) ? (
                    <div className="chat__image">
                        <div className="chat__imageLink">
                            <img src={getFileURL(message.file.name)} alt="" />
                        </div>
                    </div>
                ) : (
                    <div className="shareImgContainer">
                        <AttachFileRounded /><span>{getFileURL(message.file.name).split(" ")[3]}</span>
                    </div>
                )
            }

            {message.body}

            <span className="chat__timestamp">
                {dateToLocal(new Date(message.dateSent))}
            </span>
        </div>
    );
}

export default Message;
