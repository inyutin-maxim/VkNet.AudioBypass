using System;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.Model;

namespace VkNet.AudioBypassService.Extensions
{
	public static class AudioDownloadExtensions
	{
		[Obsolete("Avoid using this method, it will be removed in future releases.")]
		public static Task DownloadAsync(this IAudioCategoryAsync audioCategory, Audio audio, string downloadPath)
		{
			return DownloadAsync(audioCategory, audio.Url, downloadPath);
		}

		[Obsolete("Avoid using this method, it will be removed in future releases.")]
		public static Task DownloadAsync(this IAudioCategoryAsync audioCategory, Uri uri, string downloadPath)
		{
			return DownloadAsync(audioCategory, uri.ToString(), downloadPath);
		}

		[Obsolete("Avoid using this method, it will be removed in future releases.")]
		public static Task DownloadAsync(this IAudioCategoryAsync audioCategory, string url, string downloadPath)
		{
			return Task.FromResult(0);
		}

		[Obsolete("Avoid using this method, it will be removed in future releases.")]
		public static void Download(this IAudioCategory audioCategory, Audio audio, string downloadPath)
		{
			DownloadAsync(audioCategory, audio.Url, downloadPath).GetAwaiter().GetResult();
		}

		[Obsolete("Avoid using this method, it will be removed in future releases.")]
		public static void Download(this IAudioCategory audioCategory, string url, string downloadPath)
		{
			DownloadAsync(audioCategory, new Uri(url), downloadPath).GetAwaiter().GetResult();
		}

		[Obsolete("Avoid using this method, it will be removed in future releases.")]
		public static void Download(this IAudioCategory audioCategory, Uri uri, string downloadPath)
		{
			DownloadAsync(audioCategory, uri, downloadPath).GetAwaiter().GetResult();
		}
	}
}