"use client";

import React, { useEffect, useMemo, useState } from "react";
import { DndContext, closestCenter, type DragEndEvent, PointerSensor, KeyboardSensor, useSensor, useSensors } from "@dnd-kit/core";
import { SortableContext, verticalListSortingStrategy, arrayMove } from "@dnd-kit/sortable";
import { PlusCircle, Image as ImageIcon } from "lucide-react";

import { useGetStyleDetails } from "@/api/queries";

import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Skeleton } from "@/components/ui/skeleton";
import { Switch } from "@/components/ui/switch";
import { ButtonAlignment, ErrorConfig, SystemStyle } from "@/lib/types";
import { SortableButton, ButtonConfig } from "./ButtonConstructor";
import { FormField } from "./FormField";
import { Combobox, ComboboxOption } from "@/components/ui/combobox";
import { InfoPopover } from "./InfoPopover";

interface ErrorInstanceProps {
    instance: ErrorConfig;
    styles: SystemStyle[];
    isLoading: boolean;
    onConfigChange: (newConfig: Partial<ErrorConfig['config']>) => void;
    onOpenIconPicker: () => void;
    onAddNewButton: () => void;
    onEditButton: (button: ButtonConfig) => void;
    onDeleteButton: (buttonId: string) => void;
}

const ErrorInstanceFC: React.FC<ErrorInstanceProps> = ({
    instance, styles, isLoading, onConfigChange, onOpenIconPicker,
    onAddNewButton, onEditButton, onDeleteButton
}) => {
    const sensors = useSensors(useSensor(PointerSensor), useSensor(KeyboardSensor));
    const { config } = instance;

    const [selectedSystem, setSelectedSystem] = useState<string | null>(null);

    const groupedStyles = useMemo(() => {
        const map = new Map<string, SystemStyle[]>();
        styles.forEach(style => {
            const family = style.system_family;
            if (!map.has(family)) {
                map.set(family, []);
            }
            map.get(family)!.push(style);
        });
        return map;
    }, [styles]);

    useEffect(() => {
        if (config.styleId && styles.length > 0) {
            const currentStyle = styles.find(s => s.id === config.styleId);
            if (currentStyle && currentStyle.system_family !== selectedSystem) {
                setSelectedSystem(currentStyle.system_family);
            }
        }
    }, [config.styleId, styles, selectedSystem]);


    const { data: styleDetails, isLoading: isDetailsLoading } = useGetStyleDetails(config.styleId);

    useEffect(() => {
        if (styleDetails) {
            const updates: Partial<ErrorConfig['config']> = {};
            let needsUpdate = false;

            if (instance.config.maxIconId !== styleDetails.max_icon_id) {
                updates.maxIconId = styleDetails.max_icon_id;
                const currentIconId = parseInt(instance.config.iconId, 10);
                if (currentIconId > styleDetails.max_icon_id) {
                    updates.iconId = styleDetails.max_icon_id.toString();
                }
                needsUpdate = true;
            }

            const supportedTypes = styleDetails.metrics.supported_button_types || [];
            if (JSON.stringify(instance.config.supportedButtonTypes) !== JSON.stringify(supportedTypes)) {
                updates.supportedButtonTypes = supportedTypes;
                needsUpdate = true;
            }

            if (needsUpdate) {
                onConfigChange(updates);
            }
        }
    }, [styleDetails, instance.config, onConfigChange]);


    const handleButtonsDragEnd = (event: DragEndEvent) => {
        const { active, over } = event;
        if (over && active.id !== over.id) {
            const oldIndex = config.buttons.findIndex(b => b.id === active.id);
            const newIndex = config.buttons.findIndex(b => b.id === over.id);
            onConfigChange({ buttons: arrayMove(config.buttons, oldIndex, newIndex) });
        }
    };

    const handleDebouncedIconIdChange = (value: string) => {
        if (value === "") {
            onConfigChange({ iconId: "" });
            return;
        }
        const numValue = parseInt(value, 10);
        if (isNaN(numValue)) return;
        const clampedValue = Math.max(0, Math.min(numValue, config.maxIconId));
        onConfigChange({ iconId: clampedValue.toString() });
    };

    const systemOptions: ComboboxOption[] = Array.from(groupedStyles.keys()).map(family => ({
        value: family,
        label: family,
    }));

    const themeOptions: ComboboxOption[] = selectedSystem ? (groupedStyles.get(selectedSystem) || []).map(style => ({
        value: style.id,
        label: style.theme_name,
    })) : [];

    const handleSystemChange = (systemFamily: string) => {
        setSelectedSystem(systemFamily);
        const firstTheme = groupedStyles.get(systemFamily)?.[0];
        if (firstTheme) {
            onConfigChange({ styleId: firstTheme.id });
        }
    };

    const isBusy = isLoading || isDetailsLoading;

    return (
        <Accordion type="multiple" defaultValue={["item-1", "item-2", "item-3", "item-4", "item-5"]} className="w-full space-y-2">
            <AccordionItem value="item-1">
                <AccordionTrigger className="text-base font-semibold text-zinc-300">1. Select Style</AccordionTrigger>
                <AccordionContent>
                    <div className="grid w-full items-center gap-2 p-2">
                        {isLoading ? (
                            <>
                                <Skeleton className="h-9 w-full" />
                                <Skeleton className="h-9 w-full" />
                            </>
                        ) : (
                            <>
                                <div className="flex w-full items-start gap-3">
                                    {isLoading ? (
                                        <>
                                            <Skeleton className="h-9 flex-1" />
                                            <Skeleton className="h-9 flex-1" />
                                        </>
                                    ) : (
                                        <>
                                            <div className="grid w-full items-center gap-1.5 flex-1">
                                                <Label htmlFor={`system-${instance.id}`} className="text-zinc-400">Operating System</Label>
                                                <Combobox
                                                    options={systemOptions}
                                                    value={selectedSystem || ""}
                                                    onChange={handleSystemChange}
                                                    placeholder="Select an OS..."
                                                />
                                            </div>
                                            <div className="grid w-full items-center gap-1.5 flex-1">
                                                <Label htmlFor={`theme-${instance.id}`} className="text-zinc-400">Theme</Label>
                                                <Combobox
                                                    options={themeOptions}
                                                    value={config.styleId}
                                                    onChange={(styleId) => onConfigChange({ styleId })}
                                                    placeholder="Select a theme..."
                                                    className={!selectedSystem ? "opacity-50 cursor-not-allowed" : ""}
                                                />
                                            </div>
                                        </>
                                    )}
                                </div>
                            </>
                        )}
                    </div>
                </AccordionContent>
            </AccordionItem>

            <AccordionItem value="item-2">
                <AccordionTrigger className="text-base font-semibold text-zinc-300">2. Main Content</AccordionTrigger>
                <AccordionContent>
                    <div className="grid w-full items-center gap-4 p-2">
                        {isBusy ? (
                            <>
                                <Skeleton className="h-9 w-full" />
                                <Skeleton className="min-h-[100px] w-full" />
                                <Skeleton className="h-9 w-full" />
                            </>
                        ) : (
                            <>
                                <FormField
                                    id={`title-${instance.id}`}
                                    label="Title"
                                    value={config.title}
                                    onDebouncedChange={(title) => onConfigChange({ title })}
                                    placeholder="Enter window title..."
                                />
                                <FormField
                                    as="textarea"
                                    id={`content-${instance.id}`}
                                    label="Content"
                                    value={config.content}
                                    onDebouncedChange={(content) => onConfigChange({ content })}
                                    placeholder="Enter error message content..."
                                    inputClassName="min-h-[100px]"
                                />
                                <FormField
                                    id={`max-width-${instance.id}`}
                                    label="Max Width (px)"
                                    type="number"
                                    value={config.maxWidth}
                                    onDebouncedChange={(maxWidth) => onConfigChange({ maxWidth })}
                                    placeholder="Auto"
                                    min={0}
                                >
                                    <InfoPopover
                                        label="Max Width (px)"
                                        popoverContent="Sets the maximum total width of the final image, including borders. The content area will be scaled down to fit within this limit."
                                    />
                                </FormField>
                            </>
                        )}
                    </div>
                </AccordionContent>
            </AccordionItem>

            <AccordionItem value="item-3">
                <AccordionTrigger className="text-base font-semibold text-zinc-300">3. Icon & Details</AccordionTrigger>
                <AccordionContent>
                    <div className="flex items-end justify-between p-2 gap-4">
                        {isBusy ? (
                            <Skeleton className="h-9 w-full" />
                        ) : (
                            <div className="flex items-end gap-2 w-full">
                                <FormField
                                    id={`icon-${instance.id}`}
                                    label="Icon ID"
                                    type="number"
                                    value={config.iconId}
                                    onDebouncedChange={handleDebouncedIconIdChange}
                                    min={0}
                                    max={config.maxIconId}
                                    placeholder="0"
                                    className="flex-grow"
                                >
                                    <InfoPopover
                                        label="Icon ID"
                                        popoverContent="Each style has its own set of icons. Use the picker button to see all available icons and their IDs for the selected style."
                                    />
                                </FormField>
                                <Button variant="outline" size="icon" className="bg-zinc-800 border-zinc-700 hover:bg-zinc-700 flex-shrink-0" onClick={onOpenIconPicker}>
                                    <ImageIcon className="h-4 w-4" />
                                </Button>
                            </div>
                        )}
                        <div className="flex items-center space-x-2 pb-2 flex-shrink-0">
                            {isBusy ? (<Skeleton className="h-4 w-4" />) : (
                                <InfoPopover
                                    htmlFor={`cross-${instance.id}`}
                                    label="Cross Enabled"
                                    popoverContent="Determines whether the close button (cross) in the window's title bar is rendered as active or disabled."
                                />
                            )}
                            <Checkbox id={`cross-${instance.id}`} checked={config.isCrossEnabled} onCheckedChange={(c) => onConfigChange({ isCrossEnabled: c as boolean })} />
                        </div>
                    </div>
                </AccordionContent>
            </AccordionItem>

            <AccordionItem value="item-4">
                <AccordionTrigger className="text-base font-semibold text-zinc-300">4. Button Constructor</AccordionTrigger>
                <AccordionContent>
                    <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleButtonsDragEnd}>
                        <div className="p-2 space-y-3">
                            <SortableContext items={config.buttons} strategy={verticalListSortingStrategy}>
                                {config.buttons.length > 0 && (
                                    <div className="space-y-2">
                                        {config.buttons.map((button, index) => (
                                            <SortableButton key={button.id} button={button} index={index} isSortEnabled={config.isButtonSortEnabled} onEdit={onEditButton} onDelete={onDeleteButton} />
                                        ))}
                                    </div>
                                )}
                            </SortableContext>
                            <Button variant="outline" className="w-full border-dashed" onClick={onAddNewButton} disabled={isBusy}>
                                <PlusCircle className="mr-2 h-4 w-4" /> Add New Button
                            </Button>
                        </div>
                    </DndContext>
                </AccordionContent>
            </AccordionItem>

            <AccordionItem value="item-5">
                <AccordionTrigger className="text-base font-semibold text-zinc-300">5. Button Layout</AccordionTrigger>
                <AccordionContent>
                    <div className="p-2 space-y-4">
                        <div className="grid w-full items-center gap-1.5">
                            <InfoPopover
                                label="Alignment"
                                popoverContent="Controls the horizontal alignment of the entire button block. The 'Auto' option uses the default alignment defined by the selected style, while other options force a specific layout."
                            />
                            <RadioGroup value={config.buttonAlignment} onValueChange={(v) => onConfigChange({ buttonAlignment: v as ButtonAlignment })} className="flex flex-wrap gap-4 pt-1">
                                <div className="flex items-center space-x-2"><RadioGroupItem value="Auto" id={`align-auto-${instance.id}`} /><Label htmlFor={`align-auto-${instance.id}`}>Auto</Label></div>
                                <div className="flex items-center space-x-2"><RadioGroupItem value="Left" id={`align-left-${instance.id}`} /><Label htmlFor={`align-left-${instance.id}`}>Left</Label></div>
                                <div className="flex items-center space-x-2"><RadioGroupItem value="Center" id={`align-center-${instance.id}`} /><Label htmlFor={`align-center-${instance.id}`}>Center</Label></div>
                                <div className="flex items-center space-x-2"><RadioGroupItem value="Right" id={`align-right-${instance.id}`} /><Label htmlFor={`align-right-${instance.id}`}>Right</Label></div>
                            </RadioGroup>
                        </div>
                        <div className="flex items-center justify-between">
                            <InfoPopover
                                htmlFor={`auto-sort-${instance.id}`}
                                label="Auto-sort Buttons"
                                popoverContent="When enabled, buttons are automatically arranged based on their type in an order predefined by the style (e.g., Recommended, then Default, then Disabled). Disable this to allow manual drag-and-drop sorting."
                            />
                            <Switch id={`auto-sort-${instance.id}`} checked={config.isButtonSortEnabled} onCheckedChange={(c) => onConfigChange({ isButtonSortEnabled: c })} />
                        </div>
                    </div>
                </AccordionContent>
            </AccordionItem>
        </Accordion>
    );
};

export const ErrorInstance = React.memo(ErrorInstanceFC);