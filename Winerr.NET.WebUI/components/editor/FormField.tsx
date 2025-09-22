"use client";

import React, { useState, useEffect, memo } from "react";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { cn } from "@/lib/utils";

interface FormFieldProps {
    id: string;
    label: string;
    value: string;
    onDebouncedChange: (value: string) => void;
    as?: 'input' | 'textarea';
    placeholder?: string;
    type?: string;
    className?: string;
    labelClassName?: string;
    inputClassName?: string;
    min?: number;
    max?: number;
}

const FormFieldComponent: React.FC<FormFieldProps> = ({
    id,
    label,
    value: propValue,
    onDebouncedChange,
    as = 'input',
    className,
    labelClassName,
    inputClassName,
    ...props
}) => {
    const [internalValue, setInternalValue] = useState(propValue);

    useEffect(() => {
        setInternalValue(propValue);
    }, [propValue]);

    useEffect(() => {
        const handler = setTimeout(() => {
            if (internalValue !== propValue) {
                onDebouncedChange(internalValue);
            }
        }, 200);

        return () => {
            clearTimeout(handler);
        };
    }, [internalValue, onDebouncedChange, propValue]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        setInternalValue(e.target.value);
    };

    const InputComponent = as === 'textarea' ? Textarea : Input;

    return (
        <div className={cn("grid w-full items-center gap-1.5", className)}>

            <Label htmlFor={id} className={cn("text-zinc-400", labelClassName)}>
                {label}
            </Label>

            <InputComponent
                id={id}
                value={internalValue}
                onChange={handleChange}
                className={cn("bg-zinc-800 border-zinc-700", inputClassName)}
                {...props}
            />

        </div>
    );
};

export const FormField = memo(FormFieldComponent);
