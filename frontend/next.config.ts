import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Output standalone for Azure App Service deployment
  output: "standalone",
  images: {
    remotePatterns: [
      {
        protocol: "https",
        hostname: "**",
      },
    ],
  },
  turbopack: {},
};

export default nextConfig;
