import React from 'react';
import { Avatar } from '@mui/material';
import { Link } from 'react-router-dom';
import { getFileURL } from './global';
import './SidebarChat.css';

function SidebarChat({ room, addNewChat }) {

    let checkmark = "";

    if (room) {
        if (room.latestMessage.notReceivedCount > 0) {
            checkmark = "✔";
        }
        else if (room.latestMessage.notReadCount > 0) {
            checkmark = "✔✔";
        }
        else checkmark = "✔✔✔";
    }

    const createChat = () => {
        alert("Not yet implemented!");
    };

    return !addNewChat ? (
        <Link to={`/rooms/${room.id}`}>
            <div className="sidebarChat">
                <Avatar src={getFileURL(room.photoFileName)} />

                <div className="sidebarChat__info">
                    <div className='chat__messageInfo'>
                        <h2>{room.name}</h2>
                        <p>{room.latestMessage.shortBody}</p>
                    </div>
                    <div className='chat__readInfo'>
                        {checkmark}
                    </div>
                </div>
            </div>
        </Link>
    ) : (
        <div onClick={createChat} className="sidebarChat">
            <h2>Add New Chat</h2>
        </div>
    );
}

export default SidebarChat;
