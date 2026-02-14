using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot.RequestAndParcing.ModelBse
{
    public class ModelCrypto
    {
        public CurrencyRate bitcoin { get; set; }
        public CurrencyRate ethereum { get; set; }
        public CurrencyRate tether { get; set; }
    }
    public class CurrencyRate
    {
        public decimal rub { get; set; }
    }
}
