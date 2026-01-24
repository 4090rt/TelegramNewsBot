using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
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
                    await _botClient.SendTextMessageAsync
                        (chatId: chatId,
                        text: "Привет",
                        cancellationToken: cancellationToken
                        );
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
                var messageText = string.Join("\n\n",
                    resultat.Select((item, index) =>
                        $"{(index + 1)}. {item.Title}\n{item.Description}"));

                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: messageText,
                    cancellationToken: cancellationToken
                    );
            }
            else
            {
                Console.WriteLine("Результат пустой");
                return;
            }
        }
    }
}
