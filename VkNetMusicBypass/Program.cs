using System.IO;
using VkNet;
using System.Collections.ObjectModel;
using VkNet.Model;
using System;
using Microsoft.Extensions.DependencyInjection;
using VkNet.Abstractions.Utils;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VkNetMusicBypass
{
    class Program
    {
        static VkApi api;
        static Authorize auth;

        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.TryAddSingleton<IRestClient, RestClient>();
            api = new VkApi(serviceCollection);
            auth = new Authorize(api, new ReceiptParser());

            string token = File.ReadAllText("token.txt");

            if (!string.IsNullOrEmpty(token))
                auth.Auth(token);
                
            else
                auth.Auth("login", "password");

            //Один токен после манипуляций с обходом может работать дальше
            File.WriteAllText("token.txt", api.Token);

            var audios = Get();
            foreach (var track in audios)
            {
                Console.WriteLine($"{track.Artist} - {track.Title}");
            }

            Console.ReadLine();
        }

        /// <summary>
        /// Пример использования
        /// </summary>
        /// <returns></returns>
        private static ReadOnlyCollection<VkNet.Model.Attachments.Audio> Get()
        {
            User user = null;

            try
            {
                return api.Audio.Get(out user, new VkNet.Model.RequestParams.AudioGetParams()
                {
                    Count = 40,
                });
            }
            catch (VkNet.Exception.VkApiException)
            {
                //token confirmation required exception
                string newToken = auth.Bypass();
                auth.Auth(newToken);
                File.WriteAllText("token.txt", newToken); //Сохранить токен после манипуляция с обходом
                return Get();
            }
        }
    }
}
