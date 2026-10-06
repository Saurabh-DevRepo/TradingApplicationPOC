# Trading Dashboard — Implementation Checklist

## Phase 0 — Project Scaffold ✅
- [x] Root monorepo `package.json` with npm workspaces
- [x] `shared/types.ts` — wire-format types (TickEvent, OHLCBar, EngineMetrics…)
- [x] `backend/` — tsconfig, package.json
- [x] `frontend/` — tsconfig, package.json, next.config.js
- [x] All source stubs created

## Phase 1 — Backend Core
- [ ] Install backend deps: `npm install --workspace=backend`
- [ ] Validate `simulator.ts` — unit-test GBM price series (mean-reversion check)
- [ ] Validate `aggregator.ts` — test VWAP against manual calculation
- [ ] Wire `server.ts` — start WS server, confirm ticks broadcast at correct interval
- [ ] Add `DepthSnapshot` generation to server (synthetic order book)
- [ ] Add latency echo: client sends `{ type:"ping", ts }`, server replies `{ type:"pong", ts }`

## Phase 2 — Frontend Core
- [ ] Install frontend deps: `npm install --workspace=frontend`
- [ ] Confirm Next.js dev server boots (`npm run dev --workspace=frontend`)
- [ ] Verify `useWebSocket` connects and populates the Zustand store
- [ ] Render `Watchlist` with live tick data
- [ ] Flash animation CSS — confirm green/red flash triggers on each price update
- [ ] `DepthWidget` — render for all symbols
- [ ] `EngineMetrics` — confirm FPS counter and ticks/sec display correctly

## Phase 3 — Performance & Polish
- [ ] Batch ticks client-side: accumulate 100 ms worth of updates, apply in one `setState`
- [ ] Measure React render time with Profiler — target < 2 ms per frame
- [ ] Add `React.memo` / `useMemo` where measured benefit is real
- [ ] Debounce OHLC bar updates in UI (5-second cadence is fine)
- [ ] Add virtualized scrolling (e.g. `@tanstack/react-virtual`) if > 50 symbols

## Phase 4 — Production Readiness
- [ ] Add `.env.example` with `WS_PORT` and `NEXT_PUBLIC_WS_URL`
- [ ] Docker Compose file for backend + frontend
- [ ] Add graceful shutdown handler to `server.ts` (SIGTERM → close all sockets)
- [ ] Add reconnection unit test (mock WS, simulate drop, assert backoff timing)
- [ ] CI pipeline (GitHub Actions): lint → type-check → build

## Phase 5 — Stretch Goals
- [ ] Candlestick chart component (Lightweight Charts or Victory)
- [ ] Historical replay mode (replay stored ticks at configurable speed)
- [ ] Multi-timeframe OHLC (1 min, 5 min, 1 hr windows in aggregator)
- [ ] Alerts: configurable price thresholds → toast notification
- [ ] Dark/light theme toggle
