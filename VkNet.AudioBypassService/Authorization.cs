using System;
using System.Collections.Generic;
using System.Net;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using VkNet.Abstractions.Core;
using VkNet.Abstractions.Utils;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Utils;

namespace VkNet.AudioBypassService
{
    public class Authorization : IBrowser
    {
        public Authorization(IVkApiVersionManager versionManager, IRestClient restClient, IReceiptParser parser,
            [CanBeNull] ILogger<Authorization> logger)
        {
            _versionManager = versionManager;
            _restClient = restClient;
            _parser = parser;
            _logger = logger;
        }

        public IWebProxy Proxy { get; set; }

        public AuthorizationResult Authorize()
        {
            var authResult = BaseAuth();

            var receipt = _parser.GetReceipt();

            if (receipt == null)
                throw new VkApiException("receipt is null");

            var newToken = RefreshToken(authResult.AccessToken, receipt);

            return new AuthorizationResult
            {
                AccessToken = newToken,
                ExpiresIn = authResult.ExpiresIn,
                UserId = authResult.UserId
            };
        }

        public void SetAuthParams(IApiAuthParams authParams)
        {
            _apiAuthParams = authParams;
        }

        private AuthorizationResult BaseAuth(string code = null)
        {
            if (string.IsNullOrEmpty(code))
                _logger?.LogDebug("1. Авторизация.");

            var response = Invoke("https://oauth.vk.com/token",
                new VkParameters
                {
                    {"grant_type", "password"},
                    {"client_id", "2274003"},
                    {"client_secret", "hHbZxrka2uZ6jB1inYsH"},
                    {"2fa_supported", true},
                    {"username", $"{_apiAuthParams.Login}"},
                    {"password", $"{_apiAuthParams.Password}"},
                    {"code", code}
                });

            var json = JObject.Parse(response);

            var error = json["error"];

            if (error == null)
                return json.ToObject<AuthorizationResult>(DefaultJsonSerializer);

            switch (error.ToString())
            {
                case "need_validation":
                    _logger?.LogDebug("1.1 Требуется код двухфакторной аутентификаци.");

                    if (_apiAuthParams.TwoFactorAuthorization == null)
                        throw new ArgumentNullException(nameof(_apiAuthParams.TwoFactorAuthorization));

                    var result = _apiAuthParams.TwoFactorAuthorization.BeginInvoke(null, null);
                    result.AsyncWaitHandle.WaitOne();
                    var authCode = _apiAuthParams.TwoFactorAuthorization.EndInvoke(result);

                    return BaseAuth(authCode);
                case "invalid_request":
                case "invalid_client":
                    var errorDescription = json["error_description"].ToString();
                    throw new VkApiAuthorizationException(errorDescription, _apiAuthParams.Login,
                        _apiAuthParams.Password);
                case "need_captcha":
                    var sid = json["captcha_sid"].Value<long>();
                    var imgUrl = json["captcha_img"].ToString();
                    throw new CaptchaNeededException(sid, imgUrl);
                default:
                    throw new VkApiException($"Неизвестная ошибка.{Environment.NewLine}{response}");
            }
        }

        private string Invoke(string url, VkParameters parameters)
        {
            var response = _restClient.PostAsync(new Uri(url), parameters)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            var answer = response.Value ?? response.Message;

            return answer;
        }

        private string RefreshToken(string oldToken, string receipt)
        {
            _logger?.LogDebug("2. Обновление токена.");

            var response = Invoke("https://api.vk.com/method/auth.refreshToken",
                new VkParameters
                {
                    {"receipt", receipt},
                    {"access_token", oldToken},
                    {"v", _versionManager.Version}
                });

            var jObject = JObject.Parse(response);

            var rawResponse = jObject["response"];

            return rawResponse["token"].ToString();
        }

        #region Private Fields

        private IApiAuthParams _apiAuthParams;

        private readonly IVkApiVersionManager _versionManager;

        private readonly IRestClient _restClient;

        private readonly ILogger<Authorization> _logger;

        private readonly IReceiptParser _parser;

        private JsonSerializer DefaultJsonSerializer => new JsonSerializer
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        #endregion

        #region Not Implemented

        public Uri CreateAuthorizeUrl(ulong clientId, ulong scope, Display display, string state)
        {
            throw new NotImplementedException();
        }

        public AuthorizationResult Validate(string validateUrl, string phoneNumber)
        {
            throw new NotImplementedException();
        }

        public string GetJson(string url, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            throw new NotImplementedException();
        }

        public AuthorizationResult Validate(string validateUrl)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}