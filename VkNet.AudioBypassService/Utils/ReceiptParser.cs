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

			var result = registerResponse.Split(new[] { "=" }, StringSplitOptions.None);

			var key = result.FirstOrDefault();
			var value = result.LastOrDefault();

			if (key == null || value == null || key.ToLower() == "error")
			{
				throw new InvalidOperationException($"Bad Response: {registerResponse}");
			}

			return value;
		}
	}
}