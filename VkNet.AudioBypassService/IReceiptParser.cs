using JetBrains.Annotations;

namespace VkNet.AudioBypassService
{
    public interface IReceiptParser
    {
        [CanBeNull]
        string GetReceipt();
    }
}
