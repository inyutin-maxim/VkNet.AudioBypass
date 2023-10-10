using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VkNet.Abstractions.Utils;
using VkNet.Utils;

namespace VkNet.AudioBypassService.Utils;

/// <inheritdoc cref="IRestClient" />
[UsedImplicitly]
public class RestClientWithUserAgent : IRestClient
{
	private static readonly IDictionary<string, string> VkHeaders = new Dictionary<string, string>
	{
		{ "User-Agent", "VKAndroidApp/7.26-12338 (Android 11; SDK 30; armeabi-v7a; Android; ru; 2960x1440)" },
		{ "x-vk-android-client", "new" }
	};

	/// <summary>
	/// Http client
	/// </summary>
	[UsedImplicitly]
	public HttpClient HttpClient { get; }

	private readonly ILogger _logger;

	public RestClientWithUserAgent(HttpClient httpClient, ILogger logger)
	{
		HttpClient = httpClient;
		_logger = logger;
		foreach (var header in VkHeaders)
		{
			HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
		}
	}

	/// <inheritdoc />
	public Task<HttpResponse<string>> GetAsync(Uri uri, IEnumerable<KeyValuePair<string, string>> parameters, Encoding encoding,
											   CancellationToken token = default)
	{
		var url = Url.Combine(uri.ToString(), Url.QueryFrom(parameters.ToArray()));

		if (_logger.IsEnabled(LogLevel.Debug))
		{
			_logger.LogDebug("GET request: {Url}", url);
		}

		return CallAsync(() => HttpClient.GetAsync(new Uri(url), token), encoding, token);
	}

	/// <inheritdoc />
	public Task<HttpResponse<string>> PostAsync(Uri uri, IEnumerable<KeyValuePair<string, string>> parameters, Encoding encoding,
												IEnumerable<KeyValuePair<string, string>> headers = null, CancellationToken token = default)
	{
		if (_logger is not null)
		{
			var json = JsonConvert.SerializeObject(parameters);

			if (_logger.IsEnabled(LogLevel.Debug))
			{
				_logger.LogDebug("POST request: {Uri}{NewLine}{PrettyJson}", uri, Environment.NewLine, Utilities.PrettyPrintJson(json));
			}
		}

		if (headers is not null && headers.Any())
		{
			headers.ToList()
				   .ForEach(header =>
				   {
					   if (header.Key.ToLower() == "content-type")
					   {
						   HttpClient.DefaultRequestHeaders.Accept.Add(new(header.Value));
					   }
					   else
					   {
						   HttpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
					   }
				   });
		}

		var content = new FormUrlEncodedContent(parameters);

		return CallAsync(() => HttpClient.PostAsync(uri, content, token), encoding, token);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		HttpClient?.Dispose();
	}

	private async Task<HttpResponse<string>> CallAsync(Func<Task<HttpResponseMessage>> method, Encoding encoding, CancellationToken token)
	{
		var response = await method()
			.ConfigureAwait(false);
#if NETSTANDARD2_0
		var bytes = await response.Content.ReadAsByteArrayAsync()
								  .ConfigureAwait(false);

#else
		var bytes = await response.Content.ReadAsByteArrayAsync(token)
			.ConfigureAwait(false);

#endif

		var content = encoding.GetString(bytes, 0, bytes.Length);

		if (_logger.IsEnabled(LogLevel.Debug))
		{
			_logger.LogDebug("Response:{NewLine}{PrettyJson}", Environment.NewLine, Utilities.PrettyPrintJson(content));
		}

		var requestUri = response.RequestMessage?.RequestUri;

		return response.IsSuccessStatusCode
			? HttpResponse<string>.Success(response.StatusCode, content, requestUri)
			: HttpResponse<string>.Fail(response.StatusCode, content, requestUri);
	}
}