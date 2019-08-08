using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VkNet.Abstractions.Utils;
using VkNet.Utils;

namespace VkNet.AudioBypassService.Utils
{
	/// <inheritdoc cref="IRestClient" />
	[UsedImplicitly]
	public class RestClientWithUserAgent : IRestClient
	{
		private readonly string _userAgent = "VKAndroidApp/5.2.6-3146 (Android 13.3.7; SDK 228; armeabi-v7a; AudioBypass; en)";

		private readonly ILogger<RestClient> _logger;

		/// <inheritdoc />
		public RestClientWithUserAgent(ILogger<RestClient> logger, string userAgent = null)
		{
			_logger = logger;
			if (!string.IsNullOrWhiteSpace(userAgent))
			{
				_userAgent = userAgent;
			}
		}

		public IWebProxy Proxy { get; set; }

		/// <inheritdoc />
		public TimeSpan Timeout { get; set; }

		/// <inheritdoc />
		public Task<HttpResponse<string>> GetAsync(Uri uri, IEnumerable<KeyValuePair<string, string>> parameters)
		{
			if (_logger != null)
			{
				var uriBuilder = new UriBuilder(uri) { Query = string.Join("&", parameters.Select(x => $"{x.Key}={x.Value}")) };

				_logger.LogDebug($"GET request: {uriBuilder.Uri}");
			}

			return CallAsync(() => uri.ToString().AllowAnyHttpStatus().SetQueryParams(parameters).WithHeader("User-Agent", _userAgent).GetAsync());
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

			return CallAsync(() => uri.ToString().AllowAnyHttpStatus().WithHeader("User-Agent", _userAgent).PostAsync(content));
		}

		/// <inheritdoc />
		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		private async Task<HttpResponse<string>> CallAsync(Func<Task<HttpResponseMessage>> method)
		{
			var response = await method().ConfigureAwait(false);

			var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			_logger?.LogDebug($"Response:{Environment.NewLine}{Utilities.PrettyPrintJson(content)}");
			var url = response.RequestMessage.RequestUri.ToString();

			return response.IsSuccessStatusCode
				? HttpResponse<string>.Success(response.StatusCode, content, url)
				: HttpResponse<string>.Fail(response.StatusCode, content, url);
		}
	}
}