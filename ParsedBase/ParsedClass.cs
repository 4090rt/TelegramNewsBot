using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TelegramNewsBot.ModelBse;

namespace TelegramNewsBot.ParsedBase
{
    public class ParsedClass
    {
        private readonly Microsoft.Extensions.Logging.ILogger<ParsedClass> _logger;

        public ParsedClass(Microsoft.Extensions.Logging.ILogger<ParsedClass> logger)
        { 
            _logger = logger;
        }

        public async Task<List<ModelClassRss>> ParseRss(Stream stream)
        {
            try
            {
                var items = new List<ModelClassRss>();

                using (var reader = XmlReader.Create(stream))
                {
                    if (reader != null)
                    {
                        var fedd = SyndicationFeed.Load(reader);

                        foreach (var item in fedd.Items)
                        {
                            items.Add(new ModelClassRss
                            {
                                Title = item.Title?.Text,
                                Link = item.Links.FirstOrDefault()?.Uri,
                                Description = item.Summary?.Text,
                                PublisDate = item.PublishDate,
                                ID = item.Id
                            });
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Лента пуста");
                        return null;
                    }
                }
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возниклои исключение при парсинге ленты" + ex.Message);
                return null;
            }
        }
    }
}
