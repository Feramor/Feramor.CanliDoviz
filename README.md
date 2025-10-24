
# Feramor.CanliDoviz

A real-time currency exchange rate client library for .NET that connects to CanliDoviz.com's WebSocket API to receive live currency updates.

[![.NET](https://img.shields.io/badge/.NET-5.0%2B-512BD4)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/badge/NuGet-Available-blue)](https://www.nuget.org/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

## Features

- 🔄 **Real-time Updates**: Receive live currency exchange rates via WebSocket connection
- 📊 **Price Change Tracking**: Automatic calculation of buy/sell price changes
- 🔌 **Auto-reconnection**: Built-in reconnection logic with configurable attempts
- 🎯 **Event-driven Architecture**: Subscribe to currency changes and connection events
- 🛡️ **Thread-safe**: Designed with concurrent operations in mind
- 📝 **Comprehensive Logging**: Built-in logging system with multiple log levels
- 🌐 **Multi-target Support**: Compatible with .NET Standard 2.0, 2.1, and .NET 5+

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package Feramor.CanliDoviz
```

Or via Package Manager Console:

```powershell
Install-Package Feramor.CanliDoviz
```

## Quick Start

### Basic Usage

```csharp
using Feramor.CanliDoviz.Client;
using Feramor.CanliDoviz.Models;

// Create a cancellation token source for graceful shutdown
var cts = new CancellationTokenSource();

// Initialize the client
using var client = new CanliDovizClient(cts.Token);

// Subscribe to currency change events
client.OnCurrencyChanged += (sender, data) =>
{
    Console.WriteLine($"Currency: {data.Symbol}");
    Console.WriteLine($"Buy Price: {data.BuyPrice}");
    Console.WriteLine($"Sell Price: {data.SellPrice}");
    Console.WriteLine($"Change: {data.BuyPriceChange}");
    Console.WriteLine($"Last Update: {data.LastUpdate}");
    Console.WriteLine("---");
};

// Subscribe to log events
client.OnLog += (sender, log) =>
{
    Console.WriteLine($"[{log.LogLevel}] {log.Date}: {log.Message}");
};

// Start the client
await client.StartAsync();

// Keep the application running
Console.WriteLine("Press Ctrl+C to exit...");
await Task.Delay(Timeout.Infinite, cts.Token);
```

### Advanced Usage with Custom Symbol Mapping

```csharp
// Pre-define currency symbols to monitor
var symbolMap = new Dictionary<int, string>
{
    { 1, "USD" },
    { 2, "EUR" },
    { 3, "GBP" }
};

using var client = new CanliDovizClient(symbolMap, cancellationToken);

client.OnCurrencyChanged += (sender, data) =>
{
    // Handle only the currencies you specified
    Console.WriteLine($"{data.Symbol}: {data.BuyPrice}/{data.SellPrice}");
};

await client.StartAsync();
```

### Handling Reconnection Failures

```csharp
client.OnReconnectFailed += (sender, args) =>
{
    Console.WriteLine("Failed to reconnect after maximum attempts!");
    // Implement your recovery logic here
    // e.g., send alert, restart application, etc.
};
```

## API Reference

### CanliDovizClient

The main client class for connecting to CanliDoviz.com's real-time currency data.

#### Constructors

```csharp
// Default constructor - fetches all available currencies
CanliDovizClient(CancellationToken cancellationToken = default)

// Constructor with custom symbol mapping
CanliDovizClient(Dictionary<int, string> symbolMap, CancellationToken cancellationToken = default)
```

#### Methods

| Method | Description |
|--------|-------------|
| `StartAsync()` | Starts the WebSocket connection and begins receiving currency updates |
| `Dispose()` | Disconnects and cleans up resources |

#### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnCurrencyChanged` | `EventHandler<CurrencyData>` | Fired when a currency rate is updated |
| `OnLog` | `EventHandler<Log>` | Fired when a log message is generated |
| `OnReconnectFailed` | `EventHandler` | Fired when reconnection attempts are exhausted |

### CurrencyData

Represents the current state of a currency exchange rate.

```csharp
public class CurrencyData
{
    public string? Symbol { get; set; }           // Currency symbol (e.g., "USD", "EUR")
    public decimal? BuyPrice { get; set; }        // Current buy price
    public decimal? SellPrice { get; set; }       // Current sell price
    public decimal? BuyPriceChange { get; set; }  // Change in buy price
    public decimal? SellPriceChange { get; set; } // Change in sell price
    public decimal? Value { get; set; }           // Additional value field
    public DateTime LastUpdate { get; set; }      // Timestamp of last update
}
```

### Log

Contains logging information with severity levels.

```csharp
public class Log
{
    public LogLevel LogLevel { get; }     // Severity level
    public DateTime Date { get; }         // Timestamp
    public string Message { get; }        // Log message
    public Exception? Exception { get; }  // Optional exception details
}
```

### LogLevel

Available log severity levels:

```csharp
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}
```

## Architecture

The library consists of several key components:

- **CanliDovizClient**: Main client managing WebSocket connections and event handling
- **CanliDovizDovizKurlari**: Helper class for fetching currency ID mappings from canlidoviz.com
- **CurrencyData**: Model representing currency exchange rate data
- **Log/LogLevel**: Logging infrastructure for monitoring client behavior

### Connection Flow

1. Client initializes with optional symbol mapping
2. If no mapping provided, automatically fetches available currencies
3. Establishes WebSocket connection to `s.canlidoviz.com`
4. Sends subscription request for specified currencies
5. Receives real-time updates via `"c"` event
6. Parses updates and fires `OnCurrencyChanged` events

## Configuration

### Reconnection Settings

The client includes built-in reconnection logic:

- **Reconnection Delay**: 1000ms (1 second)
- **Reconnection Attempts**: 5 attempts
- **Auto Reconnect**: Enabled by default

These settings are configured in the `SocketIOOptions` and can be modified in the constructor if needed.

## Error Handling

The library provides comprehensive error handling:

```csharp
client.OnLog += (sender, log) =>
{
    switch (log.LogLevel)
    {
        case LogLevel.Error:
        case LogLevel.Critical:
            // Handle critical errors
            Console.Error.WriteLine($"ERROR: {log.Message}");
            if (log.Exception != null)
            {
                Console.Error.WriteLine($"Exception: {log.Exception}");
            }
            break;
        case LogLevel.Warning:
            Console.WriteLine($"WARNING: {log.Message}");
            break;
        default:
            Console.WriteLine(log.Message);
            break;
    }
};
```

## Requirements

- .NET Standard 2.0, 2.1, .NET 5.0 or higher
- Internet connection for WebSocket communication
- Dependencies:
  - SocketIOClient
  - Newtonsoft.Json
  - HtmlAgilityPack

## Thread Safety

The client maintains internal dictionaries that are accessed from event handlers. While the WebSocket library handles thread safety for connection events, it's recommended to use appropriate synchronization when accessing shared resources in your event handlers.

## Performance Considerations

- The client maintains an in-memory cache of current rates for change calculation
- Event handlers are called synchronously - avoid long-running operations in handlers
- Consider using a message queue or background processor for heavy processing

## Examples

### Console Application (Tested)

There is an example project under [Feramor.CanliDoviz.TestConsole](https://github.com/Feramor/Feramor.CanliDoviz/tree/main/Feramor.CanliDoviz)

### ASP.NET Core Background Service (Not Tested AI Generated)

```csharp
public class CurrencyMonitorService : BackgroundService
{
    private readonly ILogger<CurrencyMonitorService> _logger;
    private CanliDovizClient? _client;

    public CurrencyMonitorService(ILogger<CurrencyMonitorService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client = new CanliDovizClient(stoppingToken);
        
        _client.OnCurrencyChanged += (sender, data) =>
        {
            _logger.LogInformation(
                "Currency updated: {Symbol} - Buy: {Buy}, Sell: {Sell}",
                data.Symbol, data.BuyPrice, data.SellPrice);
        };
        
        _client.OnLog += (sender, log) =>
        {
            _logger.Log(
                MapLogLevel(log.LogLevel),
                log.Exception,
                log.Message);
        };

        await _client.StartAsync();
    }

    private static Microsoft.Extensions.Logging.LogLevel MapLogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogLevel.Info => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            LogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => Microsoft.Extensions.Logging.LogLevel.Information
        };
    }

    public override void Dispose()
    {
        _client?.Dispose();
        base.Dispose();
    }
}
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Data provided by [CanliDoviz.com](https://canlidoviz.com)
- Built with [SocketIOClient](https://github.com/doghappy/socket.io-client-csharp)

## Support

If you encounter any issues or have questions:

- Open an issue on [GitHub](https://github.com/Feramor/Feramor.CanliDoviz/issues)
- Check existing issues for solutions

## Disclaimer

This library is not officially affiliated with CanliDoviz.com. Use at your own risk. The authors are not responsible for any financial decisions made based on data received through this library.

---

**Note**: Exchange rates are provided for informational purposes only. Always verify rates with official sources before making financial decisions.
