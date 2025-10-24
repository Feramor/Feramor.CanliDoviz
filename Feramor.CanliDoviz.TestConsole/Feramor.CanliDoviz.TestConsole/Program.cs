using Feramor.CanliDoviz.Client;
using Feramor.CanliDoviz.Models;

namespace Feramor.CanliDoviz.TestConsole;

class Program
{ 
    private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         Currency Exchange WebSocket Client (canlidoviz)        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var options = new Options()
        {
            Reconnection = true,
            ReconnectionAttempts = 10,
            ReconnectionDelay = 5000,
            CurrencyList = CurrencyList.All
        };
        
        var client = new CanliDovizClient(options:options, cancellationToken:_cancellationTokenSource.Token);
        client.OnLog += (sender, log) =>
        {
            Console.WriteLine($"[{log.Date:HH:mm:ss.fff}] [{log.LogLevel}] {log.Message} {(log.Exception is not null ? $"{log.Exception.Message}" : "")}");
        };
        client.OnCurrencyChanged += (sender, currency) =>
        {
            DisplayCurrencyUpdate(currency);
        };
        client.OnReconnectFailed  += (sender, error) =>
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] [{LogLevel.Error}] Reconnect failed");

        };
        
        var clientTask = client.StartAsync();
        var inputTask = Task.Run(() => HandleUserInput(_cancellationTokenSource));
        
        Console.CancelKeyPress += async (sender, eventArgs) =>
        {
            _ = _cancellationTokenSource.CancelAsync();
        };
        try
        {
            await Task.Delay(Timeout.Infinite, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Ctrl+C or Q detected, application shutting down gracefully...");
        }
    }
    
    private static void HandleUserInput(CancellationTokenSource cancellationTokenSource)
    {
        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                
                if (key.Key == ConsoleKey.Q)
                {
                    _ = _cancellationTokenSource.CancelAsync();
                    return;
                }
            }
            
            Thread.Sleep(100);
        }
    }
    
    private static void DisplayCurrencyUpdate(CurrencyData data)
    {
        string priceInfo;
        if (data.BuyPrice.HasValue && data.SellPrice.HasValue)
        {
            priceInfo = $"Buy: {data.BuyPrice:F4} | Change (Buy) : {data.BuyPriceChange:F4} | Sell: {data.SellPrice:F4} | Change (Sell) : {data.SellPriceChange:F4} | Spread: {(data.SellPrice - data.BuyPrice):F4}";
        }
        else if (data.Value.HasValue)
        {
            priceInfo = $"Value: {data.Value:F2}";
        }
        else if (data.BuyPrice.HasValue)
        {
            priceInfo = $"Buy: {data.BuyPrice:F4}";
        }
        else if (data.SellPrice.HasValue)
        {
            priceInfo = $"Sell: {data.SellPrice:F4}";
        }
        else
        {
            return;
        }

        Console.WriteLine($"[{data.LastUpdate:HH:mm:ss.fff}] [{LogLevel.Info}] {data.Symbol,-12} {priceInfo}");
    }
}