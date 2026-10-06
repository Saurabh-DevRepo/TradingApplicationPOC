"use client";

import React from "react";
import { useMarketStore, selectTicker } from "@/store/marketStore";

export default function TickerRow({ symbol }: { symbol: string }) {
  const ticker = useMarketStore(selectTicker(symbol));

  if (!ticker) return null;

  const { latest, prevPrice, ohlc } = ticker;
  const isUp = latest.price >= prevPrice;
  const colorClass = isUp ? "up" : "down";

  return (
    <tr>
      <td><b>{symbol}</b></td>
      <td className={colorClass}>{latest.price.toFixed(2)}</td>
      <td className={colorClass}>
        {isUp ? "▲" : "▼"} {Math.abs(latest.price - prevPrice).toFixed(2)}
      </td>
      <td>{latest.volume.toLocaleString()}</td>
      {ohlc ? (
        <>
          <td>{ohlc.vwap.toFixed(2)}</td>
          <td>{ohlc.open.toFixed(2)}/{ohlc.high.toFixed(2)}/{ohlc.low.toFixed(2)}/{ohlc.close.toFixed(2)}</td>
        </>
      ) : (
        <>
          <td>-</td>
          <td>-</td>
        </>
      )}
    </tr>
  );
}
