import type { NextConfig } from "next";

const isProd = process.env.NODE_ENV === 'production';

const nextConfig: NextConfig = {
  output: isProd ? 'export' : undefined,
  
  images: {
    unoptimized: true,
  },

  ...(!isProd ? {
    async rewrites() {
      return [
        {
          source: "/v1/:path*",
          destination: "http://localhost:5000/v1/:path*",
        },
      ];
    },
  } : {}),
};

export default nextConfig;