import React from 'react';
import { Avatar } from '@mui/material';
import { Link } from 'react-router-dom';
import { getFileURL } from './global';
import './SidebarChat.css';

function SidebarChat({ room, addNewChat }) {
    
     // this works as a checkmark. I don't have any idea about how it shows up here
    const [checkmark, setCheckmark] = React.useState([''])

    React.useEffect(()=>{
        if(room){
            if(room.latestMessage.notReadCount > 0){
                setCheckmark((_prev)=>[..._prev, '✔'])
            }
            if(room.latestMessage.notReceivedCount > 0){
                setCheckmark((_prev)=>[..._prev, '✔'])
            }
        }
    },[])
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
                        {checkmark.map((mark, idx) => (
                            <span key={idx}>{mark}</span>
                        ))}
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
