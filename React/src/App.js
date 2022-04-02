import './App.css';
import React from 'react';
import { useStateValue } from './StateProvider';
import Login from './Login';
import Sidebar from './Sidebar';
import ChatRoom from './ChatRoom';
import { BrowserRouter, Routes, Route } from 'react-router-dom';

function App() {
    const [{ user }, dispatch] = useStateValue();

    return (
        // BEM naming convention
        <div className="app">
            {!user ? (
                <Login />
            ) : (
                <div className="app__body">
                    <BrowserRouter>
                        <Sidebar />
                        <Routes>
                            <Route path="/" element={<ChatRoom />} />
                            <Route path="/rooms/:roomId" element={<ChatRoom />} />
                        </Routes>
                    </BrowserRouter>
                </div>
            )}
        </div>
    );
}

export default App;
