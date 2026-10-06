"use client";

import { create } from "zustand";
import { subscribeWithSelector } from "zustand/middleware";
import type { TickEvent, OHLCBar, EngineMetrics } from "shared";

export interface TickerData {
  latest: TickEvent;
  prevPrice: number;
  ohlc?: OHLCBar;
}

interface MarketState {
  tickers: Record<string, TickerData>;
  symbols: string[];
  metrics: EngineMetrics;
  wsStatus: "connecting" | "open" | "closed" | "error";

  applyTick: (tick: TickEvent) => void;
  applyOHLC: (bar: OHLCBar) => void;
  setMetrics: (m: Partial<EngineMetrics>) => void;
  setWsStatus: (s: MarketState["wsStatus"]) => void;
}

export const useMarketStore = create<MarketState>()(
  subscribeWithSelector((set) => ({
    tickers: {},
    symbols: [],
    metrics: { ticksPerSecond: 0, fps: 0, latencyMs: 0, connectedClients: 0 },
    wsStatus: "connecting",

    applyTick: (tick) =>
      set((state) => {
        const prev = state.tickers[tick.symbol];
        const newTickers = {
          ...state.tickers,
          [tick.symbol]: {
            latest: tick,
            prevPrice: prev?.latest.price ?? tick.price,
            ohlc: prev?.ohlc,
          },
        };
        return {
          tickers: newTickers,
          symbols: !prev ? Object.keys(newTickers).sort() : state.symbols,
        };
      }),

    applyOHLC: (bar) =>
      set((state) => {
        const prev = state.tickers[bar.symbol];
        if (!prev) return state;
        return {
          tickers: { ...state.tickers, [bar.symbol]: { ...prev, ohlc: bar } },
        };
      }),

    setMetrics: (m) => set((state) => ({ metrics: { ...state.metrics, ...m } })),
    setWsStatus: (wsStatus) => set({ wsStatus }),
  }))
);

export const selectTicker = (symbol: string) => (s: MarketState) => s.tickers[symbol];
export const selectAllSymbols = (s: MarketState) => s.symbols;
export const selectMetrics = (s: MarketState) => s.metrics;
export const selectWsStatus = (s: MarketState) => s.wsStatus;
