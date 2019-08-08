using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Flurl.Http;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using VkNet.AudioBypassService.Models.Google;

namespace VkNet.AudioBypassService.Utils
{
	[UsedImplicitly]
	internal class FakeSafetyNetClient
	{
		private const string GcmUserAgent = "Android-GCM/1.5 (generic_x86 KK)";

		private readonly string _appId;

		[CanBeNull]
		private readonly ILogger<FakeSafetyNetClient> _logger;

		public FakeSafetyNetClient([CanBeNull] ILogger<FakeSafetyNetClient> logger)
		{
			_logger = logger;
			_appId = RandomString.Generate(11);
		}

		public async Task<AndroidCheckinResponse> CheckIn()
		{
			_logger?.LogDebug($"{nameof(CheckIn)}");

			var androidRequest = new AndroidCheckinRequest
			{
				Checkin = new AndroidCheckinProto
				{
					CellOperator = "310260",
					Roaming = "mobile:LTE:",
					SimOperator = "310260",
					Type = DeviceType.DeviceAndroidOs
				},
				Digest = "1-929a0dca0eee55513280171a8585da7dcd3700f8",
				Locale = "en_US",
				LoggingId = -8212629671123625360,
				Meid = "358240051111110",
				OtaCerts = { "71Q6Rn2DDZl1zPDVaaeEHItd+Yg=" },
				TimeZone = "America/New_York",
				Version = 3
			};
			var requestStream = new MemoryStream();
			Serializer.Serialize(requestStream, androidRequest);

			var content = new ByteArrayContent(requestStream.ToArray());
			content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-protobuffer");

			var response = await "https://android.clients.google.com/checkin"
								 .WithHeader("User-Agent", GcmUserAgent)
								 .PostAsync(content)
								 .ConfigureAwait(false);

			response.EnsureSuccessStatusCode();

			var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

			return Serializer.Deserialize<AndroidCheckinResponse>(responseStream);
		}

		public async Task<string> GetReceipt(AndroidCheckinResponse credentials)
		{
			_logger?.LogDebug($"{nameof(GetReceipt)}");

			var requestParams = GetRequestParams(credentials.AndroidId.ToString());
			var content = new FormUrlEncodedContent(requestParams);

			var response = await "https://android.clients.google.com/c2dm/register3"
								 .WithHeader("Authorization", $"AidLogin {credentials.AndroidId}:{credentials.SecurityToken}")
								 .WithHeader("User-Agent", GcmUserAgent)
								 .PostAsync(content)
								 .ConfigureAwait(false);

			response.EnsureSuccessStatusCode();

			var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			var result = responseString.Split(new[] { "|ID|2|:" }, StringSplitOptions.None)[1];
			if (result == "PHONE_REGISTRATION_ERROR")
			{
				throw new InvalidOperationException($"{nameof(GetReceipt)} bad response: {result}\n{content}");
			}

			return result;
		}

		private Dictionary<string, string> GetRequestParams(string device)
		{
			return new Dictionary<string, string>
			{
				{ "X-scope", "id" },
				{ "X-osv", "23" },
				{ "X-subtype", "54740537194" },
				{ "X-app_ver", "443" },
				{ "X-kid", "|ID|2|" },
				{ "X-appid", _appId },
				{ "X-gmsv", "13283005" },
				{ "X-cliv", "iid-10084000" },
				{ "X-app_ver_name", "51.2 lite" },
				{ "X-X-kid", "|ID|2|" },
				{ "X-subscription", "54740537194" },
				{ "X-X-subscription", "54740537194" },
				{ "X-X-subtype", "54740537194" },
				{ "app", "com.perm.kate_new_6" },
				{ "sender", "54740537194" },
				{ "device", device },
				{ "cert", "966882ba564c2619d55d0a9afd4327a38c327456" },
				{ "app_ver", "443" },
				{ "info", "g57d5w1C4CcRUO6eTSP7b7VoT8yTYhY" },
				{ "gcm_ver", "13283005" },
				{ "plat", "0" },
				{ "X-messenger2", "1" }
			};
		}
	}
}