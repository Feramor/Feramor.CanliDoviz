using HtmlAgilityPack;

namespace Feramor.CanliDoviz.Helpers;

public static class CanliDovizDovizKurlari
{
    public static async Task<Dictionary<int, string>> GetCidToCurrencyMapping(CancellationToken cancellationToken = default)
    {
        using (var httpClient = new HttpClient())
        {
            string url = "https://canlidoviz.com/doviz-kurlari";
            string html = await httpClient.GetStringAsync(url, cancellationToken);
        
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
        
            var cidToCurrency = htmlDoc.DocumentNode
                                    .SelectNodes("//tr[@itemprop='itemListElement']")
                                    ?.Select(row => new
                                    {
                                        Currency = row.SelectSingleNode(".//span[@itemprop='currency']")?.GetAttributeValue("content", null),
                                        Cid = row.SelectSingleNode(".//td[@itemprop='currentExchangeRate']//span[@itemprop='price']")?.GetAttributeValue("cid", null)
                                    })
                                    .Where(x => !string.IsNullOrEmpty(x.Cid) && !string.IsNullOrEmpty(x.Currency))
                                    .ToDictionary(x => Convert.ToInt32(x.Cid!), x => x.Currency!)
                                ?? new Dictionary<int, string>();
        
            return cidToCurrency;
        }
    }
}