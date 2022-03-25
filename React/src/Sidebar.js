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
    const [showMore, setShowMore] = useState(false)
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
        setShowMore(false);
        const contactNumber = prompt("Enter account number of contact");
        if (contactNumber) {
            const url = `/api/ChatRoom/CreatePrivate?userId=${contactNumber}`;
            _fetch(user, url, "POST")
                .then((response) => response.json())
                .then((response) => {
                    console.info("Private discussion created", response);
                    window.alert("Success! Private discussion created.");
                    fetchRooms();
                })
                .catch((err) => console.error(err));
        }
    };

    const createNewGroup = async () => {
        setShowMore(false)
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
                    fetchRooms();
                })
                .catch((err) => console.error(err));
        }
    };

    const joinNewGroup = async () => {
        setShowMore(false)
        const groupToken = prompt("Enter token to join a group");
        if (groupToken) {
            const [groupId, joinToken] = groupToken.split(': ');
            const url = `/api/GroupProfile/JoinGroup?id=${groupId}&joinToken=${encodeURIComponent(joinToken)}`;
            _fetch(user, url, "POST")
                .then((response) => response.json())
                .then((response) => {
                    console.info("Successfully joined group", response);
                    window.alert("Success! You have joined the group.");
                    fetchRooms();
                })
                .catch((err) => console.error(err));
        }
    };

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
                    <IconButton onClick={() => setShowMore(!showMore)}>
                        <MoreVertIcon />
                        {showMore ? (
                            <div className="sidebar__headerOptions">
                                <div className="overlay" onClick={() => setShowMore(false)} ></div>
                                <ul className="options">
                                    <li onClick={createNewDiscussion}>New discussion</li>
                                    <li onClick={createNewGroup}>New group</li>
                                    <li onClick={joinNewGroup}>Join group</li>
                                    <li
                                        onClick={() => {
                                            window.localStorage.removeItem(
                                                "userData"
                                            );
                                            window.location.reload();
                                        }}
                                    >
                                        Logout
                                    </li>
                                </ul>
                            </div>
                        ) : (
                            ""
                        )}
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
