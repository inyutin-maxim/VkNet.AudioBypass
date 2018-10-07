using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VkNet.Abstractions.Utils;
using VkNet.Utils;

namespace VkNet.AudioBypassService
{
    /// <inheritdoc />
    [UsedImplicitly]
    public class RestClientEx : IRestClient
    {
        /// <summary>
        /// The log
        /// </summary>
        private readonly ILogger<RestClientEx> _logger;

        private TimeSpan _timeoutSeconds;

        /// <inheritdoc />
        public RestClientEx(ILogger<RestClientEx> logger, IWebProxy proxy)
        {
            _logger = logger;
            Proxy = proxy;
        }

        /// <inheritdoc />
        public IWebProxy Proxy { get; set; }

        /// <inheritdoc />
        public TimeSpan Timeout
        {
            get => _timeoutSeconds == TimeSpan.Zero ? TimeSpan.FromSeconds(300) : _timeoutSeconds;
            set => _timeoutSeconds = value;
        }

        /// <inheritdoc />
        public Task<HttpResponse<string>> GetAsync(Uri uri, VkParameters parameters)
        {
            var queries = parameters.Where(k => !string.IsNullOrWhiteSpace(k.Value))
                .Select(kvp => $"{kvp.Key.ToLowerInvariant()}={kvp.Value}");

            var url = new UriBuilder(uri)
            {
                Query = string.Join("&", queries)
            };

            _logger?.LogDebug($"GET request: {url.Uri}");

            return Call(httpClient => httpClient.GetAsync(url.Uri));
        }

        /// <inheritdoc />
        public Task<HttpResponse<string>> PostAsync(Uri uri, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var json = JsonConvert.SerializeObject(parameters);
            _logger?.LogDebug($"POST request: {uri}{Environment.NewLine}{Utilities.PreetyPrintJson(json)}");
            HttpContent content = new FormUrlEncodedContent(parameters);

            return Call(httpClient => httpClient.PostAsync(uri, content));
        }

        private async Task<HttpResponse<string>> Call(Func<HttpClient, Task<HttpResponseMessage>> method)
        {
            var handler = new HttpClientHandler
            {
                UseProxy = false
            };

            if (Proxy != null)
            {
                handler = new HttpClientHandler
                {
                    Proxy = Proxy,
                    UseProxy = true
                };

                _logger?.LogDebug($"Use Proxy: {Proxy}");
            }

            using (var client = new HttpClient(handler))
            {
                if (Timeout != TimeSpan.Zero)
                {
                    client.Timeout = Timeout;
                }
                
                client.DefaultRequestHeaders.Add("User-Agent", "VKAndroidApp/5.0.1-1237 (Android 7.1.1; SDK 25; armeabi-v7a; Razer p90; en)");
                var response = await method(client).ConfigureAwait(false);
                var requestUri = response.RequestMessage.RequestUri.ToString();

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger?.LogDebug($"Response:{Environment.NewLine}{Utilities.PreetyPrintJson(content)}");
                
                return response.IsSuccessStatusCode 
                    ? HttpResponse<string>.Success(response.StatusCode, content, requestUri) 
                    : HttpResponse<string>.Fail(response.StatusCode, content, requestUri);
            }
        }
    }
}