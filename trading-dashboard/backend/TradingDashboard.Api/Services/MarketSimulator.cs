using TradingDashboard.Api.Models;

namespace TradingDashboard.Api.Services;

public sealed class MarketSimulator
{
    private static readonly InstrumentConfig[] Instruments =
    [
        new("NIFTY",     25180.5, 0.0025, 0.0008, 100),
        new("BANKNIFTY", 54200.0, 0.004,  0.0012, 100),
        new("RELIANCE",  2845.2,  0.003,  0.001,  100),
        new("HDFC",      1765.4,  0.0025, 0.0009, 100),
        new("INFY",      1522.1,  0.0035, 0.0011, 100),
        new("TCS",       3210.4,  0.0025, 0.0008, 100),
        new("Mphasis",   25180.5, 0.0025, 0.0008, 100),
        new("ABB",       54200.0, 0.004,  0.0012, 100),
        new("ADANI",     2845.2,  0.003,  0.001,  100),
        new("HEG",       1765.4,  0.0025, 0.0009, 100),
        new("TECHM",     1522.1,  0.0035, 0.0011, 100),
        new("VBL",       3210.4,  0.0025, 0.0008, 100),
    ];

    private readonly InstrumentConfigState[] _states;
    private readonly object _lock = new();

    public MarketSimulator()
    {
        _states = Instruments
            .Select(cfg => new InstrumentConfigState(cfg, 42u ^ (uint)cfg.Symbol[0]))
            .ToArray();
    }

    public IReadOnlyList<TickEvent> GenerateTicks()
    {
        lock (_lock)
        {
            var ticks = new TickEvent[_states.Length];
            for (int i = 0; i < _states.Length; i++)
                ticks[i] = NextTick(_states[i]);
            return ticks;
        }
    }

    private static double NextPrice(double current, InstrumentConfig cfg, ref uint seed)
    {
        double dt = cfg.IntervalMs / 1000.0;
        double drift = (cfg.StdDeviation - 0.5 * cfg.Volitality * cfg.Volitality) * dt;
        double diffusion = cfg.Volitality * Math.Sqrt(dt) * BoxMuller(ref seed);
        return current * Math.Exp(drift + diffusion);
    }

    private static double BoxMuller(ref uint seed)
    {
        double u1 = Rand(ref seed) + 1e-12;
        double u2 = Rand(ref seed);
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }

    private static double Rand(ref uint seed)
    {
        unchecked
        {
            uint t = seed += 0x6D2B79F5u;
            t = (t ^ (t >> 15)) * (t | 1u);
            t ^= t + ((t ^ (t >> 7)) * (t | 61u));
            return (double)((t ^ (t >> 14)) >> 0) / 4294967296.0;
        }
    }

    private static TickEvent NextTick(InstrumentConfigState s)
    {
        uint seed = s.Seed;
        double price = NextPrice(s.CurrentPrice, s.Config, ref seed);
        s.Seed = seed;
        s.CurrentPrice = price;

        int vol = 100 + (int)(Rand(ref seed) * 9900.0);
        s.Seed = seed;

        return new TickEvent(
            Symbol:    s.Config.Symbol,
            Timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Price:     Math.Round(price, 4),
            Volume:    vol
        );
    }
}
