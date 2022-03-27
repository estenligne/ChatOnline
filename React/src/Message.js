import React, { useState } from "react";
import { getFileURL, dateToLocal } from "./global";

function Message({ messages, message, roomInfo, setLinkedId }) {
    return (
        <div
            key={message.id}
            className={`chat__message ${message.senderId === roomInfo.userChatRoomId &&
                "chat__receiver"
                }`}
        >
            <div className="chat__name">
                <span>{message.senderName}</span>
                <div className="chat_carretParent">
                    <CarretDownIcon setLinkedId={setLinkedId} message={message} />
                </div>
            </div>

            {message.linkedId ? (
                <p className="chat__ref">
                    <span className="chat__refname">
                        {
                            getMessageById(
                                messages,
                                message.linkedId
                            )?.senderName
                        }
                    </span>
                    {
                        getMessageById(messages, message.linkedId)
                            ?.body
                    }
                </p>
            ) : null}

            {message.file ? (
                <div className="chat__image">
                    <div className="chat__imageLink">
                        <img
                            src={getFileURL(message.file.name)}
                            alt=""
                        />
                    </div>
                </div>
            ) : null}

            {message.body}
            <span className="chat__timestamp">
                {dateToLocal(new Date(message.dateSent))}
            </span>
        </div>
    );
}

function CarretDownIcon({ message, setLinkedId, ...props }) {
    const [showChildren, setShowChildren] = useState(false)
    return (
        <div className="carret" {...props} onClick={() => setShowChildren(true)} onMouseLeave={() => setShowChildren(false)} >
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


            <div className="message__options">
                {showChildren ? (
                    <div className="list__reply">
                        <ul className="options">
                            <li
                                onClick={() => {
                                    setLinkedId(
                                        message.id
                                    )
                                    setShowChildren(false)
                                }
                                }
                            >
                                reply
                            </li>
                            <li>Delete</li>
                        </ul>
                    </div>
                ) : ""}
            </div>
        </div>
    );
}

function getMessageById(listOfMessages, id) {
    const message = listOfMessages.filter(message => message.id === id)
    return message[0]
}

export default Message;
