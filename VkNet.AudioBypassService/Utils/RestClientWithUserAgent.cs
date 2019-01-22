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

namespace VkNet.AudioBypassService.Utils
{
    /// <inheritdoc />
    [UsedImplicitly]
    public class RestClientWithUserAgent : IRestClient
    {
        private const string DefaultUserAgent =
            "KateMobileAndroid/51.2 lite-443 (Android 4.4.2; SDK 19; x86; unknown Android SDK built for x86; en)";

        private readonly HttpClient _httpClient;

        /// <summary>
        ///     The log
        /// </summary>
        private readonly ILogger<RestClient> _logger;

        private TimeSpan _timeoutSeconds;

        /// <inheritdoc />
        public RestClientWithUserAgent(ILogger<RestClient> logger, HttpClient httpClient,
            string userAgent = DefaultUserAgent)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
        }

        public IWebProxy Proxy
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        /// <inheritdoc />
        public TimeSpan Timeout
        {
            get => _timeoutSeconds == TimeSpan.Zero ? TimeSpan.FromSeconds(300) : _timeoutSeconds;
            set => _timeoutSeconds = value;
        }

        /// <inheritdoc />
        public Task<HttpResponse<string>> GetAsync(Uri uri, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var queries = parameters
                .Where(parameter => !string.IsNullOrWhiteSpace(parameter.Value))
                .Select(parameter => $"{parameter.Key.ToLowerInvariant()}={parameter.Value}");

            var url = new UriBuilder(uri)
            {
                Query = string.Join("&", queries)
            };

            _logger?.LogDebug($"GET request: {url.Uri}");

            var request = new HttpRequestMessage(HttpMethod.Get, url.Uri);

            return CallAsync(httpClient => httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead));
        }

        /// <inheritdoc />
        public Task<HttpResponse<string>> PostAsync(Uri uri, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            if (_logger != null)
            {
                var json = JsonConvert.SerializeObject(parameters);
                _logger.LogDebug($"POST request: {uri}{Environment.NewLine}{Utilities.PrettyPrintJson(json)}");
            }

            var content = new FormUrlEncodedContent(parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, uri) {Content = content};

            return CallAsync(httpClient => httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead));
        }

        private async Task<HttpResponse<string>> CallAsync(Func<HttpClient, Task<HttpResponseMessage>> method)
        {
            var response = await method(_httpClient).ConfigureAwait(false);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            _logger?.LogDebug($"Response:{Environment.NewLine}{Utilities.PrettyPrintJson(content)}");
            var url = response.RequestMessage.RequestUri.ToString();

            return response.IsSuccessStatusCode
                ? HttpResponse<string>.Success(response.StatusCode, content, url)
                : HttpResponse<string>.Fail(response.StatusCode, content, url);
        }
    }
}