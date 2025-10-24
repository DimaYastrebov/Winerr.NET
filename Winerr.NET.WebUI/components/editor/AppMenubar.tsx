"use client";

import { useTranslation } from "react-i18next";
import {
    Menubar,
    MenubarContent,
    MenubarItem,
    MenubarMenu,
    MenubarRadioGroup,
    MenubarRadioItem,
    MenubarSub,
    MenubarSubContent,
    MenubarSubTrigger,
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
    const { t, i18n } = useTranslation();

    const changeLanguage = (lng: string) => {
        i18n.changeLanguage(lng);
    };

    return (
        <Menubar className="rounded-none border-0 border-b border-zinc-800 bg-black px-2 lg:px-4">
            <MenubarMenu>
                <MenubarTrigger>{t('menubar.file')}</MenubarTrigger>
                <MenubarContent>
                    <MenubarItem onClick={onClearSession} className="text-red-400 focus:bg-red-500/20 focus:text-red-300">
                        {t('menubar.file_clear_session')}
                    </MenubarItem>
                </MenubarContent>
            </MenubarMenu>

            <MenubarMenu>
                <MenubarTrigger>{t('menubar.view')}</MenubarTrigger>
                <MenubarContent>
                    <MenubarSub>
                        <MenubarSubTrigger>{t('menubar.view_language')}</MenubarSubTrigger>
                        <MenubarSubContent>
                            <MenubarRadioGroup value={i18n.language}>
                                <MenubarRadioItem value="en" onClick={() => changeLanguage('en')}>
                                    English
                                </MenubarRadioItem>
                                <MenubarRadioItem value="uk" onClick={() => changeLanguage('uk')}>
                                    Українська
                                </MenubarRadioItem>
                            </MenubarRadioGroup>
                        </MenubarSubContent>
                    </MenubarSub>
                </MenubarContent>
            </MenubarMenu>

            <MenubarMenu>
                <MenubarTrigger>{t('menubar.help')}</MenubarTrigger>
                <MenubarContent>
                    <MenubarItem asChild>
                        <a href="https://github.com/DimaYastrebov/Winerr.NET/wiki" target="_blank" rel="noopener noreferrer">
                            {t('menubar.help_docs')}
                        </a>
                    </MenubarItem>
                    <MenubarItem onClick={onOpenAbout}>
                        {t('menubar.help_about')}
                    </MenubarItem>
                </MenubarContent>
            </MenubarMenu>
        </Menubar>
    );
}