using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot.RequestAndParcing.ModelBse
{
    public class ModelClassRss
    {
        public string Title { get; set; }
        public Uri Link { get; set; }
        public string Description { get; set; }
        public DateTimeOffset PublisDate { get; set; }
        public string ID { get; set; }
        public List<string> Categories { get; set; }
        public string Author { get; set; }
        public Uri ImageUrl { get; set; }
    }
}
