using System;
using System.Net;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.Model.Attachments;
using VkNet.Utils;

namespace VkNet.AudioBypassService.Extensions
{
    public static class AudioDownloadExt
    {
        private static WebClient _webClient = new WebClient();

        public static void Download(this IAudioCategory audioCategory, Audio audio, string downloadPath)
        {
            Download(audioCategory, audio.Url, downloadPath);
        }

        public static void Download(this IAudioCategory audioCategory, string url, string downloadPath)
        {
            Download(audioCategory, new Uri(url), downloadPath);
        }

        public static void Download(this IAudioCategory audioCategory, Uri uri, string downloadPath)
        {
            _webClient.DownloadFile(uri, downloadPath);
        }

        public static Task DownloadAsync(this IAudioCategoryAsync audioCategory, Audio audio, string downloadPath)
        {
            return DownloadAsync(audioCategory, audio.Url, downloadPath);
        }

        public static Task DownloadAsync(this IAudioCategoryAsync audioCategory, string url, string downloadPath)
        {
            return DownloadAsync(audioCategory, new Uri(url), downloadPath);
        }

        public static Task DownloadAsync(this IAudioCategoryAsync audioCategory, Uri uri, string downloadPath)
        {
            return TypeHelper.TryInvokeMethodAsync(() => _webClient.DownloadFileAsync(uri, downloadPath));
        }
    }
}
