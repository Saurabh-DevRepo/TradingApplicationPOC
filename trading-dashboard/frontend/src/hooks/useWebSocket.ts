"use client";

import { useEffect, useRef } from "react";
import type { ServerMessage } from "shared";
import { useMarketStore } from "@/store/marketStore";

interface UseWebSocketOptions {
  url: string;
  baseDelayMs?: number;
  maxDelayMs?: number;
}

export function useWebSocket({ url, baseDelayMs = 500, maxDelayMs = 30_000 }: UseWebSocketOptions): void {
  const wsRef = useRef<WebSocket | null>(null);
  const retryDelay = useRef(baseDelayMs);
  const retryTimer = useRef<ReturnType<typeof setTimeout> | null>(null);
  const dead = useRef(false);

  useEffect(() => {
    dead.current = false;
    retryDelay.current = baseDelayMs;

    const { applyTick, applyOHLC, setWsStatus } = useMarketStore.getState();

    function connect() {
      if (dead.current) return;

      setWsStatus("connecting");
      const ws = new WebSocket(url);
      wsRef.current = ws;

      ws.onopen = () => {
        console.log("[ws] connected");
        setWsStatus("open");
        retryDelay.current = baseDelayMs;
      };

      ws.onmessage = ({ data }) => {
        let msg: ServerMessage;
        try { msg = JSON.parse(data); } catch { return; }

        if (msg.type === "tick") applyTick(msg.data);
        else if (msg.type === "ohlc") applyOHLC(msg.data);
        else if (msg.type === "error") console.warn("[ws] server error:", msg.message);
      };

      ws.onclose = () => {
        if (dead.current) return;
        setWsStatus("closed");
        console.log(`[ws] closed, retrying in ${retryDelay.current}ms`);
        retryTimer.current = setTimeout(() => {
          retryDelay.current = Math.min(retryDelay.current * 2, maxDelayMs);
          connect();
        }, retryDelay.current);
      };

      ws.onerror = () => {
        setWsStatus("error");
        console.warn("[ws] error");
      };
    }

    connect();

    return () => {
      dead.current = true;
      if (retryTimer.current) clearTimeout(retryTimer.current);
      const ws = wsRef.current;
      if (ws && ws.readyState !== WebSocket.CLOSED) {
        ws.onclose = null;
        ws.close();
      }
    };
  }, [url, baseDelayMs, maxDelayMs]);

  // fps counter
  useEffect(() => {
    const { setMetrics } = useMarketStore.getState();
    let frameId: number;
    let last = performance.now();
    let count = 0;

    function tick(ts: number) {
      count++;
      if (ts - last >= 1000) {
        setMetrics({ fps: count });
        count = 0;
        last = ts;
      }
      frameId = requestAnimationFrame(tick);
    }

    frameId = requestAnimationFrame(tick);
    return () => cancelAnimationFrame(frameId);
  }, []);
}
