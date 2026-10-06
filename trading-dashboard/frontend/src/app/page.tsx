"use client";

import React from "react";
import { useWebSocket } from "@/hooks/useWebSocket";
import Watchlist from "@/components/Watchlist";
import EngineMetrics from "@/components/EngineMetrics";

const WS_URL = process.env.NEXT_PUBLIC_WS_URL ?? "ws://localhost:5000/ws";

export default function DashboardPage() {
  useWebSocket({ url: WS_URL });

  return (
    <div className="dashboard">
      <h1>Trading Dashboard (POC)</h1>

      <EngineMetrics />

      <Watchlist />
    </div>
  );
}
