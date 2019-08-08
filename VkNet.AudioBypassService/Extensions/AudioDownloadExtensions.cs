using System;
using System.Threading.Tasks;
using Flurl.Http;
using VkNet.Abstractions;
using VkNet.Model.Attachments;

namespace VkNet.AudioBypassService.Extensions
{
	public static class AudioDownloadExtensions
	{
		public static Task DownloadAsync(this IAudioCategoryAsync audioCategory, Audio audio, string downloadPath)
		{
			return DownloadAsync(audioCategory, audio.Url, downloadPath);
		}

		public static Task DownloadAsync(this IAudioCategoryAsync audioCategory, Uri uri, string downloadPath)
		{
			return DownloadAsync(audioCategory, uri.ToString(), downloadPath);
		}

		public static Task DownloadAsync(this IAudioCategoryAsync audioCategory, string url, string downloadPath)
		{
			return url.DownloadFileAsync(downloadPath);
		}

		public static void Download(this IAudioCategory audioCategory, Audio audio, string downloadPath)
		{
			DownloadAsync(audioCategory, audio.Url, downloadPath).GetAwaiter().GetResult();
		}

		public static void Download(this IAudioCategory audioCategory, string url, string downloadPath)
		{
			DownloadAsync(audioCategory, new Uri(url), downloadPath).GetAwaiter().GetResult();
		}

		public static void Download(this IAudioCategory audioCategory, Uri uri, string downloadPath)
		{
			DownloadAsync(audioCategory, uri, downloadPath).GetAwaiter().GetResult();
		}
	}
}