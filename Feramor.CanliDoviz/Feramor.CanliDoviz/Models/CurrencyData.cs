namespace Feramor.CanliDoviz.Models;

public class CurrencyData
{
    public string Symbol { get; set; }
    public decimal? BuyPrice { get; set; }
    public decimal? SellPrice { get; set; }
    public decimal? BuyPriceChange { get; set; }
    public decimal? SellPriceChange { get; set; }
    public decimal? Value { get; set; }
    public DateTime LastUpdate { get; set; }
}