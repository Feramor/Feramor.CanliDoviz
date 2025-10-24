namespace Feramor.CanliDoviz.Models;

public class Options
{
    public bool Reconnection { get; set; } = true;
    public double ReconnectionDelay { get; set; } = 1000;
    public int ReconnectionAttempts { get; set; } = 5;
    public CurrencyList CurrencyList { get; set; } = CurrencyList.Currency;
}