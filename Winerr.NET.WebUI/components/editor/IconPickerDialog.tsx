"use client";

import React, { useEffect, useMemo, useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { ScrollArea } from "@/components/ui/scroll-area";

interface IconMapData {
    spritesheet_url: string;
    icon_size: { width: number; height: number; };
    map: { [key: string]: { x: number; y: number; } };
}

interface IconPickerDialogProps {
    isOpen: boolean;
    onOpenChange: (isOpen: boolean) => void;
    onSelect: (iconId: string) => void;
    styleId: string;
}

export const IconPickerDialog: React.FC<IconPickerDialogProps> = ({ isOpen, onOpenChange, onSelect, styleId }) => {
    const [iconData, setIconData] = useState<IconMapData | null>(null);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        if (isOpen && !iconData && styleId) {
            const fetchIconMap = async () => {
                setIsLoading(true);
                try {
                    const response = await fetch(`/v1/styles/${styleId}/icons/map`);
                    if (!response.ok) throw new Error("Failed to fetch icon map");
                    const data = await response.json();
                    setIconData(data.data);
                } catch (error) {
                    console.error("Error fetching icon map:", error);
                } finally {
                    setIsLoading(false);
                }
            };
            fetchIconMap();
        }
    }, [isOpen, styleId, iconData]);

    const handleIconClick = (iconId: string) => {
        onSelect(iconId);
        onOpenChange(false);
    };

    const sortedIconIds = useMemo(() => {
        if (!iconData) return [];
        return Object.keys(iconData.map).sort((a, b) => parseInt(a, 10) - parseInt(b, 10));
    }, [iconData]);

    return (
        <Dialog open={isOpen} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-3xl h-[80vh] bg-zinc-900 border-zinc-800 flex flex-col">
                <DialogHeader>
                    <DialogTitle>Select an Icon for '{styleId}'</DialogTitle>
                </DialogHeader>
                <div className="flex-1 overflow-hidden">
                    <ScrollArea className="h-full pr-4">
                        <div className="p-1">
                            {isLoading && (
                                <div className="grid grid-cols-[repeat(auto-fill,minmax(64px,1fr))] gap-2">
                                    {Array.from({ length: 50 }).map((_, i) => (
                                        <Skeleton key={i} className="h-16 w-16" />
                                    ))}
                                </div>
                            )}
                            {iconData && (
                                <div className="grid grid-cols-[repeat(auto-fill,minmax(64px,1fr))] gap-2">
                                    {sortedIconIds.map((id: string) => (
                                        <button
                                            key={id}
                                            onClick={() => handleIconClick(id)}
                                            className="flex flex-col items-center justify-center pt-2 rounded-md hover:bg-zinc-800 focus:outline-none focus:ring-2 focus:ring-zinc-500 transition-colors"
                                        >
                                            <div
                                                style={{
                                                    width: `${iconData.icon_size.width}px`,
                                                    height: `${iconData.icon_size.height}px`,
                                                    backgroundImage: `url(${iconData.spritesheet_url})`,
                                                    backgroundPosition: `-${iconData.map[id].x}px -${iconData.map[id].y}px`,
                                                    transform: 'scale(1)',
                                                }}
                                            />
                                            <span className="mt-1 text-xs text-zinc-400">{id}</span>
                                        </button>
                                    ))}
                                </div>
                            )}
                        </div>
                    </ScrollArea>
                </div>
            </DialogContent>
        </Dialog>
    );
};
