using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using TelegramNewsBot.RequestAndParcing.ModelBse;

namespace TelegramNewsBot.RequestAndParcing.ParsedBase
{
    public class ParsedClass
    {
        private readonly ILogger<ParsedClass> _logger;

        public ParsedClass(ILogger<ParsedClass> logger)
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
                                //Description = item.Summary?.Text,
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

        public async Task<T> ParsedApi<T>(Stream json) where T : new()
        {
            try
            {
                if (json != null)
                {
                    var deserialize1 = await JsonSerializer.DeserializeAsync<Dictionary<string, JsonElement>>(json);

                    if (deserialize1 != null)
                    {
                        foreach (var des in deserialize1)
                        {
                            Console.WriteLine($"{des.Key}: {des.Value.GetRawText()}");
                        }

                        T result = new T();
                        Type type = typeof(T);

                        foreach (var kvp in deserialize1)
                        {
                            PropertyInfo prop = type.GetProperty(kvp.Key,
                         BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                            if (prop != null && prop.CanWrite)
                            {
                                object ob = JsonSerializer.Deserialize(kvp.Value.GetRawText(), prop.PropertyType);
                                prop.SetValue(result, ob);
                                _logger.LogInformation("Десериализаванно успешно");
                            }
                            else
                            {
                                _logger.LogWarning("Свойства не найдены");
                            }
                        }
                        return result;
                    }
                    else
                    {
                        _logger.LogWarning("Объект пуст");
                        return default;
                    }
                }
                else
                {
                    _logger.LogWarning("Поток данных пуст");
                    return default;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message);
                return default;
            }
        }
        public async Task<List<ModelClassRss>> ParseRssReserve(Stream stream)
        {
            if (stream == null || stream.Length == 0)
            {
                _logger.LogError("Поток пуст");
                return new List<ModelClassRss>();
            }
            try
            {
                var settings = new XmlReaderSettings
                { 
                    Async = true, // async чтение
                    IgnoreComments = true, // игнорирование комент.
                    IgnoreWhitespace = true, // игнорирование лишних проб
                    DtdProcessing = DtdProcessing.Ignore,// игнорируем dtd
                    MaxCharactersFromEntities = 1024 // Ограничиваем размер сущностей
                };

                var items = new List<ModelClassRss>();

                using (var reader = XmlReader.Create(stream, settings))
                {
                    if (!await reader.ReadAsync())
                    {
                        _logger.LogWarning("Не удалось прочитать XML поток");
                        return new List<ModelClassRss>();
                    }
                    if (reader != null)
                    {
                        var loader = SyndicationFeed.Load(reader);

                        if (loader == null || !loader.Items.Any())
                        {
                            _logger.LogDebug("RSS лента пуста или не содержит элементов");
                            return new List<ModelClassRss>();
                        }
                        foreach (var item in loader.Items)
                        {
                            items.Add(new ModelClassRss
                            {
                                Title = item.Title?.Text,
                                Link = item.Links.FirstOrDefault()?.Uri,
                                //Description = item.Summary?.Text,
                                PublisDate = item.PublishDate,
                                ID = item.Id
                            });
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Лента пуста");
                        return new List<ModelClassRss>();
                    }
                }
                return items;
            }
            catch (XmlException xmlEx)
            {
                _logger.LogError(xmlEx, "Ошибка парсинга XML: {Message}", xmlEx.Message);
                throw new InvalidDataException("Некорректный формат RSS ленты", xmlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError("Возниклои исключение при парсинге ленты" + ex.Message);
                return new List<ModelClassRss>();
            }
        }
    }
}
