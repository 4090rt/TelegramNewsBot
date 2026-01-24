using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bots.Types;
using TelegramNewsBot.TelegramBotSet.CommandHendler;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using Microsoft.Extensions.Hosting;
using TelegramNewsBot.TelegramBotSet.ModelsTg;

namespace TelegramNewsBot.TelegramBotSet.TelegramService
{
    public class TelegramService: IHostedService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramService> _logger;
        private readonly BotConfigModel _config;
        private readonly CommandHendler.CommandHendler _commandHandlerr;

        //присваиваем значения
        public TelegramService(ITelegramBotClient botClient,IOptions<BotConfigModel> config, ILogger<TelegramService> logger, CommandHendler.CommandHendler commandHandler)
        {
            _config = config.Value;
            _logger = logger;
            _commandHandlerr = commandHandler;
            _botClient = botClient;

            _logger.LogInformation("TelegramBotService создан");
        }


        // метод запуска бота и обновления сообщений и нажатий по кнопка
        public async Task StartAsync(CancellationToken cancellation)
        {
            // объект ReceiverOptions - обновление настроек  -    AllowedUpdates  - какие настройки - 
            var recivedOptoins = new Telegram.Bot.Polling.ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                    // обновляем пользовтаельские сообщения
                    Telegram.Bot.Types.Enums.UpdateType.Message,
                    // обновляем сообщения-ответы на inline-кнопки
                    Telegram.Bot.Types.Enums.UpdateType.CallbackQuery
                },
                // отбрасывает все накопленные сообщения за время отключения бота и работает только с новыми
                ThrowPendingUpdates = true,
            };
            //Запускаем прослушивание
            _botClient.StartReceiving(
                // метод обработки обновлений
                updateHandler: HandlerUpdate,
                // метод обработки ошибок обновления
                pollingErrorHandler: HandlerPollyErrorAsync,
                // передаем то что настраивали выше
                receiverOptions: recivedOptoins,
                // токен отмены
                cancellationToken: cancellation
                );

            try
            {
                var botToken = _botClient.BotId?.ToString();
                // или другой способ посмотреть токен

                _logger.LogInformation($"Пытаюсь подключиться с токеном, ID бота: {_botClient.BotId}");
                // с помощью GetMeAsync  проверяем, что бот запущен
                var me = await _botClient.GetMeAsync(cancellation);
                _logger.LogInformation($"Бот @{me.Username} запущен успешно!");
                _logger.LogInformation($"ID бота: {me.Id}");
                _logger.LogInformation($"Имя бота: {me.FirstName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить информацию о боте");
                throw;
            }
        }
        //Метод обработки обновлении (смотри какой тип сообщения пришел и правильно на него реагирует посылая в методы обработки сообщений)
        public async Task HandlerUpdate(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            try
            {
                // по типу полученного сообщения
                switch (update.Type)
                { 
                    //если сообщение текстовое
                    case Telegram.Bot.Types.Enums.UpdateType.Message:
                        // отправляем в метод обработки сообщений
                        await _commandHandlerr.HanderMessage(update.Message, cancellationToken);
                        break;
                    // если нажатие на кнопку
                    case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:

                        // 1. Отправляем сообщение в чат
                        var progressMsg = await _botClient.SendTextMessageAsync(
                            chatId: update.CallbackQuery.Message.Chat.Id,
                            text: "⏳ Обрабатываю ваш запрос...",
                            cancellationToken: cancellationToken
                        );
                        // отправляем в метод обработки callback
                        await _commandHandlerr.CallBackAsync(update.CallbackQuery, cancellationToken);
                        break;
                        // исключения
                    default:
                        _logger.LogDebug($"Необработанный тип обновления: {update.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки обновления");
            }
        }

        //Метод обработки ошибок с тг апи
        public Task HandlerPollyErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellation)
        {
            string errormessage;
            // если исключение ApiRequestException то выводим информацию о нем в противном случае просто выводим исключение
            switch (exception)
            {
                case ApiRequestException apiRequestException:
                    errormessage = $"Telegram API Error: {apiRequestException.ErrorCode} - {apiRequestException.Message}";
                    break;
                default:
                    errormessage = exception.ToString();
                    break;


            }
            _logger.LogError(errormessage);
            return Task.CompletedTask;
        }

        // остановка бота
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Остановка Telegram бота...");
            return Task.CompletedTask;
        }
    }
}
