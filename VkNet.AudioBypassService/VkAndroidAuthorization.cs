using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using VkNet.Abstractions.Authorization;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Exceptions;
using VkNet.AudioBypassService.Utils;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Utils;

namespace VkNet.AudioBypassService
{
	/// <inheritdoc />
	[UsedImplicitly]
	public class VkAndroidAuthorization : IAuthorizationFlow
	{
		#region Private Fields

		private IApiAuthParams _apiAuthParams;

		[NotNull]
		private readonly IVkApiInvoker _vkApiInvoker;

		[CanBeNull]
		private readonly ILogger<VkAndroidAuthorization> _logger;

		#endregion

		[NotNull]
		public IReceiptParser ReceiptParser { get; }

		public VkAndroidAuthorization([NotNull] IVkApiInvoker vkApiInvoker,
									  [NotNull] IReceiptParser parser,
									  [CanBeNull] ILogger<VkAndroidAuthorization> logger)
		{
			_vkApiInvoker = vkApiInvoker;
			ReceiptParser = parser;
			_logger = logger;
		}

		public async Task<AuthorizationResult> AuthorizeAsync(CancellationToken token = default)
		{
			_logger?.LogDebug("1. Авторизация");
			var authResult = await BaseAuthAsync().ConfigureAwait(false);

			_logger?.LogDebug("2. Получение receipt");
			var receipt = await ReceiptParser.GetReceipt().ConfigureAwait(false);

			if (string.IsNullOrWhiteSpace(receipt))
			{
				throw new VkApiException("receipt is null or empty");
			}

			_logger?.LogDebug("3. Обновление токена");
			var newToken = await RefreshTokenAsync(authResult.AccessToken, receipt).ConfigureAwait(false);

			return new AuthorizationResult
			{
				AccessToken = newToken,
				ExpiresIn = authResult.ExpiresIn,
				UserId = authResult.UserId
			};
		}

		public void SetAuthorizationParams(IApiAuthParams authorizationParams)
		{
			_apiAuthParams = authorizationParams;
		}

		private async Task<AuthorizationResult> BaseAuthAsync()
		{
			try
			{
				var parameters = new VkParameters
				{
					{ "grant_type", "password" },
					{ "client_id", "2274003" },
					{ "client_secret", "hHbZxrka2uZ6jB1inYsH" },
					{ "2fa_supported", _apiAuthParams.TwoFactorSupported ?? true },
					{ "force_sms", _apiAuthParams.ForceSms },
					{ "username", _apiAuthParams.Login },
					{ "password", _apiAuthParams.Password },
					{ "code", _apiAuthParams.Code },
					{ "scope", "all" },
					{ "device_id", RandomString.Generate(16) }
				};

				return await _vkApiInvoker.CallAsync<AuthorizationResult>(new Uri("https://oauth.vk.com/token"), parameters).ConfigureAwait(false);
			}
			catch (VkAuthException exception)
			{
				switch (exception.AuthError.Error)
				{
					case "need_validation":
						_logger?.LogDebug("Требуется ввести код двухфакторной авторизации");

						if (_apiAuthParams.TwoFactorAuthorization == null)
						{
							throw new
								InvalidOperationException($"Two-factor authorization required, but {nameof(_apiAuthParams.TwoFactorAuthorization)} callback is null. Set {nameof(_apiAuthParams.TwoFactorAuthorization)} callback to handle two-factor authorization.");
						}

						var result = _apiAuthParams.TwoFactorAuthorization();
						_apiAuthParams.Code = result;

						return await BaseAuthAsync().ConfigureAwait(false);
					default:
						throw;
				}
			}
		}

		public async Task<string> RefreshTokenAsync(string oldToken, string receipt)
		{
			var parameters = new VkParameters
			{
				{ "receipt", receipt },
				{ "access_token", oldToken }
			};

			var response = await _vkApiInvoker.CallAsync("auth.refreshToken", parameters).ConfigureAwait(false);

			return response["token"]?.ToString();
		}
	}
}