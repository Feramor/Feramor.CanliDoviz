using HtmlAgilityPack;

namespace Feramor.CanliDoviz.Helpers;

public static class CanliDovizMappings
{
    public static async Task<Dictionary<int, string>> GetCurrencyMappings(CancellationToken cancellationToken = default)
    {
        using (var httpClient = new HttpClient())
        {
            string url = "https://canlidoviz.com/doviz-kurlari";
            #if NETSTANDARD2_0 || NETSTANDARD2_1
                string html = await httpClient.GetStringAsync(url);
            #else
                string html = await httpClient.GetStringAsync(url, cancellationToken);
            #endif
            
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
        
            var cidToCurrency = htmlDoc.DocumentNode
                                    .SelectNodes("//tr[@itemprop='itemListElement']")
                                    ?.Select(row => new
                                    {
                                        Currency = row.SelectSingleNode(".//span[@itemprop='currency']")?.GetAttributeValue("content", string.Empty),
                                        Cid = row.SelectSingleNode(".//td[@itemprop='currentExchangeRate']//span[@itemprop='price']")?.GetAttributeValue("cid", string.Empty)
                                    })
                                    .Where(x => !string.IsNullOrEmpty(x.Cid) && !string.IsNullOrEmpty(x.Currency))
                                    .ToDictionary(x => Convert.ToInt32(x.Cid!), x => x.Currency!)
                                ?? new Dictionary<int, string>();
        
            return cidToCurrency;
        }
    }
    
    public static async Task<Dictionary<int, string>> GetGoldMappings(CancellationToken cancellationToken = default)
    {
        using (var httpClient = new HttpClient())
        {
            string url = "https://canlidoviz.com/altin-fiyatlari";
            #if NETSTANDARD2_0 || NETSTANDARD2_1
                string html = await httpClient.GetStringAsync(url);
            #else
                string html = await httpClient.GetStringAsync(url, cancellationToken);
            #endif
            
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
        
            var cidToCurrency = htmlDoc.DocumentNode
                                    .SelectNodes("//tr[@itemprop='itemListElement']")
                                    ?.Select(row => new
                                    {
                                        Currency = row.SelectSingleNode(".//span[@itemprop='currency']")?.GetAttributeValue("content", string.Empty),
                                        Cid = row.SelectSingleNode(".//td[@itemprop='currentExchangeRate']//span[@itemprop='price']")?.GetAttributeValue("cid", string.Empty)
                                    })
                                    .Where(x => !string.IsNullOrEmpty(x.Cid) && !string.IsNullOrEmpty(x.Currency))
                                    .ToDictionary(x => Convert.ToInt32(x.Cid!), x => x.Currency!)
                                ?? new Dictionary<int, string>();
        
            return cidToCurrency;
        }
    }
    
    public static async Task<Dictionary<int, string>> GetStockMappings(CancellationToken cancellationToken = default)
    {
        using (var httpClient = new HttpClient())
        {
            string url = "https://canlidoviz.com/borsa";
            #if NETSTANDARD2_0 || NETSTANDARD2_1
                string html = await httpClient.GetStringAsync(url);
            #else
                string html = await httpClient.GetStringAsync(url, cancellationToken);
            #endif
            
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
        
            var cidToCurrency = htmlDoc.DocumentNode
                                    .SelectNodes("//tr[@itemprop='itemListElement']")
                                    ?.Select(row => new
                                    {
                                        Currency = row.SelectSingleNode(".//span[@itemprop='currency']")?.GetAttributeValue("content", string.Empty),
                                        Cid = row.SelectSingleNode(".//td[@itemprop='currentExchangeRate']//span[@itemprop='price']")?.GetAttributeValue("cid", string.Empty)
                                    })
                                    .Where(x => !string.IsNullOrEmpty(x.Cid) && !string.IsNullOrEmpty(x.Currency))
                                    .ToDictionary(x => Convert.ToInt32(x.Cid!), x => x.Currency!)
                                ?? new Dictionary<int, string>();
        
            return cidToCurrency;
        }
    }
    
    public static async Task<Dictionary<int, string>> GetCryptoMappings(CancellationToken cancellationToken = default)
    {
        using (var httpClient = new HttpClient())
        {
            string url = "https://canlidoviz.com/kripto-paralar";
            #if NETSTANDARD2_0 || NETSTANDARD2_1 
                string html = await httpClient.GetStringAsync(url);
            #else 
                string html = await httpClient.GetStringAsync(url, cancellationToken);
            #endif
            
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
        
            var cidToCurrency = htmlDoc.DocumentNode
                                    .SelectNodes("//tr[@itemprop='itemListElement']")
                                    ?.Select(row => new
                                    {
                                        Currency = row.SelectSingleNode(".//span[@itemprop='currency']")?.GetAttributeValue("content", string.Empty),
                                        Cid = row.SelectSingleNode(".//td[@itemprop='currentExchangeRate']//span[@itemprop='price']")?.GetAttributeValue("cid", string.Empty)
                                    })
                                    .Where(x => !string.IsNullOrEmpty(x.Cid) && !string.IsNullOrEmpty(x.Currency))
                                    .ToDictionary(x => Convert.ToInt32(x.Cid!), x => x.Currency!)
                                ?? new Dictionary<int, string>();
        
            return cidToCurrency;
        }
    }
}