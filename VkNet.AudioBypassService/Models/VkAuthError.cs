using System;
using Newtonsoft.Json;

namespace VkNet.AudioBypassService.Models
{
	public class VkAuthError
	{
		[JsonProperty("error")]
		public string Error { get; set; }

		[JsonProperty("error_type")]
		public string ErrorType { get; set; }

		[JsonProperty("error_description")]
		public string ErrorDescription { get; set; }

		[JsonProperty("captcha_sid")]
		public ulong? CaptchaSid { get; set; }

		[JsonProperty("captcha_img")]
		public Uri CaptchaImg { get; set; }
	}
}