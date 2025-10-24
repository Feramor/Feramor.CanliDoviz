# Feramor.CanliDoviz

A real-time currency exchange rate client library for .NET that connects to CanliDoviz.com's WebSocket API to receive live currency updates.

[![.NET](https://img.shields.io/badge/.NET-5.0%2B-512BD4)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/badge/NuGet-Available-blue)](https://www.nuget.org/packages/Feramor.CanliDoviz)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

## Features

- 🔄 **Real-time Updates**: Receive live exchange rates via WebSocket connection
- 💰 **Multiple Asset Types**: Support for currencies, gold, stocks, and cryptocurrencies
- 📊 **Price Change Tracking**: Automatic calculation of buy/sell price changes
- 🔌 **Auto-reconnection**: Built-in reconnection logic with configurable attempts
- ⚙️ **Flexible Configuration**: Customizable options for reconnection and asset type selection
- 🎯 **Event-driven Architecture**: Subscribe to rate changes and connection events
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

// Initialize the client with default options (currencies only)
using var client = new CanliDovizClient(cancellationToken: cts.Token);

// Subscribe to currency change events
client.OnCurrencyChanged += (sender, data) =>
{
    Console.WriteLine($"Symbol: {data.Symbol}");
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

### Advanced Usage with Options

```csharp
// Configure options to monitor multiple asset types
var options = new Options
{
    CurrencyList = CurrencyList.Currency | CurrencyList.Gold | CurrencyList.Crypto,
    Reconnection = true,
    ReconnectionDelay = 2000,
    ReconnectionAttempts = 10
};

using var client = new CanliDovizClient(options, cancellationToken);

client.OnCurrencyChanged += (sender, data) =>
{
    Console.WriteLine($"{data.Symbol}: {data.BuyPrice}/{data.SellPrice}");
};

await client.StartAsync();
```

### Monitor Specific Asset Types

```csharp
// Monitor only gold prices
var goldOptions = new Options
{
    CurrencyList = CurrencyList.Gold
};

using var goldClient = new CanliDovizClient(goldOptions, cancellationToken);

// Monitor currencies and stocks
var mixedOptions = new Options
{
    CurrencyList = CurrencyList.Currency | CurrencyList.Stock
};

using var mixedClient = new CanliDovizClient(mixedOptions, cancellationToken);

// Monitor everything
var allOptions = new Options
{
    CurrencyList = CurrencyList.All
};

using var allClient = new CanliDovizClient(allOptions, cancellationToken);
```

### Custom Symbol Mapping

```csharp
// Pre-define specific symbols to monitor
var symbolMap = new Dictionary<int, string>
{
    { 1, "USD" },
    { 2, "EUR" },
    { 3, "GBP" }
};

using var client = new CanliDovizClient(symbolMap, cancellationToken: cancellationToken);

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
// Default constructor - fetches currencies based on options
CanliDovizClient(Options? options = null, CancellationToken cancellationToken = default)

// Constructor with custom symbol mapping
CanliDovizClient(Dictionary<int, string> symbolMap, Options? options = null, CancellationToken cancellationToken = default)
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

### Options

Configuration options for the client.

```csharp
public class Options
{
    public bool Reconnection { get; set; } = true;           // Enable automatic reconnection
    public double ReconnectionDelay { get; set; } = 1000;    // Delay between reconnection attempts (ms)
    public int ReconnectionAttempts { get; set; } = 5;       // Maximum reconnection attempts
    public CurrencyList CurrencyList { get; set; } = CurrencyList.Currency;  // Asset types to monitor
}
```

### CurrencyList

Flags enum for selecting which asset types to monitor.

```csharp
[Flags]
public enum CurrencyList
{
    None = 0,
    Currency = 1,    // Foreign exchange currencies
    Gold = 2,        // Gold prices
    Stock = 4,       // Stock market indices
    Crypto = 8,      // Cryptocurrencies
    All = Currency | Gold | Stock | Crypto
}
```

**Usage Examples:**

```csharp
// Single type
var options = new Options { CurrencyList = CurrencyList.Currency };

// Multiple types using bitwise OR
var options = new Options { CurrencyList = CurrencyList.Currency | CurrencyList.Gold };

// All types
var options = new Options { CurrencyList = CurrencyList.All };
```

## Helper Methods

### CanliDovizMappings

Static helper class that provides methods to fetch symbol ID mappings from CanliDoviz.com. These methods are used internally by the client but can also be called directly if you need to retrieve mappings separately.

#### Available Methods

```csharp
// Get currency mappings (USD, EUR, GBP, etc.)
Task<Dictionary<int, string>> GetCurrencyMappings(CancellationToken cancellationToken = default)

// Get gold price mappings (gram gold, quarter gold, etc.)
Task<Dictionary<int, string>> GetGoldMappings(CancellationToken cancellationToken = default)

// Get stock market index mappings (BIST 100, XU030, etc.)
Task<Dictionary<int, string>> GetStockMappings(CancellationToken cancellationToken = default)

// Get cryptocurrency mappings (BTC, ETH, XRP, etc.)
Task<Dictionary<int, string>> GetCryptoMappings(CancellationToken cancellationToken = default)
```

#### Usage Example

```csharp
using Feramor.CanliDoviz.Helpers;

// Fetch currency mappings manually
var currencyMappings = await CanliDovizMappings.GetCurrencyMappings();
foreach (var mapping in currencyMappings)
{
    Console.WriteLine($"ID: {mapping.Key}, Symbol: {mapping.Value}");
}

// Use custom mappings with the client
var goldMappings = await CanliDovizMappings.GetGoldMappings();
var client = new CanliDovizClient(goldMappings);
await client.StartAsync();
```

## Architecture

The library consists of several key components:

- **CanliDovizClient**: Main client managing WebSocket connections and event handling
- **CanliDovizMappings**: Helper class providing methods to fetch symbol ID mappings for currencies, gold, stocks, and cryptocurrencies from canlidoviz.com
- **CurrencyData**: Model representing exchange rate data for any asset type
- **Options**: Configuration class for client behavior and asset type selection
- **CurrencyList**: Flags enum for selecting which asset types to monitor
- **Log/LogLevel**: Logging infrastructure for monitoring client behavior

### Connection Flow

1. Client initializes with optional configuration and symbol mapping
2. If no custom mapping provided, automatically fetches symbols based on `CurrencyList` option:
   - Currencies from `https://canlidoviz.com/doviz-kurlari`
   - Gold prices from `https://canlidoviz.com/altin-fiyatlari`
   - Stocks from `https://canlidoviz.com/borsa`
   - Cryptocurrencies from `https://canlidoviz.com/kripto-paralar`
3. Establishes WebSocket connection to `s.canlidoviz.com`
4. Sends subscription request with selected asset types
5. Receives real-time updates via `"c"` event
6. Parses updates and fires `OnCurrencyChanged` events

## Configuration

### Options Class

Configure the client behavior using the `Options` class:

```csharp
var options = new Options
{
    // Enable/disable automatic reconnection
    Reconnection = true,
    
    // Delay between reconnection attempts in milliseconds
    ReconnectionDelay = 1000,
    
    // Maximum number of reconnection attempts
    ReconnectionAttempts = 5,
    
    // Asset types to monitor (using flags)
    CurrencyList = CurrencyList.Currency | CurrencyList.Gold
};

var client = new CanliDovizClient(options, cancellationToken);
```

### Default Settings

If no options are provided, the client uses these defaults:

- **Reconnection**: Enabled
- **Reconnection Delay**: 1000ms (1 second)
- **Reconnection Attempts**: 5 attempts
- **Currency List**: `CurrencyList.Currency` (currencies only)

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

There is a complete tested working example in the [Feramor.CanliDoviz.TestConsole](https://github.com/Feramor/Feramor.CanliDoviz/tree/main/Feramor.CanliDoviz.TestConsole) project.

**Key features demonstrated:**
- Monitoring all asset types (Currency, Gold, Stock, Crypto)
- Real-time price updates with spread calculation
- Buy/Sell price change tracking
- Graceful shutdown with Ctrl+C or 'Q' key
- Comprehensive logging with timestamps
- Reconnection handling

**Quick example:**
```csharp
var options = new Options()
{
    Reconnection = true,
    ReconnectionAttempts = 10,
    ReconnectionDelay = 5000,
    CurrencyList = CurrencyList.All
};

var client = new CanliDovizClient(options: options, cancellationToken: cts.Token);

client.OnCurrencyChanged += (sender, currency) =>
{
    if (currency.BuyPrice.HasValue && currency.SellPrice.HasValue)
    {
        Console.WriteLine($"{currency.Symbol}: Buy {currency.BuyPrice:F4} | Sell {currency.SellPrice:F4}");
    }
};

await client.StartAsync();
```

### Simple Usage Example (Not Tested AI Generated)

For minimal setup with default options:

```csharp
using Feramor.CanliDoviz.Client;
using Feramor.CanliDoviz.Models;

var cts = new CancellationTokenSource();

// Default options: monitors currencies only
using var client = new CanliDovizClient(cancellationToken: cts.Token);

client.OnCurrencyChanged += (sender, data) =>
{
    if (data.BuyPrice.HasValue && data.SellPrice.HasValue)
    {
        Console.WriteLine($"{data.Symbol}: {data.BuyPrice:N4} / {data.SellPrice:N4}");
    }
};

client.OnLog += (sender, log) =>
{
    Console.WriteLine($"[{log.LogLevel}] {log.Message}");
};

await client.StartAsync();
await Task.Delay(Timeout.Infinite, cts.Token);
```

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
        var options = new Options
        {
            CurrencyList = CurrencyList.All,
            ReconnectionAttempts = 10
        };

        _client = new CanliDovizClient(options, stoppingToken);
        
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
