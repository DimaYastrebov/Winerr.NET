"use client";

import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { ServerCrash, RefreshCw } from "lucide-react";
import { Button } from "../ui/button";
import { toast } from "sonner";

interface ServerDownOverlayProps {
    isOpen: boolean;
    onReload: () => void;
}

export const ServerDownOverlay: React.FC<ServerDownOverlayProps> = ({ isOpen, onReload }) => {
    const { t } = useTranslation();
    const [isChecking, setIsChecking] = useState(false);

    const handleCheckServer = async () => {
        setIsChecking(true);
        try {
            const response = await fetch('/v1/health');
            if (response.ok) {
                onReload();
            } else {
                throw new Error('Server responded with an error');
            }
        } catch (error) {
            console.error("Server check failed:", error);
            toast.error("Still unavailable", {
                description: "The server did not respond. Please try again in a moment.",
            });
            setIsChecking(false);
        }
    };

    if (!isOpen) {
        return null;
    }

    return (
        <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="server-down-title"
            className="fixed inset-0 z-50 flex h-screen w-screen flex-col items-center justify-center bg-black/80 p-4 text-center text-white backdrop-blur-sm"
        >
            <div className="flex flex-col items-center gap-4">
                <ServerCrash className="h-20 w-20 text-red-500" />

                <h2 id="server-down-title" className="text-3xl font-bold">
                    {t('server_down.title')}
                </h2>

                <p className="max-w-md text-zinc-400">
                    {t('server_down.description')}
                </p>

                <Button
                    onClick={handleCheckServer}
                    disabled={isChecking}
                    className="mt-4 bg-red-600 hover:bg-red-700 text-white w-[160px]"
                    size="lg"
                >
                    {isChecking ? (
                        <>
                            <RefreshCw className="mr-2 h-5 w-5 animate-spin" />
                            {t('server_down.checking_button')}
                        </>
                    ) : (
                        <>
                            <RefreshCw className="mr-2 h-5 w-5" />
                            {t('server_down.reload_button')}
                        </>
                    )}
                </Button>
            </div>
        </div>
    );
};