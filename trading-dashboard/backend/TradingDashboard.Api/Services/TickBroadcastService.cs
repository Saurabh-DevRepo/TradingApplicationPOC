using System.Text.Json;

namespace TradingDashboard.Api.Services;

public sealed class TickBroadcastService(
    WebSocketConnectionManager manager,
    MarketSimulator simulator,
    OLHCBarService aggregator,
    ILogger<TickBroadcastService> logger)
    : BackgroundService
{
    private int _tickCount = 0;

    protected override Task ExecuteAsync(CancellationToken ct)
        => Task.WhenAll(TickLoop(ct), OhlcLoop(ct), MetricsLoop(ct));

    private async Task TickLoop(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
        while (await timer.WaitForNextTickAsync(ct))
        {
            foreach (var tick in simulator.GenerateTicks())
            {
                aggregator.Ingest(tick);
                Interlocked.Increment(ref _tickCount);
                await manager.BroadcastAsync(Wrap("tick", tick), ct);
            }
        }
    }

    private async Task OhlcLoop(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (await timer.WaitForNextTickAsync(ct))
        {
            foreach (var sym in aggregator.Symbols)
            {
                var bar = aggregator.GetOHLC(sym);
                if (bar != null)
                    await manager.BroadcastAsync(Wrap("ohlc", bar), ct);
            }
        }
    }

    private async Task MetricsLoop(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (await timer.WaitForNextTickAsync(ct))
        {
            var tps = Interlocked.Exchange(ref _tickCount, 0);
            logger.LogInformation("ticks/s={Tps} clients={C}", tps, manager.Count);
        }
    }

    private static string Wrap<T>(string type, T data)
        => JsonSerializer.Serialize(new { type, data },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
}
