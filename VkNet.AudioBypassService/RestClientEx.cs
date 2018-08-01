using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
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
        private readonly ILogger _logger;

        private TimeSpan _timeoutSeconds;

        /// <inheritdoc />
        public RestClientEx(ILogger logger, IWebProxy proxy)
        {
            _logger = logger;
            Proxy = proxy;
        }

        /// <inheritdoc />
        public IWebProxy Proxy { get; set; }

        /// <inheritdoc />
        public TimeSpan Timeout
        {
            get => _timeoutSeconds == TimeSpan.Zero ? TimeSpan.FromSeconds(value: 300) : _timeoutSeconds;
            set => _timeoutSeconds = value;
        }

        /// <inheritdoc />
        public async Task<HttpResponse<string>> GetAsync(Uri uri, VkParameters parameters)
        {
            var queries = parameters.Where(predicate: k => !string.IsNullOrWhiteSpace(value: k.Value))
                .Select(selector: kvp => $"{kvp.Key.ToLowerInvariant()}={kvp.Value}");

            var url = new UriBuilder(uri: uri)
            {
                Query = string.Join(separator: "&", values: queries)
            };

            _logger?.Debug(message: $"GET request: {url.Uri}");

            return await Call(method: httpClient => httpClient.GetAsync(requestUri: url.Uri)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<HttpResponse<string>> PostAsync(Uri uri, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var json = JsonConvert.SerializeObject(value: parameters);
            _logger?.Debug(message: $"POST request: {uri}{Environment.NewLine}{Utilities.PreetyPrintJson(json: json)}");
            HttpContent content = new FormUrlEncodedContent(nameValueCollection: parameters);

            return await Call(method: httpClient => httpClient.PostAsync(requestUri: uri, content: content)).ConfigureAwait(false);
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

                _logger?.Debug(message: $"Use Proxy: {Proxy}");
            }

            using (var client = new HttpClient(handler: handler))
            {
                if (Timeout != TimeSpan.Zero)
                {
                    client.Timeout = Timeout;
                }
                client.DefaultRequestHeaders.Add("User-Agent", "VKAndroidApp/5.0.1-1237 (Android 7.1.1; SDK 25; armeabi-v7a; Razer p90; en)");
                var response = await method(arg: client).ConfigureAwait(false);
                var requestUri = response.RequestMessage.RequestUri.ToString();

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger?.Debug(message: $"Response:{Environment.NewLine}{Utilities.PreetyPrintJson(json: content)}");

                return response.IsSuccessStatusCode ?
                    HttpResponse<string>.Success(httpStatusCode: response.StatusCode, value: content, requestUri: requestUri) :
                    HttpResponse<string>.Fail(httpStatusCode: response.StatusCode, message: content, requestUri: requestUri);
            }
        }
    }
}