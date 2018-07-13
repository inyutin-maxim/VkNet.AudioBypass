using VkNet;
using Newtonsoft.Json.Linq;
using System.Net;
using VkNet.Model;
using VkNet.Utils;

namespace VkNetMusicBypass
{
    class Authorize
    {
        public VkApi api;
        private IReceiptParser receipt;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_api"></param>
        /// <param name="_receipt">Реализация парсера receipt'а</param>
        public Authorize(VkApi _api, IReceiptParser _receipt)
        {
            api = _api;
            receipt = _receipt;
        }

        public void Auth(string token)
        {
            api.Authorize(new ApiAuthParams()
            {
                AccessToken = token
            });
        }

        /// <summary>
        /// Получение токена через логин и пароль
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <returns></returns>
        public void Auth(string login, string password)
        {
            string URL = $"https://oauth.vk.com/token?grant_type=password&client_id=2274003&client_secret=hHbZxrka2uZ6jB1inYsH&username={login}&password={password}";
            var wc = new WebClient();
            string response = wc.DownloadString(URL);
            var json = JObject.Parse(response);

            wc.Dispose();
            Auth(json["access_token"].ToString());
            Auth(Bypass()); 
        }

        public string Bypass()
        {
            MessagesPushSettings messagesPush = new MessagesPushSettings()
            {
                NoSound = false,
                NoText = false
            };

            api.Account.RegisterDevice(new VkNet.Model.RequestParams.AccountRegisterDeviceParams()
            {
                Token = "LZA-UH0bUzN:APA91bHuxO87e5Mb975w4zBXuC-oNs61_SiRKg6c1oHHMefNTVbUacI5WKQ_Lfb_HlF-ZhGsXqvfzxcJk5Wcd03oGCFTkepUXt_lLrtcnKBzniXdkMFmuLpuvxR5lyts1D6lp9YnSXcDnv_bPjbJH0WWS7_7pPsHqg",
                DeviceId = "a3478da7eb6f0804",
                DeviceModel = "Razer",
                SystemVersion = "7.1.1",
                Settings = new PushSettings()
                {
                    FriendFound = true,
                    Birthday = true,
                    SdkOpen = true,
                    GroupAccepted = true,
                    WallPublish = true,
                    AppRequest = true,
                    WallPost = true,
                    GroupInvite = true,
                    FriendAccepted = true,
                    TagPhoto = true,
                    Comment = true,
                    EventSoon = true,
                    NewPost = true,
                    Reply = true,
                    Repost = true,
                    Mention = true,
                    Friend = true,
                    Like = true,
                    Chat = messagesPush,
                    Msg = messagesPush
                }
            });

            var response = api.Call("auth.refreshToken", new VkParameters()
                {
                    { "receipt", receipt.GetReceipt() }
                });

            return JObject.Parse(response.RawJson)["response"]["token"].ToString();
        }
    }
}
