using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Utils;
using VkNet.Utils;

namespace VkNet.AudioBypassService.Extensions
{
    public static class AudioBypassServiceCollection
    {
        public static IServiceCollection AddAudioBypass([NotNull] this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<FakeSafetyNetClient>();
            services.TryAddSingleton<IBrowser, VkAndroidAuthorization>();
            services.TryAddSingleton<IRestClient, RestClientWithUserAgent>();
            services.TryAddSingleton<IReceiptParser, ReceiptParser>();

            return services;
        }
    }
}