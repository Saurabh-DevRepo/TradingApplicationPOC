namespace TradingDashboard.Api.Models;

public sealed record OHLCBar(
    string Symbol,
    long   OpenTime,
    long   CloseTime,
    double Open,
    double High,
    double Low,
    double Close,
    long   Volume,
    double Vwap
);

