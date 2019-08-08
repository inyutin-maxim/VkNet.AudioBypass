using System.Threading.Tasks;

namespace VkNet.AudioBypassService.Abstractions
{
	public interface IReceiptParser
	{
		Task<string> GetReceipt();
	}
}