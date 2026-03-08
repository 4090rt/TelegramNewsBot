using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TelegramNewsBot.RequestAndParcing.ModelBse
{
    public class ModelValute
    {
        [JsonPropertyName("base_code")]
        public string BaseCode { get; set; } = null!;

        [JsonPropertyName("conversion_rates")]
        public Dictionary<string, decimal> ConversionRates { get; set; } = new Dictionary<string, decimal>();

        public int Count => ConversionRates.Count;

        public decimal GetRate(string currencyCode)
        {
            if (ConversionRates.TryGetValue(currencyCode, out var rate))
            {
                return rate;
            }
            return 0;
        }

        public List<ValuteItem> GetValuteItems()
        {
            return ConversionRates
                .Select(kvp => new ValuteItem { Code = kvp.Key, Rate = kvp.Value })
                .ToList();
        }
    }

    public class ValuteItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = null!;

        [JsonPropertyName("rate")]
        public decimal Rate { get; set; }
    }
}
