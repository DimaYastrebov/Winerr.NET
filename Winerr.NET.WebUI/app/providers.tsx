"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState } from "react";
import { I18nextProvider } from "react-i18next";
import i18n from "@/i18n";

export function Providers({ children }: { children: React.ReactNode }) {
    const [queryClient] = useState(() => new QueryClient());

    return (
        <I18nextProvider i18n={i18n}>
            <QueryClientProvider client={queryClient}>
                {children}
            </QueryClientProvider>
        </I18nextProvider>
    );
}