import React from "react";

function Message({...props}) {
    const {
        messages,
    message,
    roomInfo,
    getMessageById,
    setLinkedId,
    Link,
    getFileURL,
    dateToLocal,
    } = props

    const [showReply, setShowReply] = React.useState(false)
    return (
        <div
        onMouseEnter={()=>setShowReply(true)}
        onMouseLeave={()=>setShowReply(false)}
            key={message.id}
            className={`chat__message ${
                message.senderId === roomInfo.userChatRoomId && "chat__receiver"
            }`}
        >
            <div className="chat__name">
                <span>{message.senderName}</span>
                {showReply ? (
                    <ul>
                        <li onClick={() => setLinkedId(message.id)}>reply</li>
                    </ul>
                ) : (
                    ""
                )}
            </div>
            {message.linkedId ? (
                <div className="chat__ref">
                    <span className="chat__refname">
                        {getMessageById(messages, message.linkedId)?.senderName}
                    </span>
                    {getMessageById(messages, message.linkedId)?.file ? (
                        <p className="chat__image">
                            <Link
                                className="chat__imageLink"
                                to={
                                    getMessageById(messages, message.linkedId)
                                        ?.file.name
                                }
                                target="_blank"
                            >
                                <img
                                    src={getFileURL(
                                        getMessageById(
                                            messages,
                                            message.linkedId
                                        )?.file.name
                                    )}
                                    alt=""
                                />
                            </Link>
                        </p>
                    ) : (
                        ""
                    )}
                    {getMessageById(messages, message.linkedId)?.body}
                </div>
            ) : (
                ""
            )}
            {message.file ? (
                <p className="chat__image">
                    <Link
                        className="chat__imageLink"
                        to={message.file.name}
                        target="_blank"
                    >
                        <img src={getFileURL(message.file.name)} alt="" />
                    </Link>
                </p>
            ) : (
                ""
            )}

            {message.body}
            <span className="chat__timestamp">
                {dateToLocal(new Date(message.dateSent))}
            </span>
        </div>
    );
}

export default Message;
