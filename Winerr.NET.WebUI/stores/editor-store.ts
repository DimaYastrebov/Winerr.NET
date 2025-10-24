/* eslint-disable @typescript-eslint/no-explicit-any */
import { create } from "zustand";
import { arrayMove } from "@dnd-kit/sortable";
import { toast } from "sonner";
import type { DragEndEvent } from "@dnd-kit/core";
import {
    type SystemStyle,
    type ArchiveFormat,
    type ErrorConfig,
    type ImportedInstance
} from "@/lib/types";
import { TFunction } from "i18next";

const createNewErrorInstance = (name: string, styleId: string): ErrorConfig => ({
    id: `inst-${Math.random().toString(36).slice(2)}`,
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
});

interface EditorState {
    mode: 'single' | 'batch';
    errorInstances: ErrorConfig[];
    batchSettings: { format: ArchiveFormat, compression: number };
    isGenerating: boolean;
    generatedImageUrl: string | null;
    generatedImageBlob: Blob | null;
    generationTime: number | null;
    isInitialized: boolean;
}

interface EditorActions {
    initialize: (initialState: {
        instances: ErrorConfig[],
        mode: 'single' | 'batch',
        batchSettings: { format: ArchiveFormat, compression: number }
    } | null, styles: SystemStyle[], t: TFunction) => void;
    setMode: (mode: 'single' | 'batch') => void;
    setBatchSettings: (settings: { format: ArchiveFormat, compression: number }) => void;
    setIsGenerating: (isGenerating: boolean) => void;
    setGeneratedImage: (blob: Blob | null) => void;
    setGenerationTime: (time: number | null) => void;
    addInstance: (styles: SystemStyle[]) => void;
    deleteInstance: (id: string, styles: SystemStyle[]) => void;
    updateInstance: (id: string, configUpdate: Partial<ErrorConfig['config']>) => void;
    updateInstanceName: (id: string, newName: string) => void;
    handleInstancesDragEnd: (event: DragEndEvent) => void;
    importState: (importedData: any, styles: SystemStyle[], t: TFunction) => void;
    clearSession: (t: TFunction) => void;
}

export const useEditorStore = create<EditorState & EditorActions>((set, get) => ({
    mode: 'single',
    errorInstances: [],
    batchSettings: { format: 'zip', compression: 6 },
    isGenerating: false,
    generatedImageUrl: null,
    generatedImageBlob: null,
    generationTime: null,
    isInitialized: false,

    initialize: (initialState, styles, t) => {
        if (get().isInitialized) return;
        if (initialState && initialState.instances && initialState.instances.length > 0) {
            set({
                errorInstances: initialState.instances,
                mode: initialState.mode,
                batchSettings: initialState.batchSettings,
                isInitialized: true
            });
            toast.success(t('toasts.session_restored_title'), { description: t('toasts.session_restored_desc') });
        } else {
            const firstStyleId = styles.length > 0 ? styles[0].id : "";
            set({
                errorInstances: [createNewErrorInstance("Default Error", firstStyleId)],
                isInitialized: true
            });
        }
    },

    setMode: (mode) => set({ mode }),
    setBatchSettings: (batchSettings) => set({ batchSettings }),
    setIsGenerating: (isGenerating) => set({ isGenerating }),
    setGenerationTime: (generationTime) => set({ generationTime }),
    setGeneratedImage: (blob) => {
        const currentUrl = get().generatedImageUrl;
        if (currentUrl) {
            URL.revokeObjectURL(currentUrl);
        }
        if (blob) {
            set({ generatedImageBlob: blob, generatedImageUrl: URL.createObjectURL(blob) });
        } else {
            set({ generatedImageBlob: null, generatedImageUrl: null });
        }
    },

    addInstance: (styles) => set((state) => {
        const styleId = styles.length > 0 ? styles[0].id : "";
        const newInstance = createNewErrorInstance(`Error #${state.errorInstances.length + 1}`, styleId);
        return { errorInstances: [...state.errorInstances, newInstance] };
    }),

    deleteInstance: (id, styles) => set((state) => {
        const newInstances = state.errorInstances.filter(inst => inst.id !== id);
        if (newInstances.length > 0) {
            return { errorInstances: newInstances };
        }
        const styleId = styles.length > 0 ? styles[0].id : "";
        return { errorInstances: [createNewErrorInstance("Default Error", styleId)] };
    }),

    updateInstance: (id, configUpdate) => set((state) => ({
        errorInstances: state.errorInstances.map(inst =>
            inst.id === id ? { ...inst, config: { ...inst.config, ...configUpdate } } : inst
        )
    })),

    updateInstanceName: (id, newName) => set((state) => ({
        errorInstances: state.errorInstances.map(inst =>
            inst.id === id ? { ...inst, name: newName } : inst
        )
    })),

    handleInstancesDragEnd: (event) => {
        const { active, over } = event;
        if (over && active.id !== over.id) {
            set((state) => {
                const oldIndex = state.errorInstances.findIndex((item) => item.id === active.id);
                const newIndex = state.errorInstances.findIndex((item) => item.id === over.id);
                return { errorInstances: arrayMove(state.errorInstances, oldIndex, newIndex) };
            });
        }
    },

    importState: (importedData, styles, t) => {
        try {
            if (Array.isArray(importedData.instances)) {
                const styleId = styles.length > 0 ? styles[0].id : "";
                const validatedInstances = importedData.instances.map((item: ImportedInstance) => {
                    const newInst = createNewErrorInstance(item.name || "Imported Error", item.config?.styleId || styleId);
                    newInst.config = { ...newInst.config, ...item.config };
                    return newInst;
                });

                set({
                    errorInstances: validatedInstances,
                    mode: importedData.mode || (validatedInstances.length > 1 ? 'batch' : 'single'),
                    batchSettings: importedData.batchSettings || { format: 'zip', compression: 6 }
                });
                toast.success(t('toasts.import_success'));
            } else {
                throw new Error("Invalid config format: 'instances' array not found.");
            }
        } catch (error) {
            console.error("Failed to import file:", error);
            toast.error(t('toasts.import_fail_title'), { description: t('toasts.import_fail_desc') });
        }
    },

    clearSession: (t) => {
        localStorage.removeItem('winerr-autosave');
        toast.info(t('toasts.session_cleared_title'), { description: t('toasts.session_cleared_desc') });
        setTimeout(() => {
            window.location.reload();
        }, 1000);
    },
}));