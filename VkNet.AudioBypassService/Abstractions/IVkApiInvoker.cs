using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VkNet.Utils;

namespace VkNet.AudioBypassService.Abstractions
{
	public interface IVkApiInvoker
	{
		Task<T> CallAsync<T>(Uri uri, VkParameters parameters, CancellationToken cancellationToken = default);

		Task<JToken> CallAsync(Uri uri, VkParameters parameters, CancellationToken cancellationToken = default);

		Task<T> CallAsync<T>(string methodName, VkParameters parameters, CancellationToken cancellationToken = default);

		Task<JToken> CallAsync(string methodName, VkParameters parameters, CancellationToken cancellationToken = default);
	}
}