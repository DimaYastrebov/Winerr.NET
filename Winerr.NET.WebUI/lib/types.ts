import { type ButtonConfig as ButtonConstructorConfig } from "@/components/editor/ButtonConstructor";

export interface SystemStyle {
    id: string;
    display_name: string;
    system_family: string;
    theme_name: string;
}

export type ButtonAlignment = 'Auto' | 'Left' | 'Center' | 'Right';
export type ArchiveFormat = 'zip' | 'tar';
export type ButtonConfig = ButtonConstructorConfig;

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

export interface GenerateRequestBody {
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

export interface StyleDetailsData {
    max_icon_id: number;
    metrics: {
        supported_button_types: string[];
    };
}

export interface ImportedInstance {
    id: string;
    name: string;
    config: Partial<ErrorInstanceConfig>;
}