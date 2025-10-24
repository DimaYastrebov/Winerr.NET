"use client";

import React from "react";
import { useTranslation, Trans } from "react-i18next";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { Button } from "../ui/button";
import { Github } from "lucide-react";
import { version as appVersion } from '../../package.json';

interface AboutDialogProps {
    isOpen: boolean;
    onOpenChange: (isOpen: boolean) => void;
}

export const AboutDialog: React.FC<AboutDialogProps> = ({ isOpen, onOpenChange }) => {
    const { t } = useTranslation();

    return (
        <Dialog open={isOpen} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-md bg-zinc-900 border-zinc-800">
                <DialogHeader>
                    <DialogTitle className="text-2xl font-bold">{t('about_dialog.title')}</DialogTitle>
                    <DialogDescription>
                        {t('about_dialog.version')} {appVersion}
                    </DialogDescription>
                </DialogHeader>
                <div className="mt-4 space-y-4 text-zinc-300">
                    <p>
                        {t('about_dialog.description')}
                    </p>
                    <p>
                        <Trans i18nKey="about_dialog.created_by">
                            Created by <a href="https://github.com/DimaYastrebov" target="_blank" rel="noopener noreferrer" className="text-blue-400 hover:underline">DimaYastrebov</a>.
                        </Trans>
                    </p>
                </div>
                <div className="mt-6 flex justify-end">
                    <a href="https://github.com/DimaYastrebov/Winerr.NET" target="_blank" rel="noopener noreferrer">
                        <Button variant="outline">
                            <Github className="mr-2 h-4 w-4" />
                            {t('about_dialog.github_repo')}
                        </Button>
                    </a>
                </div>
            </DialogContent>
        </Dialog>
    );
};