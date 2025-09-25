"use client";

import React, { useEffect, useState, useId, useCallback, useMemo } from "react";
import { arrayMove } from "@dnd-kit/sortable";
import { type DragEndEvent } from "@dnd-kit/core";
import { toast } from "sonner";

import { ResizablePanel, ResizablePanelGroup, ResizableHandle } from "@/components/ui/resizable";
import { ConfigurationPanel } from "@/components/editor/ConfigurationPanel";
import { PreviewPanel } from "@/components/editor/PreviewPanel";
import { useBreakpoint } from "@/hooks/use-breakpoint";
import { ButtonConfig } from "@/components/editor/ButtonConstructor";
import { ServerDownOverlay } from "../components/editor/ServerDownOverlay";
interface SystemStyle { id: string; display_name: string; }
export type ButtonAlignment = 'Auto' | 'Left' | 'Center' | 'Right';
export type ArchiveFormat = 'zip' | 'tar';

export interface ErrorInstanceConfig {
    styleId: string;
    title: string;
    content: string;
    iconId: string;
    isCrossEnabled: boolean;
    buttons: ButtonConfig[];
    buttonAlignment: ButtonAlignment;
    isButtonSortEnabled: boolean;
    maxWidth: string;
}

export interface ErrorConfig {
    id: string;
    name: string;
    config: ErrorInstanceConfig & {
        maxIconId: number;
        supportedButtonTypes: string[];
    };
}

interface GenerateRequestBody {
    style_id: string;
    title: string;
    content: string;
    icon_id: number;
    is_cross_enabled: boolean;
    sort_buttons: boolean;
    buttons: Omit<ButtonConfig, 'id'>[];
    button_alignment?: ButtonAlignment;
    max_width?: number;
}

interface StyleDetailsData {
    max_icon_id: number;
    metrics: {
        supported_button_types: string[];
    };
}

interface ImportedInstance {
    id: string;
    name: string;
    config: Partial<ErrorInstanceConfig>;
}

const Home = () => {
    const isMobile = useBreakpoint(850);
    const stableId = useId();

    const createNewErrorInstance = useCallback((name: string, styleId: string): ErrorConfig => ({
        id: `${stableId}-${Math.random().toString(36).slice(2)}`,
        name: name,
        config: {
            styleId: styleId,
            maxIconId: 0,
            supportedButtonTypes: [],
            title: "",
            content: "",
            iconId: "0",
            isCrossEnabled: true,
            buttons: [],
            buttonAlignment: 'Auto',
            isButtonSortEnabled: true,
            maxWidth: ""
        }
    }), [stableId]);

    const [isServerDown, setIsServerDown] = useState(false);
    const [styles, setStyles] = useState<SystemStyle[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isDetailsLoading, setIsDetailsLoading] = useState(false);
    const [isGenerating, setIsGenerating] = useState(false);
    const [generatedImageUrl, setGeneratedImageUrl] = useState<string | null>(null);
    const [generatedImageBlob, setGeneratedImageBlob] = useState<Blob | null>(null);
    const [mode, setMode] = useState<'single' | 'batch'>('single');
    const [errorInstances, setErrorInstances] = useState<ErrorConfig[]>([]);
    const [batchSettings, setBatchSettings] = useState({ format: 'zip' as ArchiveFormat, compression: 6 });

    const styleIdsKey = useMemo(() => {
        return errorInstances.map(i => i.config.styleId).join(',');
    }, [errorInstances]);

    const fetchAllDetails = useCallback(async (instances: ErrorConfig[]) => {
        if (instances.length === 0) return;

        setIsDetailsLoading(true);
        const uniqueStyleIds = [...new Set(instances.map(i => i.config.styleId).filter(Boolean))];
        const detailsCache = new Map<string, StyleDetailsData>();

        await Promise.all(uniqueStyleIds.map(async (styleId) => {
            try {
                const response = await fetch(`/v1/styles/${styleId}`);
                if (response.ok) {
                    const data = await response.json();
                    detailsCache.set(styleId, data.data);
                }
            } catch (error) {
                console.error(`Failed to fetch details for ${styleId}:`, error);
                toast.error(`Failed to load details for ${styleId}`);
            }
        }));

        setErrorInstances(prev => {
            const updatedInstances = prev.map(instance => {
                const details = detailsCache.get(instance.config.styleId);
                if (details) {
                    const newConfig = { ...instance.config };
                    newConfig.maxIconId = details.max_icon_id;
                    newConfig.supportedButtonTypes = details.metrics.supported_button_types || [];
                    if (parseInt(newConfig.iconId, 10) > newConfig.maxIconId) {
                        newConfig.iconId = newConfig.maxIconId.toString();
                    }
                    return { ...instance, config: newConfig };
                }
                return instance;
            });
            return updatedInstances;
        });
        setIsDetailsLoading(false);
    }, []);

    useEffect(() => {
        setErrorInstances([createNewErrorInstance("Default Error", "")]);
    }, [stableId, createNewErrorInstance]);

    useEffect(() => {
        const fetchInitialData = async () => {
            setIsLoading(true);
            try {
                const stylesResponse = await fetch("/v1/styles");
                if (!stylesResponse.ok) throw new Error("Failed to fetch styles");
                const stylesData = await stylesResponse.json();
                if (stylesData?.data?.length > 0) {
                    const firstStyleId = stylesData.data[0].id;
                    setStyles(stylesData.data);
                    setErrorInstances(prev => prev.map(inst => ({ ...inst, config: { ...inst.config, styleId: firstStyleId } })));
                }
            } catch (error) {
                console.error("Failed to fetch initial data:", error);
                setIsServerDown(true);
            }
            finally { setIsLoading(false); }
        };
        fetchInitialData();
    }, []);

    useEffect(() => {
        if (isLoading) return;
        if (styleIdsKey.split(',').every((id: string) => !id)) return;
        fetchAllDetails(errorInstances);
    }, [styleIdsKey, isLoading, fetchAllDetails, errorInstances]);

    const handleReload = () => {
        window.location.reload();
    };

    const updateInstance = (
        id: string,
        configUpdate: Partial<ErrorConfig['config']> | ((currentConfig: ErrorConfig['config']) => Partial<ErrorConfig['config']>)
    ) => {
        setErrorInstances(prev =>
            prev.map(inst => {
                if (inst.id === id) {
                    const newConfig = typeof configUpdate === 'function'
                        ? { ...inst.config, ...configUpdate(inst.config) }
                        : { ...inst.config, ...configUpdate };
                    return { ...inst, config: newConfig };
                }
                return inst;
            })
        );
    };

    const updateInstanceName = (id: string, newName: string) => {
        setErrorInstances(prev => prev.map(inst => inst.id === id ? { ...inst, name: newName } : inst));
    };

    const addInstance = () => {
        const styleId = styles.length > 0 ? styles[0].id : "";
        const newInstance = createNewErrorInstance(`Error #${errorInstances.length + 1}`, styleId);
        setErrorInstances(prev => [...prev, newInstance]);
    };

    const deleteInstance = (id: string) => {
        setErrorInstances(prev => {
            const newInstances = prev.filter(inst => inst.id !== id);
            if (newInstances.length > 0) return newInstances;
            const styleId = styles.length > 0 ? styles[0].id : "";
            return [createNewErrorInstance("Default Error", styleId)];
        });
    };

    const handleDragEnd = (instanceId: string, event: DragEndEvent) => {
        const { active, over } = event;
        if (over && active.id !== over.id) {
            updateInstance(instanceId, currentConfig => {
                const oldIndex = currentConfig.buttons.findIndex(b => b.id === active.id);
                const newIndex = currentConfig.buttons.findIndex(b => b.id === over.id);
                return {
                    buttons: arrayMove(currentConfig.buttons, oldIndex, newIndex)
                };
            });
        }
    };

    const handleGenerate = useCallback(async () => {
        setIsGenerating(true);
        setGeneratedImageUrl(null);
        setGeneratedImageBlob(null);
        try {
            if (mode === 'single' && errorInstances[0]) {
                const { config } = errorInstances[0];
                const requestBody: GenerateRequestBody = {
                    style_id: config.styleId, title: config.title, content: config.content,
                    icon_id: parseInt(config.iconId, 10) || 0, is_cross_enabled: config.isCrossEnabled,
                    sort_buttons: config.isButtonSortEnabled, buttons: config.buttons.map(({ id, ...rest }) => rest),
                };
                if (config.buttonAlignment !== 'Auto') requestBody.button_alignment = config.buttonAlignment;
                if (config.maxWidth && !isNaN(parseInt(config.maxWidth, 10))) requestBody.max_width = parseInt(config.maxWidth, 10);
                const response = await fetch("/v1/images/generate", {
                    method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(requestBody),
                });
                if (!response.ok) throw new Error(`Generation failed: ${response.statusText}`);
                const imageBlob = await response.blob();
                setGeneratedImageBlob(imageBlob);
                setGeneratedImageUrl(URL.createObjectURL(imageBlob));
            } else {
                const batchRequests = errorInstances.map(instance => {
                    const { config } = instance;
                    const requestBody: GenerateRequestBody = {
                        style_id: config.styleId, title: config.title, content: config.content,
                        icon_id: parseInt(config.iconId, 10) || 0, is_cross_enabled: config.isCrossEnabled,
                        sort_buttons: config.isButtonSortEnabled, buttons: config.buttons.map(({ id: _id, ...rest }) => rest),
                    };
                    if (config.buttonAlignment !== 'Auto') requestBody.button_alignment = config.buttonAlignment;
                    if (config.maxWidth && !isNaN(parseInt(config.maxWidth, 10))) requestBody.max_width = parseInt(config.maxWidth, 10);
                    return requestBody;
                });
                const response = await fetch("/v1/images/generate/batch", {
                    method: "POST", headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ requests: batchRequests, archive_format: batchSettings.format, compression_level: batchSettings.compression }),
                });
                if (!response.ok) throw new Error(`Batch generation failed: ${response.statusText}`);
                const archiveBlob = await response.blob();
                const url = URL.createObjectURL(archiveBlob);
                const link = document.createElement('a');
                link.href = url;
                link.download = `winerr_batch_${Date.now()}.${batchSettings.format}`;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                URL.revokeObjectURL(url);
            }
        } catch (error) {
            console.error("Error generating image(s):", error);
            toast.error("Image Generation Failed", { description: "An error occurred while communicating with the server. Please check the server logs." });
        }
        finally { setIsGenerating(false); }
    }, [errorInstances, mode, batchSettings])

    const handleCopy = async () => {
        if (!generatedImageBlob) {
            toast.error("Nothing to copy", { description: "Generate an image first." });
            return;
        }
        try {
            await navigator.clipboard.write([
                new ClipboardItem({ 'image/png': generatedImageBlob })
            ]);
            toast.success("Image copied to clipboard!");
        } catch (error) {
            console.error("Failed to copy image to clipboard:", error);
            toast.error("Copy failed", { description: "Your browser might not support this feature or did not grant permission." });
        }
    };

    const handleDownload = () => {
        if (!generatedImageUrl || !generatedImageBlob) {
            toast.error("Nothing to download", { description: "Generate an image first." });
            return;
        }
        const link = document.createElement('a');
        link.href = generatedImageUrl;
        link.download = `winerr_net_${Date.now()}.png`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    };

    const handleExport = () => {
        const dataToExport = {
            batchSettings: mode === 'batch' ? batchSettings : undefined,
            instances: errorInstances.map(({ id, name, config }) => {
                const { maxIconId, supportedButtonTypes, ...userConfig } = config;
                return { id, name, config: userConfig };
            })
        };
        const dataStr = JSON.stringify(dataToExport, null, 2);
        const dataBlob = new Blob([dataStr], { type: "application/json" });
        const url = URL.createObjectURL(dataBlob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `winerr_config_${Date.now()}.json`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    };

    const handleImport = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (!file) return;
        const reader = new FileReader();
        reader.onload = (e) => {
            try {
                const content = e.target?.result;
                if (typeof content === 'string') {
                    const importedData = JSON.parse(content);
                    if (Array.isArray(importedData.instances)) {
                        const styleId = styles.length > 0 ? styles[0].id : "";
                        const validatedInstances = importedData.instances.map((item: ImportedInstance) => ({
                            ...createNewErrorInstance(item.name || "Imported Error", item.config?.styleId || styleId),
                            ...item,
                        }));
                        setErrorInstances(validatedInstances);
                        fetchAllDetails(validatedInstances);
                        if (importedData.batchSettings) {
                            setBatchSettings(importedData.batchSettings);
                        }
                        if (validatedInstances.length > 1) {
                            setMode('batch');
                        } else {
                            setMode('single');
                        }
                        toast.success("Configuration imported successfully!");
                    } else {
                        throw new Error("Invalid config format: 'instances' array not found.");
                    }
                }
            } catch (error) {
                console.error("Failed to import file:", error);
                toast.error("Failed to import configuration", { description: "The selected file is either not a valid JSON or has an incorrect structure." });
            }
        };
        reader.readAsText(file);
        event.target.value = '';
    };

    return (
        <main className="h-screen p-4 bg-zinc-950 text-white">
            <ServerDownOverlay isOpen={isServerDown} onReload={handleReload} />

            {!isServerDown && (
                <ResizablePanelGroup direction={isMobile ? "vertical" : "horizontal"} className="h-full w-full max-w-7xl mx-auto bg-zinc-900 rounded-xl shadow-lg border border-zinc-800">
                    {isMobile ? (
                        <>
                            <ResizablePanel defaultSize={60}><PreviewPanel imageUrl={generatedImageUrl} isGenerating={isGenerating} mode={mode} /></ResizablePanel>
                            <ResizableHandle withHandle />
                            <ResizablePanel defaultSize={40} minSize={25} maxSize={80}><ConfigurationPanel styles={styles} isLoading={isLoading || isDetailsLoading} isGenerating={isGenerating} onGenerate={handleGenerate} onCopy={handleCopy} onDownload={handleDownload} isImageReady={!!generatedImageUrl} onExport={handleExport} onImport={handleImport} mode={mode} setMode={setMode} errorInstances={errorInstances} updateInstance={updateInstance} updateInstanceName={updateInstanceName} addInstance={addInstance} deleteInstance={deleteInstance} onDragEnd={handleDragEnd} batchSettings={batchSettings} setBatchSettings={setBatchSettings} /></ResizablePanel>
                        </>
                    ) : (
                        <>
                            <ResizablePanel defaultSize={27} minSize={25} maxSize={40}><ConfigurationPanel styles={styles} isLoading={isLoading || isDetailsLoading} isGenerating={isGenerating} onGenerate={handleGenerate} onCopy={handleCopy} onDownload={handleDownload} isImageReady={!!generatedImageUrl} onExport={handleExport} onImport={handleImport} mode={mode} setMode={setMode} errorInstances={errorInstances} updateInstance={updateInstance} updateInstanceName={updateInstanceName} addInstance={addInstance} deleteInstance={deleteInstance} onDragEnd={handleDragEnd} batchSettings={batchSettings} setBatchSettings={setBatchSettings} /></ResizablePanel>
                            <ResizableHandle withHandle />
                            <ResizablePanel defaultSize={73}><PreviewPanel imageUrl={generatedImageUrl} isGenerating={isGenerating} mode={mode} /></ResizablePanel>
                        </>
                    )}
                </ResizablePanelGroup>
            )}
        </main>
    );
};

export default Home;
