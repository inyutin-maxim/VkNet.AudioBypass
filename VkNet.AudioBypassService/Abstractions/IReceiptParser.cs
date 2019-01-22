using JetBrains.Annotations;

namespace VkNet.AudioBypassService.Abstractions
{
    public interface IReceiptParser
    {
        [CanBeNull]
        string GetReceipt();
    }
}