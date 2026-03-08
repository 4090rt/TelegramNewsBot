using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace TelegramNewsBot.RequestAndParcing.ModelBse
{
    public class ModelCrypto
    {
        [JsonPropertyName("bitcoin")]
        public CryptoItem Bitcoin { get; set; } = null!;

        [JsonPropertyName("ethereum")]
        public CryptoItem Ethereum { get; set; } = null!;

        [JsonPropertyName("tether")]
        public CryptoItem Tether { get; set; } = null!;

        public int Count => GetCryptoItems().Count;

        public decimal GetRate(string cryptoCode)
        {
            return cryptoCode.ToLower() switch
            {
                "bitcoin" => Bitcoin.Rub,
                "btc" => Bitcoin.Rub,
                "ethereum" => Ethereum.Rub,
                "eth" => Ethereum.Rub,
                "tether" => Tether.Rub,
                "usdt" => Tether.Rub,
                _ => 0
            };
        }

        public List<CryptoItem> GetCryptoItems()
        {
            return new List<CryptoItem>
            {
                new CryptoItem { Code = "BTC", Name = "Bitcoin", Rub = Bitcoin.Rub },
                new CryptoItem { Code = "ETH", Name = "Ethereum", Rub = Ethereum.Rub },
                new CryptoItem { Code = "USDT", Name = "Tether", Rub = Tether.Rub }
            };
        }
    }

    public class CryptoItem
    {
        [JsonPropertyName("rub")]
        public decimal Rub { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }
}
