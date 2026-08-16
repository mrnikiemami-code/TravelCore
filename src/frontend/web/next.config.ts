import type { NextConfig } from "next";

/**
 * P06-R4 APP PROXY: Media bytes are served by TravelCore, not object-storage hosts.
 * remotePatterns are only needed when the public Media API origin differs from the
 * Next.js origin. Never allowlist wildcard or provider object-storage hostnames.
 */
function mediaApiRemotePatterns(): NonNullable<
  NonNullable<NextConfig["images"]>["remotePatterns"]
> {
  const raw =
    process.env.TRAVELCORE_API_BASE_URL ?? process.env.API_BASE_URL ?? undefined;
  if (!raw) {
    return [];
  }

  try {
    const url = new URL(raw);
    const protocol = url.protocol.replace(":", "") as "http" | "https";
    if (protocol !== "http" && protocol !== "https") {
      return [];
    }

    return [
      {
        protocol,
        hostname: url.hostname,
        ...(url.port ? { port: url.port } : {}),
        pathname: "/api/media/**",
      },
    ];
  } catch {
    return [];
  }
}

const remotePatterns = mediaApiRemotePatterns();

const nextConfig: NextConfig = {
  ...(remotePatterns.length > 0
    ? {
        images: {
          remotePatterns,
        },
      }
    : {}),
};

export default nextConfig;
