"use client";

import React, { useEffect, useState } from "react";
import { Pencil, Trash2, GripVertical } from "lucide-react";
import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { useTranslation } from "react-i18next";

import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { InfoPopover } from "./InfoPopover";

export type ButtonConfig = { id: string; text: string; type: 'Default' | 'Recommended' | 'Disabled'; mnemonic: boolean; };

interface ButtonDialogProps {
    isOpen: boolean;
    onOpenChange: (isOpen: boolean) => void;
    onSave: (button: ButtonConfig) => void;
    buttonData: ButtonConfig | null;
    supportedButtonTypes: string[];
}

export const ButtonDialog: React.FC<ButtonDialogProps> = ({ isOpen, onOpenChange, onSave, buttonData, supportedButtonTypes }) => {
    const { t } = useTranslation();
    const [text, setText] = useState("");
    const [type, setType] = useState<ButtonConfig['type']>('Default');
    const [mnemonic, setMnemonic] = useState(false);

    useEffect(() => {
        if (isOpen) {
            if (buttonData) {
                setText(buttonData.text);
                setType(buttonData.type);
                setMnemonic(buttonData.mnemonic);
            } else {
                setText("");
                const defaultType = supportedButtonTypes?.includes('Default') ? 'Default' : (supportedButtonTypes?.[0] as ButtonConfig['type']) || 'Default';
                setType(defaultType);
                setMnemonic(false);
            }
        }
    }, [isOpen, buttonData, supportedButtonTypes]);

    const handleSave = () => {
        onSave({
            id: buttonData?.id || Math.random().toString(36).slice(2),
            text, type, mnemonic,
        });
        onOpenChange(false);
    };

    return (
        <Dialog open={isOpen} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[425px] bg-zinc-900 border-zinc-800">
                <DialogHeader>
                    <DialogTitle>{buttonData ? t('button_dialog.title_edit') : t('button_dialog.title_add')}</DialogTitle>
                </DialogHeader>

                <div className="grid gap-6 py-4">

                    <div className="flex flex-col gap-2">
                        <Label htmlFor="text" className="text-zinc-400">{t('button_dialog.text_label')}</Label>
                        <Input id="text" value={text} onChange={(e) => setText(e.target.value)} className="bg-zinc-800 border-zinc-700" />
                    </div>

                    <div className="flex flex-col gap-3">
                        <Label className="text-zinc-400">{t('button_dialog.type_label')}</Label>
                        <RadioGroup value={type} onValueChange={(v) => setType(v as ButtonConfig['type'])} className="flex flex-wrap gap-x-4 gap-y-2">
                            {supportedButtonTypes && supportedButtonTypes.length > 0 ? supportedButtonTypes.map(btnType => (
                                <div key={btnType} className="flex items-center space-x-2">
                                    <RadioGroupItem value={btnType} id={`r-${btnType}`} />
                                    <Label htmlFor={`r-${btnType}`} className="font-normal">{t(`button_types.${btnType}`, btnType)}</Label>
                                </div>
                            )) : <p className="text-xs text-zinc-500">{t('button_dialog.no_types_supported')}</p>}
                        </RadioGroup>
                    </div>

                    <div className="flex flex-row items-center justify-left">
                        <InfoPopover
                            htmlFor="mnemonic"
                            label={t('button_dialog.mnemonic_label')}
                            popoverContent={t('button_dialog.mnemonic_popover')}
                        />
                        <Checkbox className="ml-2" id="mnemonic" checked={mnemonic} onCheckedChange={(c) => setMnemonic(c as boolean)} />
                    </div>

                </div>

                <DialogFooter>
                    <DialogClose asChild><Button type="button" variant="secondary">{t('button_dialog.cancel')}</Button></DialogClose>
                    <Button type="button" onClick={handleSave}>{t('button_dialog.save')}</Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};

interface SortableButtonProps {
    button: ButtonConfig;
    index: number;
    isSortEnabled: boolean;
    onEdit: (button: ButtonConfig) => void;
    onDelete: (id: string) => void;
}

export const SortableButton: React.FC<SortableButtonProps> = ({ button, index, isSortEnabled, onEdit, onDelete }) => {
    const { t } = useTranslation();
    const { attributes, listeners, setNodeRef, transform, transition } = useSortable({ id: button.id, disabled: isSortEnabled });
    const style = { transform: CSS.Transform.toString(transform), transition };

    return (
        <div ref={setNodeRef} style={style} className="flex items-center justify-between p-2 bg-zinc-800 rounded-md">
            <div className="flex items-center gap-2">
                {!isSortEnabled && (<div {...attributes} {...listeners} className="cursor-grab touch-none p-1"><GripVertical className="h-4 w-4 text-zinc-500" /></div>)}
                {!isSortEnabled && <span className="text-xs text-zinc-500">{index + 1}.</span>}
                <span className="font-mono text-xs text-zinc-500">{t(`button_types.${button.type}`, button.type)}</span>
                <p className="text-zinc-200">{button.text}</p>
            </div>
            <div className="flex gap-1">
                <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => onEdit(button)}><Pencil className="h-4 w-4 text-zinc-400" /></Button>
                <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => onDelete(button.id)}><Trash2 className="h-4 w-4 text-red-500/80" /></Button>
            </div>
        </div>
    );
};