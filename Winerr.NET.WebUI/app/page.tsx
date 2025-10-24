"use client";

import React, { useEffect, useRef, useState } from "react";
import { useBreakpoint } from "@/hooks/use-breakpoint";
import { useEditorStore } from "@/stores/editor-store";
import { useGetStyles } from "@/api/queries";
import { useTranslation } from "react-i18next";

import { AboutDialog } from "@/components/editor/AboutDialog";
import { AppMenubar } from "@/components/editor/AppMenubar";
import { ResizablePanel, ResizablePanelGroup, ResizableHandle } from "@/components/ui/resizable";
import { ConfigurationPanel } from "@/components/editor/ConfigurationPanel";
import { PreviewPanel } from "@/components/editor/PreviewPanel";
import { ServerDownOverlay } from "@/components/editor/ServerDownOverlay";

const Home = () => {
    const { t } = useTranslation();
    const isMobile = useBreakpoint(850);
    const [isAboutDialogOpen, setIsAboutDialogOpen] = React.useState(false);
    const importInputRef = useRef<HTMLInputElement>(null);
    const [isClient, setIsClient] = useState(false);

    const { data: styles = [], isError: isServerDown, isLoading: isLoadingStyles } = useGetStyles();
    const { initialize, clearSession, importState } = useEditorStore();

    useEffect(() => {
        let restoredState = null;
        try {
            const savedStateJSON = localStorage.getItem('winerr-autosave');
            if (savedStateJSON) {
                restoredState = JSON.parse(savedStateJSON);
            }
        } catch (error) {
            console.error("Failed to parse state from local storage:", error);
            localStorage.removeItem('winerr-autosave');
        }
        if (styles.length > 0) {
            initialize(restoredState, styles, t);
        }
    }, [styles, initialize, t]);

    useEffect(() => {
        setIsClient(true);
    }, []);

    const handleImport = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (!file) return;
        const reader = new FileReader();
        reader.onload = (e) => {
            const content = e.target?.result;
            if (typeof content === 'string') {
                const importedData = JSON.parse(content);
                importState(importedData, styles, t);
            }
        };
        reader.readAsText(file);
        if (event.target) {
            event.target.value = '';
        }
    };

    const handleReload = () => window.location.reload();

    if (!isClient) {
        return null;
    }

    return (
        <main className="h-screen flex flex-col bg-zinc-950 text-white overflow-hidden">
            <AboutDialog isOpen={isAboutDialogOpen} onOpenChange={setIsAboutDialogOpen} />
            <ServerDownOverlay isOpen={isServerDown} onReload={handleReload} />

            {!isServerDown && (
                <>
                    <AppMenubar
                        onClearSession={() => clearSession(t)}
                        onOpenAbout={() => setIsAboutDialogOpen(true)}
                    />

                    <ResizablePanelGroup direction={isMobile ? "vertical" : "horizontal"} className="flex-1">
                        {isMobile ? (
                            <>
                                <ResizablePanel defaultSize={60}>
                                    <PreviewPanel />
                                </ResizablePanel>
                                <ResizableHandle withHandle />
                                <ResizablePanel defaultSize={40} minSize={25} maxSize={80}>
                                    <ConfigurationPanel
                                        styles={styles}
                                        isLoading={isLoadingStyles}
                                        onImport={handleImport}
                                        importInputRef={importInputRef}
                                    />
                                </ResizablePanel>
                            </>
                        ) : (
                            <>
                                <ResizablePanel defaultSize={27} minSize={25} maxSize={40}>
                                    <ConfigurationPanel
                                        styles={styles}
                                        isLoading={isLoadingStyles}
                                        onImport={handleImport}
                                        importInputRef={importInputRef}
                                    />
                                </ResizablePanel>
                                <ResizableHandle withHandle />
                                <ResizablePanel defaultSize={73}>
                                    <PreviewPanel />
                                </ResizablePanel>
                            </>
                        )}
                    </ResizablePanelGroup>
                </>
            )}
        </main>
    );
};

export default Home;