using System;
using System.Collections.Generic;
using System.Net;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NLog;
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
        private readonly ILogger _logger;
        private readonly IReceiptParser _parser;
        #endregion

        public IWebProxy Proxy { get; set; }
        public AuthorizationResult Authorize()
        {
            var authResponse = BaseAuth();

            var authObject = JsonConvert.DeserializeObject<AuthorizationResult>(authResponse, 
                new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    }
                });

            var newToken = RefreshToken(authObject.AccessToken, _parser.GetReceipt());

            var authresult = new AuthorizationResult
            {
                AccessToken = newToken,
                ExpiresIn = authObject.ExpiresIn,
                UserId = authObject.UserId
            };

            return authresult;
        }

        public Authorization(IVkApiVersionManager versionManager, IRestClient restClient, IReceiptParser parser, [CanBeNull]ILogger logger)
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

        private string BaseAuth()
        {
            _logger?.Debug("1. Авторизация с помощью логина и пароля.");

            return Invoke("https://oauth.vk.com/token",
                new VkParameters
                {
                    { "grant_type", "password" },
                    { "client_id", "2274003" },
                    { "client_secret", "hHbZxrka2uZ6jB1inYsH" },
                    { "username", $"{_apiAuthParams.Login}" },
                    { "password", $"{_apiAuthParams.Password}" }
                });
        }

        private string RefreshToken(string oldToken, string receipt)
        {
            _logger?.Debug("2. Обновление токена.");

            var response = Invoke("https://api.vk.com/method/auth.refreshToken",
                new VkParameters
                {
                    {"receipt", receipt},
                    {"access_token", oldToken},
                    {"v", _versionManager.Version}
                });

            var json = JObject.Parse(response);

            return json["response"]["token"].ToString();
        }

        private string Invoke(string url, VkParameters parameters)
        {
            var response = _restClient.GetAsync(new Uri(url), parameters)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            var answer = response.Value ?? response.Message;

            VkErrors.IfErrorThrowException(answer);
            return answer;
        }

        #region NotImplemented
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
        #endregion
    }
}
