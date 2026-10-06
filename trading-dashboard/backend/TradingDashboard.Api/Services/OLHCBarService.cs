using TradingDashboard.Api.Models;

namespace TradingDashboard.Api.Services;

public sealed class OLHCBarService : IDisposable
{
    private record struct Bar(long Timestamp, double Price, int Volume);

    private readonly TimeSpan _window;
    private readonly Dictionary<string, List<Bar>> _data = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public OLHCBarService(int windowMs = 60000)
    {
        _window = TimeSpan.FromMilliseconds(windowMs);
    }

    public void Ingest(TickEvent tick)
    {
        long cutoff = tick.Timestamp - (long)_window.TotalMilliseconds;

        _lock.EnterWriteLock();
        try
        {
            if (!_data.TryGetValue(tick.Symbol, out var list))
                _data[tick.Symbol] = list = new List<Bar>();

            list.Add(new Bar(tick.Timestamp, tick.Price, tick.Volume));

            // evict old ticks outside the window
            int i = 0;
            while (i < list.Count && list[i].Timestamp < cutoff) i++;
            if (i > 0) list.RemoveRange(0, i);
        }
        finally { _lock.ExitWriteLock(); }
    }

    public OHLCBar? GetOHLC(string symbol)
    {
        _lock.EnterReadLock();
        try
        {
            if (!_data.TryGetValue(symbol, out var list) || list.Count == 0)
                return null;

            double open = list[0].Price, close = list[^1].Price;
            double high = double.MinValue, low = double.MaxValue;
            long totalVol = 0;
            double vwapNum = 0;

            foreach (var b in list)
            {
                if (b.Price > high) high = b.Price;
                if (b.Price < low) low = b.Price;
                totalVol += b.Volume;
                vwapNum += b.Price * b.Volume;
            }

            return new OHLCBar(
                symbol, list[0].Timestamp, list[^1].Timestamp,
                Math.Round(open, 4), Math.Round(high, 4),
                Math.Round(low, 4), Math.Round(close, 4),
                totalVol, Math.Round(totalVol > 0 ? vwapNum / totalVol : close, 4)
            );
        }
        finally { _lock.ExitReadLock(); }
    }

    public IReadOnlyList<string> Symbols
    {
        get
        {
            _lock.EnterReadLock();
            try { return _data.Keys.ToList(); }
            finally { _lock.ExitReadLock(); }
        }
    }

    public void Dispose() => _lock.Dispose();
}
