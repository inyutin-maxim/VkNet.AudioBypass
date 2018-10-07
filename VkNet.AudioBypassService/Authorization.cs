using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using VkNet.Abstractions;
using VkNet.Abstractions.Utils;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Utils;

namespace VkNet.AudioBypassService
{
    public class Authorization : IBrowser
    {
        #region Private Fields

        private IApiAuthParams _apiAuthParams;
        private readonly IVkApiVersionManager _versionManager;
        private readonly IRestClient _restClient;
        private readonly ILogger<Authorization> _logger;
        private readonly IReceiptParser _parser;

        #endregion

        public IWebProxy Proxy { get; set; }

        public AuthorizationResult Authorize()
        {
            var authResult = BaseAuth();

            var newToken = RefreshToken(authResult.AccessToken, _parser.GetReceipt());

            var authresult = new AuthorizationResult
            {
                AccessToken = newToken,
                ExpiresIn = authResult.ExpiresIn,
                UserId = authResult.UserId
            };

            return authresult;
        }

        public Authorization(IVkApiVersionManager versionManager, IRestClient restClient, IReceiptParser parser,
            ILogger<Authorization> logger)
        {
            _versionManager = versionManager;
            _restClient = restClient;
            _parser = parser;
            _logger = logger;
        }

        public void SetAuthParams(IApiAuthParams authParams)
        {
            _apiAuthParams = authParams;
        }

        private AuthorizationResult BaseAuth()
        {
            _logger?.LogDebug("1. Авторизация с помощью логина и пароля.");

            return Invoke<AuthorizationResult>("https://oauth.vk.com/token",
                new VkParameters
                {
                    {"grant_type", "password"},
                    {"client_id", "2274003"},
                    {"client_secret", "hHbZxrka2uZ6jB1inYsH"},
                    {"username", $"{_apiAuthParams.Login}"},
                    {"password", $"{_apiAuthParams.Password}"}
                });
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

            var json = JObject.Parse(response);

            return json["response"]["token"].Value<string>();
        }

        private T Invoke<T>(string url, VkParameters parameters)
        {
            var response = Invoke(url, parameters);

            return JsonConvert.DeserializeObject<T>(response, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            });
        }

        private string Invoke(string url, VkParameters parameters)
        {
            var response = _restClient.GetAsync(new Uri(url), parameters)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            var answer = response.Value ?? response.Message;

            return answer;
        }

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