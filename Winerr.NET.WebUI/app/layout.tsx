import type { Metadata } from "next";
import { Inter } from "next/font/google";
import { ThemeProvider } from "@/components/theme-provider";
import { Toaster } from "@/components/ui/sonner";
import { Providers } from "./providers";
import "./globals.css";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
    title: {
        default: "Winerr.NET WebUI",
        template: "%s | Winerr.NET WebUI",
    },
    description: "A powerful web-based editor for Winerr.NET. Create and customize images of classic OS dialog windows. Style them, add text, icons, and buttons.",
    keywords: ["Winerr.NET", "dialog generator", "image generator", "OS dialog", "classic UI", "retro UI", "error message generator", "Windows", "macOS", "Linux"],
    authors: [{ name: 'DimaYastrebov', url: 'https://github.com/DimaYastrebov' }],
    creator: 'DimaYastrebov',
    icons: {
        icon: '/favicon.ico',
        shortcut: '/favicon.ico',
        apple: '/favicon.ico',
    },
    manifest: "/manifest.json",
    themeColor: [{ media: '(prefers-color-scheme: dark)', color: '#0a0a0a' }],
    openGraph: {
        title: "Winerr.NET WebUI",
        description: "Create and customize images of classic OS dialog windows with a powerful web-based editor.",
        url: "https://winerr-net.yastre.top",
        siteName: "Winerr.NET WebUI",
        images: [
            {
                url: "https://cdn.yastre.top/images/Winerr.NET%20Github%20Banner_v7.png?v=2",
                width: 1280,
                height: 640,
                alt: "Winerr.NET Preview Banner",
            },
        ],
        locale: 'en_US',
        type: 'website',
    },
    twitter: {
        card: "summary_large_image",
        title: "Winerr.NET WebUI",
        description: "Create and customize images of classic OS dialog windows with a powerful web-based editor.",
        images: ["https://cdn.yastre.top/images/Winerr.NET%20Github%20Banner_v7.png?v=2"],
    },
    robots: {
        index: true,
        follow: true,
        googleBot: {
            index: true,
            follow: true,
        },
    },
};

export default function RootLayout({
    children,
}: Readonly<{
    children: React.ReactNode;
}>) {
    return (
        <html lang="en" suppressHydrationWarning>
            <body className={inter.className}>
                <Providers>
                    <ThemeProvider
                        attribute="class"
                        defaultTheme="dark"
                        enableSystem
                        disableTransitionOnChange
                    >
                        {children}
                        <Toaster theme="dark" />
                    </ThemeProvider>
                </Providers>
            </body>
        </html>
    );
}