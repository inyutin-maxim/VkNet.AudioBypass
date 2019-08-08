using JetBrains.Annotations;
using VkNet.AudioBypassService.Models;

namespace VkNet.AudioBypassService.Exceptions
{
	public class VkAuthException : System.Exception
	{
		[NotNull]
		public VkAuthError AuthError { get; }

		public VkAuthException([NotNull] VkAuthError vkAuthError) : base(vkAuthError.ErrorDescription ?? vkAuthError.Error)
		{
			AuthError = vkAuthError;
		}
	}
}