using System;
using Microsoft.Extensions.DependencyInjection;
using VkNet;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Extensions;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace ConsoleApp
{
    internal class Program
    {
        private static IVkApi _api;

        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddAudioBypass();

            _api = new VkApi(serviceCollection);

            Console.WriteLine(" > Номер телефона/E-mail:");
            var login = Console.ReadLine();

            Console.WriteLine(" > Пароль:");
            var password = Console.ReadLine();

            _api.Authorize(new ApiAuthParams
            {
                Login = login,
                Password = password,
                TwoFactorAuthorization = () =>
                {
                    Console.WriteLine(" > Код двухфакторной аутентификации:");
                    return Console.ReadLine();
                }
            });

            Console.WriteLine($" > Access Token: {_api.Token}");

            var audios = _api.Audio.Get(new AudioGetParams {Count = 10});
            foreach (var audio in audios) Console.WriteLine($" > {audio.Artist} - {audio.Title}");

            Console.ReadLine();
        }
    }
}