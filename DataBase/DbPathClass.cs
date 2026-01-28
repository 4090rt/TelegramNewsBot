using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot.DataBase
{
    public class DbPathClass
    {
        public string dbpath()
        {
            string projectDirectory = Directory.GetCurrentDirectory();
            string dbPath = Path.Combine(projectDirectory, "DataBase.db");
            return dbPath;
        }
    }
}
