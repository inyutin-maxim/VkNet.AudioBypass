using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using VkNet.Abstractions.Core;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService.Abstractions;
using VkNet.Utils;

namespace VkNet.AudioBypassService.Utils
{
	[UsedImplicitly]
	public class VkApiInvoker : IVkApiInvoker
	{
		[NotNull]
		private readonly IVkApiVersionManager _versionManager;

		[NotNull]
		private readonly IRestClient _restClient;

		[NotNull]
		private readonly ICaptchaHandler _captchaHandler;

		public VkApiInvoker([NotNull] IVkApiVersionManager versionManager, [NotNull] IRestClient restClient, [NotNull] ICaptchaHandler captchaHandler)
		{
			_versionManager = versionManager;
			_restClient = restClient;
			_captchaHandler = captchaHandler;
		}

		public async Task<T> CallAsync<T>(Uri uri, VkParameters parameters, CancellationToken cancellationToken = default)
		{
			var result = await CallAsync(uri, parameters, cancellationToken).ConfigureAwait(false);

			return result.ToObject<T>(DefaultJsonSerializer);
		}

		public async Task<JToken> CallAsync(Uri uri, VkParameters parameters, CancellationToken cancellationToken = default)
		{
			if (!parameters.ContainsKey("v"))
			{
				parameters.Add("v", _versionManager.Version);
			}

			var response = await _captchaHandler.Perform((sid, key) =>
			{
				parameters.Add("captcha_sid", sid);
				parameters.Add("captcha_key", key);

				return InvokeAsyncInternal(uri, parameters, cancellationToken).ConfigureAwait(false);
			});

			return JToken.Parse(response);
		}

		public async Task<T> CallAsync<T>(string methodName, VkParameters parameters, CancellationToken cancellationToken = default)
		{
			var url = $"https://api.vk.com/method/{methodName}";

			var jObject = await CallAsync(new Uri(url), parameters, cancellationToken).ConfigureAwait(false);

			var response = jObject["response"];

			return response != null ? response.ToObject<T>(DefaultJsonSerializer) : default;
		}

		public Task<JToken> CallAsync(string methodName, VkParameters parameters, CancellationToken cancellationToken = default)
		{
			return CallAsync<JToken>(methodName, parameters, cancellationToken);
		}

		internal async Task<string> InvokeAsyncInternal(Uri uri, IDictionary<string, string> parameters, CancellationToken cancellationToken = default)
		{
			var response = await _restClient.PostAsync(uri, parameters, Encoding.UTF8).ConfigureAwait(false);

			var answer = response.Value ?? response.Message;

			VkAuthErrors.IfErrorThrowException(answer);
			VkErrors.IfErrorThrowException(answer);

			return answer;
		}

		private JsonSerializer DefaultJsonSerializer =>
			new JsonSerializer
			{
				ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore
			};
	}
}