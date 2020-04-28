using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using VkNet.AudioBypassService.Abstractions;

namespace VkNet.AudioBypassService.Utils
{
	[UsedImplicitly]
	internal class ReceiptParser : IReceiptParser
	{
		private readonly FakeSafetyNetClient _safetyNetClient;

		public ReceiptParser([NotNull] FakeSafetyNetClient safetyNetClient)
		{
			_safetyNetClient = safetyNetClient;
		}

		public async Task<string> GetReceipt()
		{
			var checkinResponse = await _safetyNetClient.CheckIn().ConfigureAwait(false);

			var registerResponse = await _safetyNetClient.Register(checkinResponse).ConfigureAwait(false);

			var result = registerResponse.Split(new[] { "=" }, StringSplitOptions.None).LastOrDefault();

			if (result == null || result == "PHONE_REGISTRATION_ERROR")
			{
				throw new InvalidOperationException($"Bad Response: {registerResponse}");
			}

			return registerResponse.Remove(0, 7);
		}
	}
}