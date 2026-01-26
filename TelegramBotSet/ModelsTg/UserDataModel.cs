using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot.TelegramBotSet.ModelsTg
{
    public class UserDataModel
    {
        public long ChaId { get; set; }//айди чата
        public UserState state { get; set; }// Статус пользователя
        public string Email { get; set; }// Email пользователя
        public DateTime TimeNewsUpdate { get; set; } = DateTime.UtcNow;// время обновления новостей
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;// Время последней активности
    }

    public enum UserState
    {
        Idle, //ничего не делает
        WaitingNews,// ждет новости
        Proccesing// процесс получения новостей
    }
}
