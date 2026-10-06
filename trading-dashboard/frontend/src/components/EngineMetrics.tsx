"use client";

import React from "react";
import { useMarketStore, selectMetrics, selectWsStatus } from "@/store/marketStore";

const DOT_COLOR: Record<string, string> = {
  open: "green",
  connecting: "orange",
  closed: "gray",
  error: "red",
};

export default function EngineMetrics() {
  const metrics = useMarketStore(selectMetrics);
  const wsStatus = useMarketStore(selectWsStatus);

  return (
    <div className="metrics-box">
      <span className="ws-dot" style={{ background: DOT_COLOR[wsStatus] ?? "gray" }} />
      WS: {wsStatus} &nbsp;|&nbsp;
      ticks/s: {metrics.ticksPerSecond.toFixed(0)} &nbsp;|&nbsp;
      fps: {metrics.fps.toFixed(0)} &nbsp;|&nbsp;
      latency: {metrics.latencyMs}ms &nbsp;|&nbsp;
      clients: {metrics.connectedClients}
    </div>
  );
}
