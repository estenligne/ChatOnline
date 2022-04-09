import React from "react";
import { IconButton } from "@mui/material";
import "./OptionsButton.css";

function OptionsButton({ options, children }) {
    const buttonRef = React.useRef(null);

    const mouseDown = () => {
        if (buttonRef.current === document.activeElement)
            setTimeout(() => buttonRef.current.blur(), 200);
    }

    return (
        <IconButton
            ref={buttonRef}
            className="OptionsButton"
            onMouseDown={mouseDown}
        >
            {children}

            <ul className="OptionsList">
                {options.map((item, i) => (
                    <li key={i} onClick={item.callback}>{item.name}</li>
                ))}
            </ul>
        </IconButton>
    );
}

export default OptionsButton;
