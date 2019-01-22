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

        public string GetReceipt()
        {
            var response = _safetyNetClient.CheckIn().Result;
            var receipt = _safetyNetClient.GetReceipt(response).Result;

            return receipt.Remove(0, 7);
        }
    }
}