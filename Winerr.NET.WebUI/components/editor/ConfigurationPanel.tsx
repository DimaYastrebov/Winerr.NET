"use client";

import React, { useState, useEffect, useCallback } from "react";
import { DndContext, closestCenter, PointerSensor, KeyboardSensor, useSensor, useSensors } from "@dnd-kit/core";
import { SortableContext, useSortable, verticalListSortingStrategy } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { XCircle, Anvil, PlusCircle, Trash2, ChevronDown, Upload, Download as DownloadIcon, Copy, GripVertical } from "lucide-react";
import * as AccordionPrimitive from "@radix-ui/react-accordion";
import { toast } from "sonner";
import { useTranslation } from "react-i18next";

import { useEditorStore } from "@/stores/editor-store";
import { useGenerateImage, useGenerateBatch, useGetStyleDetails } from "@/api/queries";

import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Accordion, AccordionContent, AccordionItem } from "@/components/ui/accordion";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Slider } from "@/components/ui/slider";

import { ErrorConfig, ArchiveFormat, GenerateRequestBody, SystemStyle } from "@/lib/types";
import { IconPickerDialog } from "./IconPickerDialog";
import { ButtonDialog, ButtonConfig } from "./ButtonConstructor";
import { ErrorInstance } from "./ErrorInstance";

interface SortableInstanceItemProps {
    instance: ErrorConfig;
    index: number;
    styles: SystemStyle[];
    isLoading: boolean;
    updateInstance: (id: string, newConfig: Partial<ErrorConfig['config']>) => void;
    updateInstanceName: (id: string, newName: string) => void;
    deleteInstance: (id: string) => void;
    openIconPicker: (instanceId: string) => void;
    openButtonDialog: (instanceId: string, buttonData: ButtonConfig | null) => void;
    handleDeleteButton: (instanceId: string, buttonId: string) => void;
}

const SortableInstanceItem: React.FC<SortableInstanceItemProps> = (props) => {
    const { instance, index } = props;
    const { attributes, listeners, setNodeRef, transform, transition } = useSortable({ id: instance.id });
    const style = { transform: CSS.Transform.toString(transform), transition };

    return (
        <div ref={setNodeRef} style={style} {...attributes}>
            <AccordionItem value={instance.id}>
                <AccordionPrimitive.Header className="flex">
                    <div className="flex items-center justify-between w-full p-2 rounded-md hover:bg-zinc-800">
                        <div {...listeners} className="cursor-grab touch-none p-1 flex items-center gap-2">
                            <GripVertical className="h-4 w-4 text-zinc-500" />
                            <span className="text-sm font-medium text-zinc-400">{index + 1}.</span>
                        </div>
                        <Input value={instance.name} onChange={(e) => props.updateInstanceName(instance.id, e.target.value)} className="bg-transparent border-none focus-visible:ring-1 focus-visible:ring-zinc-600 h-8 p-1 flex-1 mx-2" />
                        <Button variant="ghost" size="icon" className="h-7 w-7 flex-shrink-0" onClick={() => props.deleteInstance(instance.id)}><Trash2 className="h-4 w-4 text-red-500/80" /></Button>
                        <AccordionPrimitive.Trigger className="p-1 rounded-sm focus-visible:ring-1 focus-visible:ring-zinc-600">
                            <div className="flex items-center justify-center w-full h-full">
                                <ChevronDown className="h-4 w-4 text-zinc-400 transition-transform [&[data-state=open]]:rotate-180" />
                            </div>
                        </AccordionPrimitive.Trigger>
                    </div>
                </AccordionPrimitive.Header>
                <AccordionContent>
                    <ErrorInstance
                        instance={instance}
                        styles={props.styles}
                        isLoading={props.isLoading}
                        onConfigChange={(newConfig) => props.updateInstance(instance.id, newConfig)}
                        onOpenIconPicker={() => props.openIconPicker(instance.id)}
                        onAddNewButton={() => props.openButtonDialog(instance.id, null)}
                        onEditButton={(btn) => props.openButtonDialog(instance.id, btn)}
                        onDeleteButton={(btnId) => props.handleDeleteButton(instance.id, btnId)}
                    />
                </AccordionContent>
            </AccordionItem>
        </div>
    );
};

interface ConfigurationPanelProps {
    styles: SystemStyle[];
    isLoading: boolean;
    onImport: (event: React.ChangeEvent<HTMLInputElement>) => void;
    importInputRef: React.RefObject<HTMLInputElement | null>;
}

const ConfigurationPanelFC: React.FC<ConfigurationPanelProps> = ({
    styles, isLoading, onImport, importInputRef
}) => {
    const { t } = useTranslation();
    const {
        mode, setMode, errorInstances, batchSettings, setBatchSettings,
        updateInstance, updateInstanceName, addInstance,
        deleteInstance: deleteStoreInstance, handleInstancesDragEnd,
        generatedImageBlob, generatedImageUrl
    } = useEditorStore();

    const generateImageMutation = useGenerateImage(t);
    const generateBatchMutation = useGenerateBatch(t);
    const isGenerating = generateImageMutation.isPending || generateBatchMutation.isPending;

    const [activeDialogs, setActiveDialogs] = useState({
        button: { isOpen: false, instanceId: '', buttonData: null as ButtonConfig | null },
        icon: { isOpen: false, instanceId: '' },
    });
    const sensors = useSensors(useSensor(PointerSensor), useSensor(KeyboardSensor));

    const activeStyleId = errorInstances.find(inst => inst.id === activeDialogs.icon.instanceId)?.config.styleId ?? '';
    const { data: styleDetails, isLoading: isDetailsLoading } = useGetStyleDetails(activeStyleId);

    useEffect(() => {
        if (styleDetails) {
            updateInstance(activeDialogs.icon.instanceId, {
                maxIconId: styleDetails.max_icon_id,
                supportedButtonTypes: styleDetails.metrics.supported_button_types,
            });
        }
    }, [styleDetails, activeDialogs.icon.instanceId, updateInstance]);

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

    const handleDeleteInstance = (id: string) => {
        deleteStoreInstance(id, styles);
    };

    const handleAddInstance = () => {
        addInstance(styles);
    };
    
    const handleGenerate = useCallback(() => {
        if (mode === 'single' && errorInstances[0]) {
            const { config } = errorInstances[0];
            const requestBody: GenerateRequestBody = {
                style_id: config.styleId, title: config.title, content: config.content,
                icon_id: parseInt(config.iconId, 10) || 0, is_cross_enabled: config.isCrossEnabled,
                sort_buttons: config.isButtonSortEnabled, buttons: config.buttons.map(({ id, ...rest }) => rest),
            };
            if (config.buttonAlignment !== 'Auto') requestBody.button_alignment = config.buttonAlignment;
            if (config.maxWidth && !isNaN(parseInt(config.maxWidth, 10))) requestBody.max_width = parseInt(config.maxWidth, 10);
            generateImageMutation.mutate(requestBody);
        } else if (mode === 'batch') {
            const batchRequests = errorInstances.map(instance => {
                const { config } = instance;
                const requestBody: GenerateRequestBody = {
                    style_id: config.styleId, title: config.title, content: config.content,
                    icon_id: parseInt(config.iconId, 10) || 0, is_cross_enabled: config.isCrossEnabled,
                    sort_buttons: config.isButtonSortEnabled, buttons: config.buttons.map(({ id, ...rest }) => rest),
                };
                if (config.buttonAlignment !== 'Auto') requestBody.button_alignment = config.buttonAlignment;
                if (config.maxWidth && !isNaN(parseInt(config.maxWidth, 10))) requestBody.max_width = parseInt(config.maxWidth, 10);
                return requestBody;
            });
            generateBatchMutation.mutate({ requests: batchRequests, format: batchSettings.format, compression: batchSettings.compression });
        }
    }, [mode, errorInstances, batchSettings, generateImageMutation, generateBatchMutation]);

    const handleCopy = useCallback(async () => {
        if (!generatedImageBlob) {
            toast.error(t('toasts.copy_fail_title'), { description: t('toasts.copy_fail_desc') });
            return;
        }
        try {
            await navigator.clipboard.write([new ClipboardItem({ 'image/png': generatedImageBlob })]);
            toast.success(t('toasts.copy_success'));
        } catch (error) {
            console.error("Failed to copy image:", error);
            toast.error(t('toasts.copy_unsupported_title'), { description: t('toasts.copy_unsupported_desc') });
        }
    }, [generatedImageBlob, t]);

    const handleDownload = useCallback(() => {
        if (!generatedImageUrl) {
            toast.error(t('toasts.download_fail_title'), { description: t('toasts.download_fail_desc') });
            return;
        }
        const link = document.createElement('a');
        link.href = generatedImageUrl;
        link.download = `winerr_net_${Date.now()}.png`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }, [generatedImageUrl, t]);

    const handleExport = useCallback(() => {
        const dataToExport = {
            mode,
            batchSettings: mode === 'batch' ? batchSettings : undefined,
            instances: errorInstances.map(({ id, name, config }) => {
                const { maxIconId, supportedButtonTypes, ...userConfig } = config;
                return { name, config: userConfig };
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
    }, [errorInstances, mode, batchSettings]);

    const activeInstanceForDialog = errorInstances.find(i => i.id === activeDialogs.icon.instanceId || i.id === activeDialogs.button.instanceId);

    return (
        <div className="h-full bg-zinc-900 border-r border-zinc-800 p-4 flex flex-col">
            <div className="flex-shrink-0">
                <div className="flex items-center justify-between gap-2">
                    <div className="flex items-center gap-2 flex-shrink-0"><XCircle className="h-6 w-6 text-red-500" /><h2 className="text-lg font-semibold text-zinc-200">{t('config_panel.title')}</h2></div>
                    <div className="flex items-center gap-2">
                        {isLoading ? (<Skeleton className="h-9 w-28" />) : (
                            <Select value={mode} onValueChange={(v: 'single' | 'batch') => setMode(v)}>
                                <SelectTrigger className="w-[110px] bg-zinc-800 border-zinc-700"><SelectValue /></SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="single">{t('config_panel.mode_single')}</SelectItem>
                                    <SelectItem value="batch">{t('config_panel.mode_batch')}</SelectItem>
                                </SelectContent>
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
                                        {t('config_panel.batch_settings_title')}
                                        <ChevronDown className="h-4 w-4 text-zinc-400 transition-transform [&[data-state=open]]:rotate-180" />
                                    </AccordionPrimitive.Trigger>
                                </AccordionPrimitive.Header>
                                <AccordionContent className="pt-4 space-y-4">
                                    <div className="grid w-full items-center gap-1.5">
                                        <Label>{t('config_panel.archive_format')}</Label>
                                        <Select value={batchSettings.format} onValueChange={(v: ArchiveFormat) => setBatchSettings({ ...batchSettings, format: v })}>
                                            <SelectTrigger className="bg-zinc-800 border-zinc-700"><SelectValue /></SelectTrigger>
                                            <SelectContent><SelectItem value="zip">ZIP</SelectItem><SelectItem value="tar">TAR</SelectItem></SelectContent>
                                        </Select>
                                    </div>
                                    <div className="grid w-full items-center gap-1.5">
                                        <Label>{t('config_panel.compression_level')} ({batchSettings.compression})</Label>
                                        <Slider value={[batchSettings.compression]} onValueChange={(v: number[]) => setBatchSettings({ ...batchSettings, compression: v[0] })} max={9} step={1} />
                                    </div>
                                </AccordionContent>
                            </AccordionItem>
                        </Accordion>
                    )}

                    {mode === 'single' && errorInstances[0] && (
                        <ErrorInstance instance={errorInstances[0]} styles={styles} isLoading={isLoading || isDetailsLoading} onConfigChange={(newConfig) => updateInstance(errorInstances[0].id, newConfig)} onOpenIconPicker={() => openIconPicker(errorInstances[0].id)} onAddNewButton={() => openButtonDialog(errorInstances[0].id, null)} onEditButton={(btn) => openButtonDialog(errorInstances[0].id, btn)} onDeleteButton={(btnId) => handleDeleteButton(errorInstances[0].id, btnId)} />
                    )}
                    {mode === 'batch' && (
                        <div className="space-y-2">
                            <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleInstancesDragEnd}>
                                <div className="overflow-hidden">
                                    <SortableContext items={errorInstances} strategy={verticalListSortingStrategy}>
                                        <Accordion type="multiple" className="w-full space-y-2">
                                            {errorInstances.map((instance, index) => (
                                                <SortableInstanceItem
                                                    key={instance.id}
                                                    instance={instance}
                                                    index={index}
                                                    styles={styles}
                                                    isLoading={isLoading || isDetailsLoading}
                                                    updateInstance={updateInstance}
                                                    updateInstanceName={updateInstanceName}
                                                    deleteInstance={handleDeleteInstance}
                                                    openIconPicker={openIconPicker}
                                                    openButtonDialog={openButtonDialog}
                                                    handleDeleteButton={handleDeleteButton}
                                                />
                                            ))}
                                        </Accordion>
                                    </SortableContext>
                                </div>
                            </DndContext>
                            <Button variant="outline" className="w-full border-dashed" onClick={handleAddInstance} disabled={isLoading}><PlusCircle className="mr-2 h-4 w-4" /> {t('config_panel.add_new_error')}</Button>
                        </div>
                    )}
                </ScrollArea>
            </div>

            <div className="flex-shrink-0 pt-4">
                <Separator className="bg-zinc-800 mb-4" />
                <div className="flex flex-col gap-2">
                    <div className="flex flex-col gap-2">
                        <Button className="w-full" disabled={isLoading || isGenerating} onClick={handleGenerate}>
                            <Anvil className="mr-2 h-4 w-4" />{isGenerating ? t('config_panel.generating') : (mode === 'batch' ? t('config_panel.generate_batch') : t('config_panel.generate_single'))}
                        </Button>
                        {mode === 'single' && (
                            <div className="flex gap-2">
                                <Button variant="outline" className="flex-1" onClick={handleCopy} disabled={isLoading || !generatedImageBlob}>
                                    <Copy className="mr-2 h-4 w-4" /> {t('config_panel.copy')}
                                </Button>
                                <Button variant="outline" className="flex-1" onClick={handleDownload} disabled={isLoading || !generatedImageBlob}>
                                    <DownloadIcon className="mr-2 h-4 w-4" /> {t('config_panel.download')}
                                </Button>
                            </div>
                        )}
                    </div>
                    <div className="flex flex-wrap gap-2">
                        <input type="file" ref={importInputRef} onChange={onImport} accept=".json" className="hidden" />
                        <Button variant="outline" className="flex-1" onClick={() => importInputRef.current?.click()} disabled={isLoading}><Upload className="mr-2 h-4 w-4" /> {t('config_panel.import')}</Button>
                        <Button variant="outline" className="flex-1" onClick={handleExport} disabled={isLoading}><DownloadIcon className="mr-2 h-4 w-4" /> {t('config_panel.export')}</Button>
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

export const ConfigurationPanel = React.memo(ConfigurationPanelFC);