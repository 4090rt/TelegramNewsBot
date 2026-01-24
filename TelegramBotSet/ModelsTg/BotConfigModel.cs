using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot.TelegramBotSet.ModelsTg
{
    public class BotConfigModel
    {
        public string Bot_TokenTg{ get; set; } = string.Empty;
        public long AdminId { get; set; }
    }
}
