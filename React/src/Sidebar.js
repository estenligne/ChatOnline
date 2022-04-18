import React, { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useStateValue, actionTypes } from "./store";

import { Avatar, IconButton } from "@mui/material";
import { SearchOutlined } from "@mui/icons-material/";

import DonutLargeIcon from "@mui/icons-material/DonutLarge";
import ChatIcon from "@mui/icons-material/Chat";
import MoreVertIcon from "@mui/icons-material/MoreVert";

import SidebarChat from "./SidebarChat";
import OptionsButton from "./OptionsButton";

import { _fetch, getFileURL } from "./global";
import "./Sidebar.css";

function Sidebar() {
    const navigate = useNavigate();
    const [{ user, rooms }, dispatch] = useStateValue();

    const fetchRooms = () => {
        _fetch(user, "/api/ChatRoom/GetAll")
            .then((response) => response.json())
            .then((rooms) => {
                dispatch({ type: actionTypes.SET_ROOMS, rooms });
            });
    }
    useEffect(fetchRooms, [user, dispatch]);

    const createNewDiscussion = () => {
        const accountID = prompt("Enter account ID of contact");
        if (accountID) {
            const url = `/api/ChatRoom/CreatePrivate?accountID=${encodeURIComponent(accountID)}`;
            _fetch(user, url, "POST")
                .then((response) => response.json())
                .then((response) => {
                    console.info("Private discussion created", response);
                    window.alert("Success! Private discussion created.");
                    navigate(`/rooms/${response.chatRoomId}`);
                })
                .catch((err) => console.error(err));
        }
    };

    const createNewGroup = () => {
        const groupName = prompt("Enter name of new group");
        if (groupName) {
            const body = {
                groupName: groupName,
                joinToken: groupName,
            }
            _fetch(user, "/api/GroupProfile", "POST", body)
                .then((response) => response.json())
                .then((response) => {
                    console.info("New group created", response);
                    const groupToken = response.chatRoomId + ": " + response.chatRoom.groupProfile.joinToken;
                    window.alert(`Success! The token to join is "${groupToken}"`);
                    navigate(`/rooms/${response.chatRoomId}`);
                })
                .catch((err) => console.error(err));
        }
    };

    const joinNewGroup = () => {
        const groupToken = prompt("Enter token to join a group");
        if (groupToken) {
            const [groupId, joinToken] = groupToken.split(': ');
            const url = `/api/GroupProfile/JoinGroup?id=${groupId}&joinToken=${encodeURIComponent(joinToken)}`;
            _fetch(user, url, "POST")
                .then((response) => response.json())
                .then((response) => {
                    console.info("Successfully joined group", response);
                    window.alert("Success! You have joined the group.");
                    navigate(`/rooms/${response.chatRoomId}`);
                })
                .catch((err) => console.error(err));
        }
    };

    const logout = () => {
        const url = "/api/DeviceUsed?id=" + user.deviceUsedId;
        _fetch(user, url, "DELETE");
        window.localStorage.removeItem("userData");
        window.location.reload();
    };

    const settingsOptions = [
        { name: "New discussion", callback: createNewDiscussion },
        { name: "New group", callback: createNewGroup },
        { name: "Join group", callback: joinNewGroup },
        { name: "Logout", callback: logout }
    ];

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
                    <OptionsButton options={settingsOptions}>
                        <MoreVertIcon />
                    </OptionsButton>
                </div>
            </div>

            <div className="sidebar__search">
                <div className="sidebar__searchContainer">
                    <SearchOutlined />
                    <input placeholder="Search or start new chat" type="text" />
                </div>
            </div>

            <div className="sidebar__chats">
                {rooms.map((room) => (
                    <SidebarChat key={room.id} room={room} />
                ))}
            </div>
        </div>
    );
}

export default Sidebar;
