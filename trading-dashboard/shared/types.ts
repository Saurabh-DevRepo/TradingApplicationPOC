export interface TickEvent {
  symbol: string;
  timestamp: number;
  price: number;
  volume: number;
}

export interface OHLCBar {
  symbol: string;
  openTime: number;
  closeTime: number;
  open: number;
  high: number;
  low: number;
  close: number;
  volume: number;
  vwap: number;
}

export type ServerMessage =
  | { type: "tick"; data: TickEvent }
  | { type: "ohlc"; data: OHLCBar }
  | { type: "error"; message: string };

export interface EngineMetrics {
  ticksPerSecond: number;
  fps: number;
  latencyMs: number;
  connectedClients: number;
}
