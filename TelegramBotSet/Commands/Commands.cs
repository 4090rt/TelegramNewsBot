using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bots.Types;
using TelegramNewsBot.TelegramBotSet.InkineButtons;
using TelegramNewsBot.TelegramBotSet.ModelsTg;

namespace TelegramNewsBot.TelegramBotSet.Commands
{
    public class Commands
    {
        private readonly ITelegramBotClient _botClient;
        private readonly BotConfigModel _botConfig;
        private readonly Dictionary<long, UserDataModel> _userSession;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public Commands(ITelegramBotClient botClient, BotConfigModel config, Microsoft.Extensions.Logging.ILogger logger)
        {
            _botClient = botClient;
            _botConfig = config;
            _userSession = new Dictionary<long, UserDataModel>();
            _logger = logger;
        }

        public async Task FabricCommand(long chatId, string command, CancellationToken cancellationToken)
        {
            switch (command)
            {
                case "/MainCommands":
                    await MainCommand(chatId, cancellationToken);
                    break;
                case "/start":
                    InlineButtons inl = new InlineButtons(_botClient);
                    await inl.InlineButtonss(chatId, cancellationToken);
                    break;
                    
            }
        }

        public async Task MainCommand(long chatId,CancellationToken cancellationToken)
        {
            await _botClient.SendTextMessageAsync
                (
                    chatId: chatId,
                    text: "Ищем самые последние новости!",
                    cancellationToken: cancellationToken
                );

            Program prog = new Program();
            var resultat = await prog.Httprogram();
            if (resultat != null)
            {
                var messagecount = 1;
                var newstextbuilder = new StringBuilder();
                const int maxsize = 4000;

                foreach (var news in resultat)
                {
                    var newsText = $"📰 *{news.Title}*\n🔗 {news.Link}\n⏰ {news.PublisDate}\n\n";

                    if (newstextbuilder.Length + newsText.Length > maxsize)
                    {
                        await _botClient.SendTextMessageAsync
                            (
                                chatId: chatId,
                                text: newstextbuilder.ToString(),
                                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );

                        newstextbuilder.Clear();
                        newstextbuilder.AppendLine($"*Часть {messagecount}*\n");
                        messagecount++;

                        await Task.Delay(1000, cancellationToken);
                    }
                    newstextbuilder.Append(newsText);
                }

                if (newstextbuilder.Length > 0)
                {
                    await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: newstextbuilder.ToString(),
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: cancellationToken);
                }
            }
            else
            {
                Console.WriteLine("Результат пустой");
                return;
            }
        }
    }
}
