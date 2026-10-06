using TradingDashboard.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddSingleton<WebSocketConnectionManager>();
builder.Services.AddSingleton<MarketSimulator>();
builder.Services.AddSingleton<OLHCBarService>();
builder.Services.AddHostedService<TickBroadcastService>();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod()));

var app = builder.Build();

app.UseCors();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

app.MapGet("/ws", async (HttpContext ctx, WebSocketConnectionManager wsManager, ILogger<Program> log) =>
{
    if (!ctx.WebSockets.IsWebSocketRequest)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsync("WebSocket connections only.");
        return;
    }

    var ws = await ctx.WebSockets.AcceptWebSocketAsync();

    await wsManager.HandleConnectionAsync(ws, log);
});

// ── Health check ──────────────────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new { status = "healthy", ts = DateTimeOffset.UtcNow }));

// ── Start ─────────────────────────────────────────────────────────────────────
app.Run("http://localhost:5000");

