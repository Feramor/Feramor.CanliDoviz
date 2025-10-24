namespace Feramor.CanliDoviz.Models;

[Flags]
public enum CurrencyList
{
    None = 0,
    Currency = 1,
    Gold = 2,
    Stock = 4,
    Crypto = 8,
    All = Currency | Gold | Stock | Crypto
}