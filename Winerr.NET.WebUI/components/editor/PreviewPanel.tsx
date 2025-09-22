"use client";

import React from "react";
import { LoaderCircle } from "lucide-react";

interface PreviewPanelProps {
    imageUrl: string | null;
    isGenerating: boolean;
    mode: 'single' | 'batch';
}

export const PreviewPanel: React.FC<PreviewPanelProps> = ({ imageUrl, isGenerating, mode }) => {

    const renderContent = () => {
        if (isGenerating) {
            return (
                <div className="flex flex-col items-center gap-4">
                    <LoaderCircle className="h-12 w-12 text-zinc-500 animate-spin" />
                    <p className="text-zinc-400">
                        {mode === 'single' ? "Generating image..." : "Generating archive..."}
                    </p>
                </div>
            );
        }

        if (imageUrl && mode === 'single') {
            return (
                <img
                    src={imageUrl}
                    alt="Generated error window"
                    className="max-w-full max-h-full object-contain"
                />
            );
        }

        return (
            <p className="text-zinc-400">
                {mode === 'single' ? "Preview will be shown here" : "Batch mode: Preview is not available"}
            </p>
        );
    };

    return (
        <div className="h-full flex items-center justify-center bg-zinc-950 p-8">
            {renderContent()}
        </div>
    );
};
