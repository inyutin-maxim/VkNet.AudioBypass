using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using VkNet.Abstractions.Utils;
using VkNet.Utils;

namespace VkNet.AudioBypassService
{
    public class ReceiptParser : IReceiptParser
    {
        private readonly IRestClient _restClient;
        public ReceiptParser(IRestClient restClient)
        {
            _restClient = restClient;
        }
        public string GetReceipt()
        {
            var answer = _restClient.GetAsync(new Uri("http://www.dp1337.somee.com/update/fixmyfuckingmusic.aspx"),
                new VkParameters
                {
                    { "v", "0.6.2b" },
                    { "b", "exp" }
                }).ConfigureAwait(false).GetAwaiter().GetResult();

            var json = JObject.Parse(answer.Value);

            var receipts = json["rec"].ToObject<IList<string>>();

            return receipts.ElementAt(new Random().Next(receipts.Count));
        }
    }
}
