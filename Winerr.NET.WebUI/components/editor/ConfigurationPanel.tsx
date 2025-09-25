"use client";

import React, { useRef, useState } from "react";
import { XCircle, Anvil, PlusCircle, Trash2, ChevronDown, Upload, Download as DownloadIcon, Copy } from "lucide-react";
import { type DragEndEvent } from "@dnd-kit/core";
import * as AccordionPrimitive from "@radix-ui/react-accordion";

import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Accordion, AccordionContent, AccordionItem } from "@/components/ui/accordion";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Slider } from "@/components/ui/slider";

import { ErrorConfig, ArchiveFormat } from "@/app/page";
import { IconPickerDialog } from "./IconPickerDialog";
import { ButtonDialog, ButtonConfig } from "./ButtonConstructor";
import { ErrorInstance } from "./ErrorInstance";

interface SystemStyle { id: string; display_name: string; }

interface ConfigurationPanelProps {
    styles: SystemStyle[];
    isLoading: boolean;
    isGenerating: boolean;
    onGenerate: () => void;
    onCopy: () => void;
    onDownload: () => void;
    isImageReady: boolean;
    onExport: () => void;
    onImport: (event: React.ChangeEvent<HTMLInputElement>) => void;
    mode: 'single' | 'batch';
    setMode: (mode: 'single' | 'batch') => void;
    errorInstances: ErrorConfig[];
    updateInstance: (id: string, newConfig: Partial<ErrorConfig['config']>) => void;
    updateInstanceName: (id: string, newName: string) => void;
    addInstance: () => void;
    deleteInstance: (id: string) => void;
    onDragEnd: (instanceId: string, event: DragEndEvent) => void;
    batchSettings: { format: ArchiveFormat, compression: number };
    setBatchSettings: (settings: { format: ArchiveFormat, compression: number }) => void;
}

export const ConfigurationPanel: React.FC<ConfigurationPanelProps> = ({
    styles, isLoading, isGenerating, onGenerate, onCopy, onDownload, isImageReady, onExport, onImport,
    mode, setMode, errorInstances, updateInstance, updateInstanceName, addInstance, deleteInstance, onDragEnd,
    batchSettings, setBatchSettings
}) => {
    const [activeDialogs, setActiveDialogs] = useState({
        button: { isOpen: false, instanceId: '', buttonData: null as ButtonConfig | null },
        icon: { isOpen: false, instanceId: '' },
    });
    const importInputRef = useRef<HTMLInputElement>(null);

    const openIconPicker = (instanceId: string) => setActiveDialogs(prev => ({ ...prev, icon: { isOpen: true, instanceId } }));
    const openButtonDialog = (instanceId: string, buttonData: ButtonConfig | null) => setActiveDialogs(prev => ({ ...prev, button: { isOpen: true, instanceId, buttonData } }));

    const handleSaveButton = (buttonData: ButtonConfig) => {
        const { instanceId } = activeDialogs.button;
        const instance = errorInstances.find(i => i.id === instanceId);
        if (!instance) return;
        const existingIndex = instance.config.buttons.findIndex(b => b.id === buttonData.id);
        const newButtons = [...instance.config.buttons];
        if (existingIndex > -1) newButtons[existingIndex] = buttonData;
        else newButtons.push(buttonData);
        updateInstance(instanceId, { buttons: newButtons });
    };

    const handleDeleteButton = (instanceId: string, buttonId: string) => {
        const instance = errorInstances.find(i => i.id === instanceId);
        if (!instance) return;
        const newButtons = instance.config.buttons.filter(b => b.id !== buttonId);
        updateInstance(instanceId, { buttons: newButtons });
    };

    const activeInstanceForDialog = errorInstances.find(i => i.id === activeDialogs.icon.instanceId || i.id === activeDialogs.button.instanceId);

    return (
        <div className="h-full bg-zinc-900 border-r border-zinc-800 p-4 flex flex-col">
            <div className="flex-shrink-0">
                <div className="flex items-center justify-between gap-2">
                    <div className="flex items-center gap-2 flex-shrink-0"><XCircle className="h-6 w-6 text-red-500" /><h2 className="text-lg font-semibold text-zinc-200">Configuration</h2></div>
                    <div className="flex items-center gap-2">
                        {isLoading ? (<Skeleton className="h-9 w-28" />) : (
                            <Select value={mode} onValueChange={(v: 'single' | 'batch') => setMode(v)}>
                                <SelectTrigger className="w-[110px] bg-zinc-800 border-zinc-700"><SelectValue /></SelectTrigger>
                                <SelectContent><SelectItem value="single">Single</SelectItem><SelectItem value="batch">Batch</SelectItem></SelectContent>
                            </Select>
                        )}
                    </div>
                </div>
                <Separator className="bg-zinc-800 mt-4" />
            </div>

            <div className="flex-1 min-h-0 mt-4">
                <ScrollArea className="h-full -mr-4 pr-4">
                    {mode === 'batch' && (
                        <Accordion type="single" collapsible className="w-full mb-2" defaultValue="batch-settings">
                            <AccordionItem value="batch-settings">
                                <AccordionPrimitive.Header>
                                    <AccordionPrimitive.Trigger className="text-base font-semibold text-zinc-300 p-2 rounded-md hover:bg-zinc-800 flex justify-between items-center w-full">
                                        Batch Settings
                                        <ChevronDown className="h-4 w-4 text-zinc-400 transition-transform [&[data-state=open]]:rotate-180" />
                                    </AccordionPrimitive.Trigger>
                                </AccordionPrimitive.Header>
                                <AccordionContent className="pt-4 space-y-4">
                                    <div className="grid w-full items-center gap-1.5">
                                        <Label>Archive Format</Label>
                                        <Select value={batchSettings.format} onValueChange={(v: ArchiveFormat) => setBatchSettings({ ...batchSettings, format: v })}>
                                            <SelectTrigger className="bg-zinc-800 border-zinc-700"><SelectValue /></SelectTrigger>
                                            <SelectContent><SelectItem value="zip">ZIP</SelectItem><SelectItem value="tar">TAR</SelectItem></SelectContent>
                                        </Select>
                                    </div>
                                    <div className="grid w-full items-center gap-1.5">
                                        <Label>Compression Level ({batchSettings.compression})</Label>
                                        <Slider value={[batchSettings.compression]} onValueChange={(v: number[]) => setBatchSettings({ ...batchSettings, compression: v[0] })} max={9} step={1} />
                                    </div>
                                </AccordionContent>
                            </AccordionItem>
                        </Accordion>
                    )}

                    {mode === 'single' && errorInstances[0] && (
                        <ErrorInstance instance={errorInstances[0]} styles={styles} isLoading={isLoading} onConfigChange={(newConfig) => updateInstance(errorInstances[0].id, newConfig)} onOpenIconPicker={() => openIconPicker(errorInstances[0].id)} onAddNewButton={() => openButtonDialog(errorInstances[0].id, null)} onEditButton={(btn) => openButtonDialog(errorInstances[0].id, btn)} onDeleteButton={(btnId) => handleDeleteButton(errorInstances[0].id, btnId)} onDragEnd={(e) => onDragEnd(errorInstances[0].id, e)} />
                    )}
                    {mode === 'batch' && (
                        <div className="space-y-2">
                            <Accordion type="multiple" className="w-full space-y-2">
                                {errorInstances.map(instance => (
                                    <AccordionItem key={instance.id} value={instance.id}>
                                        <AccordionPrimitive.Header className="flex">
                                            <div className="flex items-center justify-between w-full p-2 rounded-md hover:bg-zinc-800">
                                                <Input value={instance.name} onChange={(e) => updateInstanceName(instance.id, e.target.value)} className="bg-transparent border-none focus-visible:ring-1 focus-visible:ring-zinc-600 h-8 p-1 flex-1" />
                                                <Button variant="ghost" size="icon" className="h-7 w-7 flex-shrink-0" onClick={() => deleteInstance(instance.id)}><Trash2 className="h-4 w-4 text-red-500/80" /></Button>
                                                <AccordionPrimitive.Trigger className="p-1 rounded-sm focus-visible:ring-1 focus-visible:ring-zinc-600">
                                                    <div className="flex items-center justify-center w-full h-full">
                                                        <ChevronDown className="h-4 w-4 text-zinc-400 transition-transform [&[data-state=open]]:rotate-180" />
                                                    </div>
                                                </AccordionPrimitive.Trigger>
                                            </div>
                                        </AccordionPrimitive.Header>
                                        <AccordionContent className="pl-2">
                                            <ErrorInstance instance={instance} styles={styles} isLoading={isLoading} onConfigChange={(newConfig) => updateInstance(instance.id, newConfig)} onOpenIconPicker={() => openIconPicker(instance.id)} onAddNewButton={() => openButtonDialog(instance.id, null)} onEditButton={(btn) => openButtonDialog(instance.id, btn)} onDeleteButton={(btnId) => handleDeleteButton(instance.id, btnId)} onDragEnd={(e) => onDragEnd(instance.id, e)} />
                                        </AccordionContent>
                                    </AccordionItem>
                                ))}
                            </Accordion>
                            <Button variant="outline" className="w-full border-dashed" onClick={addInstance} disabled={isLoading}><PlusCircle className="mr-2 h-4 w-4" /> Add New Error</Button>
                        </div>
                    )}
                </ScrollArea>
            </div>

            <div className="flex-shrink-0 pt-4">
                <Separator className="bg-zinc-800 mb-4" />
                <div className="flex flex-col gap-2">
                    <div className="flex flex-col gap-2">
                        <Button className="w-full" disabled={isLoading || isGenerating} onClick={onGenerate}>
                            <Anvil className="mr-2 h-4 w-4" />{isGenerating ? "Generating..." : (mode === 'batch' ? "Generate & Download" : "Generate")}
                        </Button>
                        {mode === 'single' && (
                            <div className="flex gap-2">
                                <Button variant="outline" className="flex-1" onClick={onCopy} disabled={isLoading || !isImageReady}>
                                    <Copy className="mr-2 h-4 w-4" /> Copy
                                </Button>
                                <Button variant="outline" className="flex-1" onClick={onDownload} disabled={isLoading || !isImageReady}>
                                    <DownloadIcon className="mr-2 h-4 w-4" /> Download
                                </Button>
                            </div>
                        )}
                    </div>
                    <div className="flex flex-wrap gap-2">
                        <input type="file" ref={importInputRef} onChange={onImport} accept=".json" className="hidden" />
                        <Button variant="outline" className="flex-1" onClick={() => importInputRef.current?.click()} disabled={isLoading}><Upload className="mr-2 h-4 w-4" /> Import</Button>
                        <Button variant="outline" className="flex-1" onClick={onExport} disabled={isLoading}><DownloadIcon className="mr-2 h-4 w-4" /> Export</Button>
                    </div>
                </div>
            </div>

            {activeInstanceForDialog && (
                <>
                    <ButtonDialog isOpen={activeDialogs.button.isOpen} onOpenChange={(isOpen) => setActiveDialogs(prev => ({ ...prev, button: { ...prev.button, isOpen } }))} onSave={handleSaveButton} buttonData={activeDialogs.button.buttonData} supportedButtonTypes={activeInstanceForDialog.config.supportedButtonTypes} />
                    <IconPickerDialog isOpen={activeDialogs.icon.isOpen} onOpenChange={(isOpen) => setActiveDialogs(prev => ({ ...prev, icon: { ...prev.icon, isOpen } }))} onSelect={(iconId) => updateInstance(activeDialogs.icon.instanceId, { iconId: iconId })} styleId={activeInstanceForDialog.config.styleId} />
                </>
            )}
        </div>
    );
};
