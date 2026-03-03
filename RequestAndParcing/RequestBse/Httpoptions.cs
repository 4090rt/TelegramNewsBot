using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot.RequestAndParcing.RequestBse
{
    public delegate HttpRequestMessage Optionshttp20(string url);
    public class Httpoptions
    {
        public HttpRequestMessage OptionsComleted(Func<string, HttpRequestMessage> delegat, string url)
        {
            var dele = delegat?.Invoke(url);
            return dele;
        }

        public HttpRequestMessage Options(string url)
        {
            var options = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Version = HttpVersion.Version20,
                VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
            };
            return options;
        }
    }
}
