import React, { useEffect, useState } from "react";
import { useStateValue } from "./StateProvider";

import { Avatar, IconButton } from "@mui/material";
import { SearchOutlined } from "@mui/icons-material/";

import DonutLargeIcon from "@mui/icons-material/DonutLarge";
import ChatIcon from "@mui/icons-material/Chat";
import MoreVertIcon from "@mui/icons-material/MoreVert";

import SidebarChat from "./SidebarChat";
import { _fetch, getFileURL } from "./global";
import "./Sidebar.css";
import { actionTypes } from "./reducer";

function Sidebar() {
    const [{ user, rooms }, dispatch] = useStateValue();
    console.log(rooms);
    useEffect(() => {
        _fetch(user, "/api/ChatRoom/GetAll")
            .then((response) => response.json())
            .then((rooms) => {
                dispatch({ type: actionTypes.SET_ROOMS, rooms });
            });
    }, [user]);

    return (
        <div className="sidebar">
            <div className="sidebar__header">
                <Avatar src={getFileURL(user?.photoFile?.name)} />

                <div className="sidebar__headerRight">
                    <IconButton>
                        <DonutLargeIcon />
                    </IconButton>
                    <IconButton>
                        <ChatIcon />
                    </IconButton>
                    <IconButton>
                        <MoreVertIcon />
                    </IconButton>
                </div>
            </div>

            <div className="sidebar__search">
                <div className="sidebar__searchContainer">
                    <SearchOutlined />
                    <input placeholder="Search or start new chat" type="text" />
                </div>
            </div>

            <div className="sidebar__chats">
                <SidebarChat addNewChat />
                {rooms.map((room) => (
                    <SidebarChat key={room.id} room={room} />
                ))}
            </div>
        </div>
    );
}

export default Sidebar;
