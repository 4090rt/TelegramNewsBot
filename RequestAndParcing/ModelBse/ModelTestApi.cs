using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TelegramNewsBot.RequestAndParcing.ModelBse
{
    public class ModelTestApi
    {
        // Часовой пояс и время
        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }

        [JsonPropertyName("timezone_offset")]
        public int TimezoneOffset { get; set; }

        [JsonPropertyName("timezone_offset_with_dst")]
        public int TimezoneOffsetWithDst { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("date_time")]
        public string DateTime { get; set; }

        [JsonPropertyName("date_time_txt")]
        public string DateTimeTxt { get; set; }

        [JsonPropertyName("date_time_wti")]
        public string DateTimeWti { get; set; }

        [JsonPropertyName("date_time_ymd")]
        public string DateTimeYmd { get; set; }

        [JsonPropertyName("date_time_unix")]
        public double DateTimeUnix { get; set; }

        [JsonPropertyName("time_24")]
        public string Time24 { get; set; }

        [JsonPropertyName("time_12")]
        public string Time12 { get; set; }

        [JsonPropertyName("week")]
        public int Week { get; set; }

        [JsonPropertyName("month")]
        public int Month { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("year_abbr")]
        public string YearAbbr { get; set; }

        [JsonPropertyName("current_tz_abbreviation")]
        public string CurrentTzAbbreviation { get; set; }

        [JsonPropertyName("current_tz_full_name")]
        public string CurrentTzFullName { get; set; }

        [JsonPropertyName("standard_tz_abbreviation")]
        public string StandardTzAbbreviation { get; set; }

        [JsonPropertyName("standard_tz_full_name")]
        public string StandardTzFullName { get; set; }

        [JsonPropertyName("is_dst")]
        public bool IsDst { get; set; }

        [JsonPropertyName("dst_savings")]
        public int DstSavings { get; set; }

        [JsonPropertyName("dst_exists")]
        public bool DstExists { get; set; }

        [JsonPropertyName("dst_tz_abbreviation")]
        public string DstTzAbbreviation { get; set; }

        [JsonPropertyName("dst_tz_full_name")]
        public string DstTzFullName { get; set; }

        [JsonPropertyName("dst_start")]
        public string DstStart { get; set; }

        [JsonPropertyName("dst_end")]
        public string DstEnd { get; set; }

        // Город и страна НЕ будут заполняться - они в geo!
        public string City { get; set; }
        public string Country { get; set; }
        public string Location { get; set; }

        public string TelegramFormattedMessage
        {
            get
            {
                // Используем то, что точно есть
                return $"📍 *Местоположение*\n" +
                       $"🕒 *Время:* {Time24}\n" +
                       $"📅 *Дата:* {Date}\n" +
                       $"🗓️ *Подробно:* {DateTimeTxt}\n" +
                       $"🌍 *Часовой пояс:* {Timezone}\n" +
                       $"⏱️ *Смещение от UTC:* UTC+{TimezoneOffset}\n" +
                       $"⚙️ *Unix время:* {DateTimeUnix}\n" +
                       $"🏙️ *Текущий пояс:* {CurrentTzAbbreviation} ({CurrentTzFullName})";
            }
        }
    }
}
