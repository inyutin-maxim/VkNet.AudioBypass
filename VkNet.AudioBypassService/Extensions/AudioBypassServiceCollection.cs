using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VkNet.Abstractions.Utils;
using VkNet.Utils;

namespace VkNet.AudioBypassService.Extensions
{
    public static class AudioBypassServiceCollection
    {
        public static IServiceCollection AddAudioBypass([NotNull] this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<IBrowser, Authorization>();
            services.TryAddSingleton<IRestClient, RestClientEx>();
            services.TryAddSingleton<IReceiptParser, ReceiptParser>();

            return services;
        }
    }
}