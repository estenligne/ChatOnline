import React from 'react';
import { Avatar } from '@mui/material';
import { Link } from 'react-router-dom';
import { getFileURL } from './global';
import './SidebarChat.css';

function SidebarChat({ room }) {

    let checkmark = "";
    if (room.latestMessage.notReceivedCount > 0) {
        checkmark = "✔";
    }
    else if (room.latestMessage.notReadCount > 0) {
        checkmark = "✔✔";
    }
    else if (room.latestMessage.id !== 0) {
        checkmark = "✔✔✔";
    }

    const body = room.latestMessage.id === 0 ?
        <i>(no message)</i> : room.latestMessage.messageType == 49? <i>(deleted)</i> :
        room.latestMessage.shortBody;

    return (
        <Link to={`/rooms/${room.id}`}>
            <div className="sidebarChat">
                <Avatar src={getFileURL(room.photoFileName)} />

                <div className="sidebarChat__info">
                    <div className='chat__messageInfo'>
                        <h2>{room.name}</h2>
                        <p>{body}</p>
                    </div>
                    <div className='chat__readInfo'>
                        {checkmark}
                    </div>
                </div>
            </div>
        </Link>
    );
}

export default SidebarChat;
