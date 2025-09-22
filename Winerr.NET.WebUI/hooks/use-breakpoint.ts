"use client";

import { useState, useEffect } from "react";

export const useBreakpoint = (width: number) => {
    const [isBelowBreakpoint, setIsBelowBreakpoint] = useState(false);

    useEffect(() => {
        const handleResize = () => {
            if (window.innerWidth < width) {
                setIsBelowBreakpoint(true);
            } else {
                setIsBelowBreakpoint(false);
            }
        };

        handleResize();

        window.addEventListener("resize", handleResize);

        return () => {
            window.removeEventListener("resize", handleResize);
        };
    }, [width]);

    return isBelowBreakpoint;
};
