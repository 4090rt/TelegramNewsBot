using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bots.Types;
using static System.Net.Mime.MediaTypeNames;

namespace TelegramNewsBot.TelegramBotSet.InkineButtons
{
    public class InlineButtons
    {
        private readonly ITelegramBotClient _botclient;

        public InlineButtons(ITelegramBotClient botclient)
        {
            _botclient = botclient;
        }

        public async Task InlineButtonss(long chatid, CancellationToken cancellation)
        {
            var objects = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]
              {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Узнать новости", "/MainCommands"),
                    InlineKeyboardButton.WithCallbackData("Погода в вашем регионе", "weather")
                }
            });

            await _botclient.SendTextMessageAsync(
             chatId: chatid,
             text: "Нажмите на кнопку, чтобы узнать новости",
             replyMarkup: objects,
             cancellationToken: cancellation
             );
        }
    }
}
