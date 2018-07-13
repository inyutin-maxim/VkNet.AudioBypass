using VkNet;
using Newtonsoft.Json.Linq;
using System.Net;
using VkNet.Model;
using VkNet.Utils;
using System;

namespace VkNetMusicBypass
{
    class Authorize
    {
        public VkApi api;

        public Authorize(VkApi _api)
        {
            api = _api;
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


            string[] receipt = new string[] { "zEXKaMARS_h:APA91bHXsKM-p7TA1cQ_dkKsDD3dk6pTlvS7egJvkfzNXpNsYIpyYZeHIiMfCgHeEKOaQ-RRJcma7vrDZF7gCWnqtkXA9TRjewqg8iXOJCdxoVuPHi-23qkGjDg0ygGfl6k_APr1Vb9COTPi9KDvMVDG4a9MUZ_waQ",
                    "7AJXiv9OMFx:APA91bF6eR6NGn3SSIkL152A610JzdjC616tBb0TBoyENXOVAQDeLb1JdZEn4RTvkV3Hoxn0VATuO4U1fTHOJCfpieAvFDVM_GjFgkkLAZev2bPdyA9rqDkjUK5AzboRP-nXoJ6t1FMRkyNIWYNnB66xFrLPInq1KA",
                    "zIA9dkystyi:APA91bGLOkfX_Maad2q_MgLV6BmSHXdeQ99K5GbGnvVZ7BLLYXdapQahJXCb1AEOafFAoMf6GY-ql0UedVK6EZd6dIUcIn8ALgqW-KMNzs1yKGOe-Jakxe8Tfr7bCkFbY6UDA5RCph4KqqIilZDEEFZ8w8PPzWAAQA" };

            var response = api.Call("auth.refreshToken", new VkParameters()
                {
                    { "receipt", receipt[new Random().Next(receipt.Length)] }
                });

            return JObject.Parse(response.RawJson)["response"]["token"].ToString();
        }
    }
}
