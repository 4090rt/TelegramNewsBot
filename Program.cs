// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System.Runtime;
using TelegramNewsBot.ParsedBase;
using TelegramNewsBot.RequestBse;

var service = new ServiceCollection();

service.AddLogging(building =>
{
    building.AddConsole();
    building.SetMinimumLevel(LogLevel.Information);
});

service.AddHttpClient("RssCLient", rssclient =>
{
    rssclient.Timeout = TimeSpan.FromSeconds(60);
    rssclient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    rssclient.DefaultRequestHeaders.Accept.ParseAdd("application/xml, text/xml, */*");
    rssclient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
    rssclient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
}).AddTransientHttpErrorPolicy(poly =>
poly.WaitAndRetryAsync(3, retry =>
TimeSpan.FromSeconds(Math.Pow(2, retry))));

service.AddHttpClient("ApiClient", apiclient =>
{
    apiclient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    apiclient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    apiclient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
}).AddTransientHttpErrorPolicy(poly =>
poly.WaitAndRetryAsync(3, retry =>
TimeSpan.FromSeconds(Math.Pow(2, retry))));

service.AddScoped<RssRequests>();
service.AddScoped<ParsedClass>();
//service.AddScoped<>();

var serviceProvider = service.BuildServiceProvider();

string url = "https://tass.com/rss/v2.xml";

using var scope = serviceProvider.CreateScope();
var main = scope.ServiceProvider.GetRequiredService<RssRequests>();
Stream result = await main.RssRequestsMethod(url);

using var scope1 = serviceProvider.CreateScope();
var Reflect = scope1.ServiceProvider.GetRequiredService<ParsedClass>();
var resultt = await Reflect.ParseRss(result);

if (resultt != null)
{
    foreach (var item in resultt)
    {
        Console.WriteLine($"Title: {item.Title}");
        Console.WriteLine($"Link: {item.Link}");
        Console.WriteLine($"Description: {item.Description}");
        Console.WriteLine($"PublishDate: {item.PublisDate}");
        Console.WriteLine($"ID: {item.ID}");
        Console.WriteLine(new string('-', 40));
    }
}