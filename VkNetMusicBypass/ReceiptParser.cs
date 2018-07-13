using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;

namespace VkNetMusicBypass
{
    class ReceiptParser : IReceiptParser
    {
        public string GetReceipt()
        {
            //Парсинг receipt в данном случае идет с сервера для VK Desktop Player
            //По хорошему нужно реализовать получение через android.clients.google.com/c2dm/register3 (мне было лень, правда)
            //Реквесты можно отловить у VK COFFEE
            var wc = new WebClient();
            string response = wc.DownloadString("http://www.dp1337.somee.com/update/fixmyfuckingmusic.aspx?v=0.6.2b&b=exp");
            var json = JObject.Parse(response);
            var rec = json["rec"].ToObject<string[]>();
            return rec[new Random().Next(rec.Length)];
        }
    }
}
