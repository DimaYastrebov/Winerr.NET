import React from 'react';
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Info } from "lucide-react";
import { Label } from '../ui/label';

interface InfoPopoverProps {
    htmlFor?: string;
    label: string;
    popoverContent: React.ReactNode;
}

export const InfoPopover: React.FC<InfoPopoverProps> = ({ htmlFor, label, popoverContent }) => {
    return (
        <div className="flex items-center gap-1.5">
            <Label htmlFor={htmlFor} className="text-zinc-400">
                {label}
            </Label>

            <Popover>
                <PopoverTrigger asChild>
                    <button
                        type="button"
                        aria-label="More info"
                        className="p-0 appearance-none bg-transparent border-none cursor-pointer"
                    >
                        <Info className="h-4 w-4 text-zinc-500 hover:text-zinc-300 transition-colors" />
                    </button>
                </PopoverTrigger>
                <PopoverContent
                    side="bottom"
                    align="start"
                    sideOffset={5}
                    className="w-auto max-w-xs bg-zinc-800 text-zinc-200 border-zinc-700 text-sm"
                >
                    {popoverContent}
                </PopoverContent>
            </Popover>
        </div>
    );
};
