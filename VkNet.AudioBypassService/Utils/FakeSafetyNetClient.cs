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

		[CanBeNull]
		private readonly ILogger<FakeSafetyNetClient> _logger;

		public FakeSafetyNetClient([CanBeNull] ILogger<FakeSafetyNetClient> logger)
		{
			_logger = logger;
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

		public async Task<string> Register(AndroidCheckinResponse credentials)
		{
			_logger?.LogDebug($"{nameof(Register)}");

			var requestParams = GetRegisterRequestParams(RandomString.Generate(11), credentials.AndroidId.ToString());
			var content = new FormUrlEncodedContent(requestParams);

			var response = await "https://android.clients.google.com/c2dm/register3"
								 .WithHeader("Authorization", $"AidLogin {credentials.AndroidId}:{credentials.SecurityToken}")
								 .WithHeader("User-Agent", GcmUserAgent)
								 .PostAsync(content)
								 .ConfigureAwait(false);

			response.EnsureSuccessStatusCode();

			return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
		}

		private Dictionary<string, string> GetRegisterRequestParams(string appId, string device)
		{
			return new Dictionary<string, string>
			{
				{ "X-scope", "*" },
				{ "X-subtype", "841415684880" },
				{ "sender", "841415684880" },
				{ "X-appid", appId },
				{ "app", "com.vkontakte.android" },
				{ "device", device },
			};
		}
	}
}