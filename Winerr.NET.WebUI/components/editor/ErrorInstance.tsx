"use client";

import React from "react";
import { DndContext, closestCenter, type DragEndEvent, PointerSensor, KeyboardSensor, useSensor, useSensors } from "@dnd-kit/core";
import { SortableContext, verticalListSortingStrategy } from "@dnd-kit/sortable";
import { PlusCircle, Image as ImageIcon } from "lucide-react";

import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Skeleton } from "@/components/ui/skeleton";
import { Switch } from "@/components/ui/switch";
import { ButtonAlignment, ErrorConfig } from "@/app/page";
import { SortableButton, ButtonConfig } from "./ButtonConstructor";
import { FormField } from "./FormField";
import { Combobox, ComboboxOption } from "@/components/ui/combobox";
import { InfoPopover } from "./InfoPopover";

interface SystemStyle { id: string; display_name: string; }

interface ErrorInstanceProps {
    instance: ErrorConfig;
    styles: SystemStyle[];
    isLoading: boolean;
    onConfigChange: (newConfig: Partial<ErrorConfig['config']>) => void;
    onOpenIconPicker: () => void;
    onAddNewButton: () => void;
    onEditButton: (button: ButtonConfig) => void;
    onDeleteButton: (buttonId: string) => void;
    onDragEnd: (event: DragEndEvent) => void;
}

export const ErrorInstance: React.FC<ErrorInstanceProps> = ({
    instance, styles, isLoading, onConfigChange, onOpenIconPicker,
    onAddNewButton, onEditButton, onDeleteButton, onDragEnd
}) => {
    const sensors = useSensors(useSensor(PointerSensor), useSensor(KeyboardSensor));
    const { config } = instance;

    const handleDebouncedIconIdChange = (value: string) => {
        if (value === "") {
            onConfigChange({ iconId: "" });
            return;
        }

        const numValue = parseInt(value, 10);
        if (isNaN(numValue)) {
            return;
        }

        const clampedValue = Math.max(0, Math.min(numValue, config.maxIconId));
        onConfigChange({ iconId: clampedValue.toString() });
    };

    const styleOptions: ComboboxOption[] = styles.map(style => ({
        value: style.id,
        label: style.display_name,
    }));

    return (
        <Accordion type="multiple" defaultValue={["item-1", "item-2", "item-3", "item-4", "item-5"]} className="w-full space-y-2">

            <AccordionItem value="item-1">
                <AccordionTrigger className="text-base font-semibold text-zinc-300">1. Select Style</AccordionTrigger>
                <AccordionContent>
                    <div className="grid w-full items-center gap-1.5 p-2">
                        <Label htmlFor={`style-${instance.id}`} className="text-zinc-400">Select a style</Label>
                        {isLoading ? (
                            <Skeleton className="h-9 w-full" />
                        ) : (
                            <Combobox
                                options={styleOptions}
                                value={config.styleId}
                                onChange={(styleId) => onConfigChange({ styleId })}
                                placeholder="Select a style..."
                            />
                        )}
                    </div>
                </AccordionContent>
            </AccordionItem>

            <AccordionItem value="item-2">
                <AccordionTrigger className="text-base font-semibold text-zinc-300">2. Main Content</AccordionTrigger>
                <AccordionContent>
                    <div className="grid w-full items-center gap-4 p-2">
                        {isLoading ? (
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
                                />
                            </>
                        )}
                    </div>
                </AccordionContent>
            </AccordionItem>

            <AccordionItem value="item-3">
                <AccordionTrigger className="text-base font-semibold text-zinc-300">3. Icon & Details</AccordionTrigger>
                <AccordionContent>
                    <div className="flex items-end justify-between p-2 gap-4">
                        {isLoading ? (
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
                                />
                                <Button variant="outline" size="icon" className="bg-zinc-800 border-zinc-700 hover:bg-zinc-700 flex-shrink-0" onClick={onOpenIconPicker}>
                                    <ImageIcon className="h-4 w-4" />
                                </Button>
                            </div>
                        )}
                        <div className="flex items-center space-x-2 pb-2 flex-shrink-0">
                            {isLoading ? (<Skeleton className="h-4 w-4" />) : (<Checkbox id={`cross-${instance.id}`} checked={config.isCrossEnabled} onCheckedChange={(c) => onConfigChange({ isCrossEnabled: c as boolean })} />)}
                            <Label htmlFor={`cross-${instance.id}`} className="text-zinc-400 whitespace-nowrap">Cross Enabled?</Label>
                        </div>
                    </div>
                </AccordionContent>
            </AccordionItem>

            <AccordionItem value="item-4">
                <AccordionTrigger className="text-base font-semibold text-zinc-300">4. Button Constructor</AccordionTrigger>
                <AccordionContent>
                    <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={onDragEnd}>
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
                            <Button variant="outline" className="w-full border-dashed" onClick={onAddNewButton} disabled={isLoading}>
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
                                popoverContent="When enabled, buttons are automatically arranged in an order predefined by the selected style (e.g., 'OK', 'Cancel', 'Yes', 'No'). Disable this to allow manual drag-and-drop sorting."
                            />
                            <Switch id={`auto-sort-${instance.id}`} checked={config.isButtonSortEnabled} onCheckedChange={(c) => onConfigChange({ isButtonSortEnabled: c })} />
                        </div>
                    </div>
                </AccordionContent>
            </AccordionItem>

        </Accordion>
    );
};
