using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using VkNet.Abstractions.Authorization;
using VkNet.Abstractions.Core;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService.Abstractions;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Utils.AntiCaptcha;

namespace VkNet.AudioBypassService.Utils
{
    /// <inheritdoc />
    [UsedImplicitly]
    public class VkAndroidAuthorization : IAuthorizationFlow
    {
        public VkAndroidAuthorization([NotNull] IVkApiVersionManager versionManager,
            [NotNull] IRestClient restClient,
            [NotNull] IReceiptParser parser,
            [CanBeNull] ICaptchaSolver captchaSolver,
            [CanBeNull] ILogger<VkAndroidAuthorization> logger)
        {
            _versionManager = versionManager;
            _restClient = restClient;
            _parser = parser;
            _captchaSolver = captchaSolver;
            _logger = logger;
        }

        public async Task<AuthorizationResult> AuthorizeAsync()
        {
            _logger?.LogDebug("1. Авторизация");
            var authResult = await BaseAuth();

            _logger?.LogDebug("2. Получение receipt");
            var receipt = await _parser.GetReceipt();

            if (string.IsNullOrWhiteSpace(receipt))
                throw new VkApiException("receipt is null or empty");

            _logger?.LogDebug("3. Обновление токена");
            var newToken = await RefreshToken(authResult.AccessToken, receipt);

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

        private async Task<string> GetJson(string url, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var response = await _restClient.PostAsync(new Uri(url), parameters)
                .ConfigureAwait(false);

            var answer = response.Value ?? response.Message;

            return answer;
        }

        private async Task<JObject> CallBase(string url, IEnumerable<KeyValuePair<string, string>> parameters,
            bool handleError = false)
        {
            var response = await GetJson(url, parameters).ConfigureAwait(false);

            if (handleError) VkErrors.IfErrorThrowException(response);

            return JObject.Parse(response);
        }

        private async Task<VkResponse> Call(string methodName, IEnumerable<KeyValuePair<string, string>> parameters,
            bool handleError = false)
        {
            var url = $"https://api.vk.com/method/{methodName}";
            var jObject = await CallBase(url, parameters, handleError).ConfigureAwait(false);

            var response = jObject["response"];

            return new VkResponse(response) {RawJson = jObject.ToString()};
        }

        private async Task<AuthorizationResult> BaseAuth()
        {
            var response = await CallBase("https://oauth.vk.com/token", new VkParameters
            {
                {"grant_type", "password"},
                {"client_id", "2274003"},
                {"client_secret", "hHbZxrka2uZ6jB1inYsH"},
                {"2fa_supported", _apiAuthParams.TwoFactorSupported ?? true},
                {"force_sms", _apiAuthParams.ForceSms},
                {"username", _apiAuthParams.Login},
                {"password", _apiAuthParams.Password},
                {"captcha_sid", _apiAuthParams.CaptchaSid},
                {"captcha_key", _apiAuthParams.CaptchaKey},
                {"code", _apiAuthParams.Code},
                {"scope", $"{_apiAuthParams.Settings}"},
                {"v", _versionManager.Version}
            });

            var error = response["error"];

            if (error == null)
                return response.ToObject<AuthorizationResult>(DefaultJsonSerializer);

            switch (error.ToString())
            {
                case "need_validation":
                    _logger?.LogDebug("Требуется ввести код двухфакторной авторизации");

                    if (_apiAuthParams.TwoFactorAuthorization == null)
                        throw new ArgumentNullException(nameof(_apiAuthParams.TwoFactorAuthorization));

                    var result = _apiAuthParams.TwoFactorAuthorization.Invoke();
                    _apiAuthParams.Code = result;

                    return await BaseAuth();
                case "invalid_request":
                case "invalid_client":
                    var errorDescription = response["error_description"].ToString();
                    throw new VkAuthorizationException(errorDescription);
                case "need_captcha":
                    _logger?.LogDebug("Капча");

                    var sid = response["captcha_sid"].Value<long>();
                    var imgUrl = response["captcha_img"].ToString();

                    if (_captchaSolver == null) throw new CaptchaNeededException(sid, imgUrl);

                    var input = _captchaSolver.Solve(imgUrl);

                    _apiAuthParams.CaptchaSid = sid;
                    _apiAuthParams.CaptchaKey = input;

                    return await BaseAuth();
                default:
                    throw new VkApiException($"Неизвестная ошибка:{Environment.NewLine}{response}");
            }
        }

        public async Task<string> RefreshToken(string oldToken, string receipt)
        {
            var response = await Call("auth.refreshToken", new VkParameters
            {
                {"receipt", receipt},
                {"access_token", oldToken},
                {"v", _versionManager.Version}
            }, true);

            return response["token"];
        }

        #region Private Fields

        private IApiAuthParams _apiAuthParams;

        private readonly IVkApiVersionManager _versionManager;

        private readonly IRestClient _restClient;

        private readonly ILogger<VkAndroidAuthorization> _logger;

        private readonly IReceiptParser _parser;

        private readonly ICaptchaSolver _captchaSolver;

        private JsonSerializer DefaultJsonSerializer => new JsonSerializer
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        #endregion
    }
}