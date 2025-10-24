"use client";

import React from "react";
import { LoaderCircle, Timer, Github } from "lucide-react";
import { Button } from "../ui/button";
import { useEditorStore } from "@/stores/editor-store";
import { useShallow } from 'zustand/react/shallow';

export const PreviewPanel: React.FC = () => {
    const { imageUrl, isGenerating, mode, generationTime } = useEditorStore(
        useShallow((state) => ({
            imageUrl: state.generatedImageUrl,
            isGenerating: state.isGenerating,
            mode: state.mode,
            generationTime: state.generationTime,
        }))
    );

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
                // eslint-disable-next-line @next/next/no-img-element
                <img
                    src={imageUrl}
                    alt="Generated error window preview"
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
        <div className="relative h-full flex items-center justify-center bg-zinc-950 p-8">
            {renderContent()}

            {generationTime !== null && mode === 'single' && !isGenerating && (
                <div className="absolute top-4 left-4 bg-black/60 text-white text-sm px-3 py-1.5 rounded-lg flex items-center gap-2 backdrop-blur-md border border-white/10 shadow-lg">
                    <Timer className="h-4 w-4 text-zinc-300" />
                    <span className="font-medium text-zinc-100">{generationTime}ms</span>
                </div>
            )}
            
            <a href="https://github.com/DimaYastrebov/Winerr.NET" target="_blank" rel="noopener noreferrer" aria-label="View on GitHub" className="absolute top-4 right-4">
                <Button variant="ghost" size="sm" className="text-zinc-400 hover:text-zinc-200">
                    GitHub
                    <Github className="h-4 w-4" />
                </Button>
            </a>
        </div>
    );
};