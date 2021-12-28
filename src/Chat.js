import React, { useEffect, useState } from 'react';
import { Avatar } from '@mui/material';
import './Chat.css';

function Chat() {
    const [seed, setSeed] = useState("");

    useEffect(() => {
        setSeed(Math.floor(Math.random() * 5000));
    }, []);

    return (
        <div className="chat">
            <div className="chat__header">
                <Avatar src={`https://avatars.dicebear.com/api/human/${seed}.svg`} />
            </div>

            <div className="chat__body">
                
            </div>

            <div className="chat__footer">
                
            </div>
        </div>
    )
}

export default Chat;
