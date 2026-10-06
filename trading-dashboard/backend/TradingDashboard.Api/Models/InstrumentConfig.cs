namespace TradingDashboard.Api.Models;

public sealed record InstrumentConfig(
    string Symbol,
    double InitialPrice,
    double StdDeviation,
    double Volitality,
    int    IntervalMs
);


public sealed class InstrumentConfigState
{
    public double BasePrice { get; set; }
    public InstrumentConfig Config       { get; init; }
    public double          CurrentPrice { get; set; }
    public uint            Seed         { get; set; }

    public InstrumentConfigState(InstrumentConfig config, uint seed)
    {
        Config       = config;
        CurrentPrice = config.InitialPrice;
        BasePrice   = config.InitialPrice;
        Seed         = seed;
    }
}
