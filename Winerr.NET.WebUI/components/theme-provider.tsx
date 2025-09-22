"use client"

import * as React from "react"
import { ThemeProvider as NextThemesProvider } from "next-themes"
type NextThemesProviderProps = React.ComponentProps<typeof NextThemesProvider>

export function ThemeProvider({ children, ...props }: NextThemesProviderProps) {
    return <NextThemesProvider {...props}>{children}</NextThemesProvider>
}
