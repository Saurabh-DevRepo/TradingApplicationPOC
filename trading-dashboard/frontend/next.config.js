/** @type {import('next').NextConfig} */
const nextConfig = {
  // Allow importing from the shared workspace package
  transpilePackages: ["shared"],

  // Strict mode for catching React issues early
  reactStrictMode: true,

  // WebSocket connections go directly to the backend — no Next.js proxying needed
  // If you want to proxy via Next.js dev server, add rewrites() here
};

module.exports = nextConfig;
