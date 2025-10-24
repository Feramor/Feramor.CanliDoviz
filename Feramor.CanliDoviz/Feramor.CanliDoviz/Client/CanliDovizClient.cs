using Feramor.CanliDoviz.Helpers;
using Feramor.CanliDoviz.Models;
using Newtonsoft.Json;
using SocketIO.Core;
using SocketIOClient;

namespace Feramor.CanliDoviz.Client;

public class CanliDovizClient  : IDisposable
{
    private static readonly string Host = "https://s.canlidoviz.com/";
    
    private readonly SocketIOClient.SocketIO _client;
    private readonly Dictionary<int, string> _symbolMap;
    private readonly Dictionary<string, CurrencyData> _currentRates;
    private readonly CancellationToken _cancellationToken;
    
    public EventHandler<CurrencyData>? OnCurrencyChanged;
    public EventHandler<Log>? OnLog;
    public EventHandler? OnReconnectFailed;

    public CanliDovizClient(CancellationToken cancellationToken = default)
    {
        _client = new SocketIOClient.SocketIO(Host, new SocketIOOptions
        {
            EIO = EngineIO.V4,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            Reconnection = true,
            ReconnectionDelay = 1000,
            ReconnectionAttempts = 5
        });
        _symbolMap = new Dictionary<int, string>();
        _currentRates = new Dictionary<string, CurrencyData>();
        _cancellationToken  = cancellationToken;
    }
    
    public CanliDovizClient(Dictionary<int, string> symbolMap, CancellationToken cancellationToken = default)
    {
        _client = new SocketIOClient.SocketIO(Host, new SocketIOOptions
        {
            EIO = EngineIO.V4,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            Reconnection = true,
            ReconnectionDelay = 1000,
            ReconnectionAttempts = 5,
        });
        _symbolMap = symbolMap;
        _currentRates = new Dictionary<string, CurrencyData>();
        _cancellationToken  = cancellationToken;
    }
    
    public async Task StartAsync()
    {
        if (_symbolMap.Count == 0)
        {
            var data = await CanliDovizDovizKurlari.GetCidToCurrencyMapping(_cancellationToken);
            foreach (var item in data)
            {
                _symbolMap.TryAdd(item.Key, item.Value);
            }
        }
        SetupEventHandlers();
        _ = _client.ConnectAsync(_cancellationToken);
    }
    
    private async Task SendRequestedCurrencyList()
    {
        await _client.EmitAsync("us", new
        {
            t = new List<int>(),
            m = false,
            c = _symbolMap.Values,
        });
    }
    
    private void SetupEventHandlers()
    {
        _client.OnConnected += async (sender, e) =>
        {
            OnLog?.Invoke(this, new Log(DateTime.UtcNow, $"Connected - Socket ID: {_client.Id}"));
            await SendRequestedCurrencyList();
        };
        
        _client.OnReconnected += async (sender, e) =>
        {
            OnLog?.Invoke(this, new Log(DateTime.UtcNow, $"Reconnected - Socket ID: {_client.Id}"));
            await SendRequestedCurrencyList();
        };
        
        _client.OnDisconnected += (sender, e) =>
        {
            OnLog?.Invoke(this, new Log(DateTime.UtcNow, $"Disconnected: {e}", LogLevel.Warning));
        };

        _client.OnError += (sender, e) =>
        {
            if (OnLog is not null)
            {
                OnLog.Invoke(this, new Log(DateTime.UtcNow, $"Error: {e}", LogLevel.Error));
            }
            else
            {
                throw new Exception($"Error: {e}");
            }
        };

        _client.OnReconnectError += (sender, e) =>
        {
            if (OnLog is not null)
            {
                OnLog.Invoke(this, new Log(DateTime.UtcNow, $"Reconnect Error: {e.Message}", LogLevel.Error, e));
            }
            else
            {
                throw new Exception($"Reconnect Error: {e.Message}", e);
            }
        };

        _client.OnReconnectFailed += (sender, e) =>
        {
            OnReconnectFailed?.Invoke(this, EventArgs.Empty);
        };
        
        _client.On("c", response =>
        {
            var jsonString = response.ToString().Replace("[[","[").Replace("]]","]");
            var rates = JsonConvert.DeserializeObject<List<string>>(jsonString);
            if (rates == null) return;
            foreach (var rateData in rates.Select(ParseCurrencyUpdate).OfType<CurrencyData>())
            {
                OnCurrencyChanged?.Invoke(this, rateData);
            }
        });
    } 
    
    private CurrencyData? ParseCurrencyUpdate(string data)
    {
        var parts = data.Split('|');
        if (parts.Length < 3)
            return null;

        if (!int.TryParse(parts[0], out int symbolId))
            return null;

        if (!_symbolMap.TryGetValue(symbolId, out var symbol))
            return null;

        if (!_currentRates.TryGetValue(symbol, out var currencyData))
        {
            currencyData = new CurrencyData { Symbol = symbol };
            _currentRates.Add(symbol, currencyData);
        }

        currencyData.LastUpdate = DateTime.UtcNow;
        
        if (!string.IsNullOrEmpty(parts[1]) && decimal.TryParse(parts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal buyPrice))
        {
            var oldBuyPrice = currencyData.BuyPrice;
            if (oldBuyPrice.HasValue)
            {
                currencyData.BuyPriceChange = buyPrice - oldBuyPrice.Value;
            }
            else
            {
                currencyData.BuyPriceChange = decimal.Zero;
            }
            currencyData.BuyPrice = buyPrice;
        }

        if (parts.Length > 2 && !string.IsNullOrEmpty(parts[2]) && decimal.TryParse(parts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal sellPrice))
        {
            var oldSellPrice = currencyData.SellPrice;
            if (oldSellPrice.HasValue)
            {
                currencyData.SellPriceChange = sellPrice - oldSellPrice.Value;
            }
            else
            {
                currencyData.SellPriceChange = decimal.Zero;
            }
            currencyData.SellPrice = sellPrice;
        }

        if (currencyData.BuyPrice != null || currencyData.SellPrice != null || parts.Length <= 2 || string.IsNullOrEmpty(parts[2])) return currencyData;
        if (decimal.TryParse(parts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            currencyData.Value = value;
        }
        return currencyData;
    }

    public void Dispose()
    {
        _client?.DisconnectAsync().GetAwaiter().GetResult();
        _client?.Dispose();
    }
}