namespace TradingDashboard.Api.Models;

public sealed record TickEvent(
    string Symbol,
    long   Timestamp,
    double Price,
    int    Volume
);

