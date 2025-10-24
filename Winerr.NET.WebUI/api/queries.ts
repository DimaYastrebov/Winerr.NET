import { useQuery, useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import {
    type SystemStyle,
    type StyleDetailsData,
    type GenerateRequestBody,
    type ArchiveFormat
} from "@/lib/types";
import { useEditorStore } from "@/stores/editor-store";
import { TFunction } from "i18next";

const fetchStyles = async (): Promise<SystemStyle[]> => {
    const response = await fetch("/v1/styles");
    if (!response.ok) {
        throw new Error("Server is unavailable. Failed to fetch styles.");
    }
    const data = await response.json();
    return data.data || [];
};

const fetchStyleDetails = async (styleId: string): Promise<StyleDetailsData> => {
    const response = await fetch(`/v1/styles/${styleId}`);
    if (!response.ok) {
        throw new Error(`Failed to fetch details for style ${styleId}`);
    }
    const data = await response.json();
    return data.data;
};

const generateSingleImage = async (requestBody: GenerateRequestBody): Promise<{ blob: Blob, usageHeader: string | null }> => {
    const response = await fetch("/v1/images/generate", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(requestBody),
    });

    if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.error?.message || `Generation failed: ${response.statusText}`);
    }

    const usageHeader = response.headers.get("X-Usage-Details");
    const blob = await response.blob();
    return { blob, usageHeader };
};

const generateBatch = async (
    requests: GenerateRequestBody[],
    format: ArchiveFormat,
    compression: number
): Promise<void> => {
    const response = await fetch("/v1/images/generate/batch", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            requests: requests,
            archive_format: format,
            compression_level: compression,
        }),
    });

    if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.error?.message || `Batch generation failed: ${response.statusText}`);
    }

    const archiveBlob = await response.blob();
    const url = URL.createObjectURL(archiveBlob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `winerr_batch_${Date.now()}.${format}`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

export const useGetStyles = () => {
    return useQuery<SystemStyle[], Error>({
        queryKey: ['styles'],
        queryFn: fetchStyles,
        staleTime: Infinity,
        retry: false,
    });
};

export const useGetStyleDetails = (styleId: string) => {
    return useQuery<StyleDetailsData, Error>({
        queryKey: ['styleDetails', styleId],
        queryFn: () => fetchStyleDetails(styleId),
        enabled: !!styleId,
        staleTime: Infinity,
    });
};

export const useGenerateImage = (t: TFunction) => {
    const { setIsGenerating, setGeneratedImage, setGenerationTime } = useEditorStore.getState();

    return useMutation({
        mutationFn: generateSingleImage,
        onMutate: () => {
            setIsGenerating(true);
            setGeneratedImage(null);
            setGenerationTime(null);
        },
        onSuccess: ({ blob, usageHeader }) => {
            if (usageHeader) {
                try {
                    const usageData = JSON.parse(usageHeader);
                    setGenerationTime(usageData.GenerationTimeMs);
                } catch (e) {
                    console.warn("Could not parse X-Usage-Details header.");
                }
            }
            setGeneratedImage(blob);
        },
        onError: (error: Error) => {
            toast.error(t('toasts.generate_fail_title'), { description: error.message });
        },
        onSettled: () => {
            setIsGenerating(false);
        },
    });
};

export const useGenerateBatch = (t: TFunction) => {
    const { setIsGenerating } = useEditorStore.getState();

    return useMutation({
        mutationFn: (variables: { requests: GenerateRequestBody[], format: ArchiveFormat, compression: number }) =>
            generateBatch(variables.requests, variables.format, variables.compression),
        onMutate: () => {
            setIsGenerating(true);
        },
        onSuccess: () => {
            toast.success(t('toasts.batch_success_title'), { description: t('toasts.batch_success_desc') });
        },
        onError: (error: Error) => {
            toast.error(t('toasts.batch_fail_title'), { description: error.message });
        },
        onSettled: () => {
            setIsGenerating(false);
        },
    });
};