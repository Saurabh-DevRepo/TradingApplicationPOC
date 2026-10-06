"use client";

import React from "react";
import { useMarketStore, selectAllSymbols } from "@/store/marketStore";
import TickerRow from "./TickerRow";

export default function Watchlist() {
  const symbols = useMarketStore(selectAllSymbols);

  if (symbols.length === 0) {
    return <p style={{ color: "#888", padding: "20px" }}>Waiting for data...</p>;
  }

  return (
    <table>
      <thead>
        <tr>
          <th>Symbol</th>
          <th>Price</th>
          <th>Change</th>
          <th>Volume</th>
          <th>VWAP</th>
          <th>O/H/L/C</th>
        </tr>
      </thead>
      <tbody>
        {symbols.map((symbol) => (
          <TickerRow key={symbol} symbol={symbol} />
        ))}
      </tbody>
    </table>
  );
}
