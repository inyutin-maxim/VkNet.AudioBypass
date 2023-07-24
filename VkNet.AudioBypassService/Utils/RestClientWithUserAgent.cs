using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using VkNet.Abstractions.Utils;
using VkNet.Utils;

namespace VkNet.AudioBypassService.Utils
{
	/// <inheritdoc cref="IRestClient" />
	[UsedImplicitly]
	public class RestClientWithUserAgent : IRestClient
	{
		private static readonly IDictionary<string, string> VkHeaders = new Dictionary<string, string>
		{
			{ "User-Agent", "VKAndroidApp/7.26-12338 (Android 11; SDK 30; armeabi-v7a; Android; ru; 2960x1440)" },
			{ "x-vk-android-client", "new" }
		};

		private readonly RestClient _restClientProxy;

		public RestClientWithUserAgent(HttpClient httpClient, ILogger<RestClient> logger)
		{
			_restClientProxy = new RestClient(httpClient, logger);

			foreach (var header in VkHeaders)
			{
				_restClientProxy.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
			}
		}

		/// <inheritdoc />
		public Task<HttpResponse<string>> GetAsync(Uri uri, IEnumerable<KeyValuePair<string, string>> parameters, Encoding encoding, CancellationToken token = default)
		{
			return _restClientProxy.GetAsync(uri, parameters, encoding, token);
		}

		/// <inheritdoc />
		public Task<HttpResponse<string>> PostAsync(Uri uri, IEnumerable<KeyValuePair<string, string>> parameters, Encoding encoding, IEnumerable<KeyValuePair<string, string>> headers = null, CancellationToken token = default)
		{
			return _restClientProxy.PostAsync(uri, parameters, encoding, headers, token);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_restClientProxy?.Dispose();
		}
	}
}