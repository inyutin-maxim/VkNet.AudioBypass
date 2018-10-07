using System;
using Microsoft.Extensions.DependencyInjection;
using VkNet;
using VkNet.Abstractions;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace ConsoleApp
{
    class Program
    {
        private static IVkApi _api;

        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IBrowser, Authorization>();
            serviceCollection.AddSingleton<IRestClient, RestClientEx>();
            serviceCollection.AddSingleton<IReceiptParser, ReceiptParser>();
            _api = new VkApi(serviceCollection);

            _api.Authorize(new ApiAuthParams
            {
                Login = "ЛОГИН",
                Password = "ПАРОЛЬ"
            });

            var audios = _api.Audio.Get(new AudioGetParams {Count = 10});
            foreach (var audio in audios)
            {
                Console.WriteLine($" > {audio.Artist} - {audio.Title}");
            }

            Console.ReadLine();
        }
    }
}