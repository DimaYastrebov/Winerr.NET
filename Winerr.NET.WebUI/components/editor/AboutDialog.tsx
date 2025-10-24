"use client";

import React from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { Button } from "../ui/button";
import { Github } from "lucide-react";
import { version as appVersion } from '../../package.json';

interface AboutDialogProps {
    isOpen: boolean;
    onOpenChange: (isOpen: boolean) => void;
}

export const AboutDialog: React.FC<AboutDialogProps> = ({ isOpen, onOpenChange }) => {
    return (
        <Dialog open={isOpen} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-md bg-zinc-900 border-zinc-800">
                <DialogHeader>
                    <DialogTitle className="text-2xl font-bold">Winerr.NET WebUI</DialogTitle>
                    <DialogDescription>
                        Version {appVersion}
                    </DialogDescription>
                </DialogHeader>
                <div className="mt-4 space-y-4 text-zinc-300">
                    <p>
                        A powerful web-based editor for Winerr.NET.
                    </p>
                    <p>
                        Created by <a href="https://github.com/DimaYastrebov" target="_blank" rel="noopener noreferrer" className="text-blue-400 hover:underline">DimaYastrebov</a>.
                    </p>
                </div>
                <div className="mt-6 flex justify-end">
                    <a href="https://github.com/DimaYastrebov/Winerr.NET" target="_blank" rel="noopener noreferrer">
                        <Button variant="outline">
                            <Github className="mr-2 h-4 w-4" />
                            GitHub Repository
                        </Button>
                    </a>
                </div>
            </DialogContent>
        </Dialog>
    );
};