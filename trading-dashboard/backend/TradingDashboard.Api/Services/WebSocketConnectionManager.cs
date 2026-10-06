using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace TradingDashboard.Api.Services;

public sealed class WebSocketConnectionManager
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new();
    public int Count => _sockets.Count;

    public async Task BroadcastAsync(string json, CancellationToken ct = default)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        var dead = new List<Guid>();

        foreach (var (id, ws) in _sockets)
        {
            if (ws.State == WebSocketState.Open)
            {
                try
                {
                    await ws.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
                }
                catch (WebSocketException)
                {
                    dead.Add(id);
                }
            }
            else
            {
                dead.Add(id);
            }
        }

        foreach (var id in dead) _sockets.TryRemove(id, out _);
    }

    public async Task HandleConnectionAsync(WebSocket ws, ILogger logger)
    {
        var id = Guid.NewGuid();
        _sockets[id] = ws;
        logger.LogInformation("Client connected {Id}, total={Count}", id, _sockets.Count);

        var buf = new byte[4096];
        try
        {
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(buf, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) break;
            }
        }
        catch (WebSocketException ex)
        {
            logger.LogWarning("WS error {Id}: {Msg}", id, ex.Message);
        }
        finally
        {
            _sockets.TryRemove(id, out _);
            logger.LogInformation("Client disconnected {Id}, remaining={Count}", id, _sockets.Count);
            try { await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None); }
            catch { }
        }
    }
}
