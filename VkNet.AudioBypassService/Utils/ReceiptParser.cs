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
			var response = await _safetyNetClient.CheckIn().ConfigureAwait(false);
			var receipt = await _safetyNetClient.GetReceipt(response).ConfigureAwait(false);

			return receipt?.Remove(0, 7);
		}
	}
}