import React, { useEffect, useState } from 'react';
import { Avatar, IconButton } from '@mui/material';
import { SearchOutlined, AttachFile, MoreVert } from '@mui/icons-material/';
import { InsertEmoticon, Mic } from '@mui/icons-material/';
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

                <div className="chat__headerInfo">
                    <h3>Room name</h3>
                    <p>Last seen at...</p>
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
                <p className={`chat__message ${true && "chat__receiver"}`}>
                    <span className="chat__name">yemelitc</span>
                    Hey Guys
                    <span className="chat__timestamp">15:52</span>
                </p>
                <p className="chat__message">Hey Guys</p>
            </div>

            <div className="chat__footer">
                <InsertEmoticon />
                <form>
                    <input type="text" placeholder="Type a message" />
                    <button type="submit">Send a message</button>
                </form>
                <Mic />
            </div>
        </div>
    )
}

export default Chat;
