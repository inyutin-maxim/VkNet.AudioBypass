using System;
using System.Linq;

namespace VkNet.AudioBypassService.Utils
{
	internal static class RandomString
	{
		private const string Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";

		private static readonly Random Random = new Random();

		public static string Generate(int length)
		{
			return new string(Enumerable.Range(1, length).Select(_ => Chars[Random.Next(Chars.Length)]).ToArray());
		}
	}
}