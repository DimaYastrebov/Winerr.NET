"use client";

import {
    Menubar,
    MenubarContent,
    MenubarItem,
    MenubarMenu,
    MenubarTrigger,
} from "@/components/ui/menubar";

interface AppMenubarProps {
    onClearSession: () => void;
    onOpenAbout: () => void;
}

export function AppMenubar({
    onClearSession,
    onOpenAbout,
}: AppMenubarProps) {
    return (
        <Menubar className="rounded-none border-0 border-b border-zinc-800 bg-black px-2 lg:px-4">
            <MenubarMenu>
                <MenubarTrigger>File</MenubarTrigger>
                <MenubarContent>
                    <MenubarItem onClick={onClearSession} className="text-red-400 focus:bg-red-500/20 focus:text-red-300">
                        Clear Session & Reload
                    </MenubarItem>
                </MenubarContent>
            </MenubarMenu>
            <MenubarMenu>
                <MenubarTrigger>Help</MenubarTrigger>
                <MenubarContent>
                    <MenubarItem asChild>
                        <a href="https://github.com/DimaYastrebov/Winerr.NET/wiki" target="_blank" rel="noopener noreferrer">
                            Documentation
                        </a>
                    </MenubarItem>
                    <MenubarItem onClick={onOpenAbout}>
                        About Winerr.NET
                    </MenubarItem>
                </MenubarContent>
            </MenubarMenu>
        </Menubar>
    );
}