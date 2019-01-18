using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using ProtoBuf;
using VkNet.Abstractions.Utils;

namespace VkNet.AudioBypassService
{
    public class ReceiptParser : IReceiptParser
    {

        private readonly Random _random;

        // ReSharper disable once UnusedParameter.Local
        // (leave restClient for compatibility)
        public ReceiptParser(IRestClient restClient)
        {
            _random = new Random();
        }

        public string GetReceipt()
        {
            AndroidCheckinResponse protoCheckIn;
            using (var http = new HttpClient()) 
            {
                http.DefaultRequestHeaders.UserAgent.ParseAdd("Android-GCM/1.5 (generic_x86 KK)");
                http.DefaultRequestHeaders.Add("Expect", "");
                http.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-protobuffer");
                var payload = new byte[] { // TODO: use protobuf serializer instead
                    0x10, 0x00, 0x1a, 0x2a, 0x31, 0x2d, 0x39, 0x32, 0x39, 0x61, 0x30, 0x64, 0x63, 0x61, 0x30, 0x65,
                    0x65, 0x65, 0x35, 0x35, 0x35, 0x31, 0x33, 0x32, 0x38, 0x30, 0x31, 0x37, 0x31, 0x61, 0x38, 0x35,
                    0x38, 0x35, 0x64, 0x61, 0x37, 0x64, 0x63, 0x64, 0x33, 0x37, 0x30, 0x30, 0x66, 0x38, 0x22, 0xe3,
                    0x01, 0x0a, 0xbf, 0x01, 0x0a, 0x45, 0x67, 0x65, 0x6e, 0x65, 0x72, 0x69, 0x63, 0x5f, 0x78, 0x38,
                    0x36, 0x2f, 0x67, 0x6f, 0x6f, 0x67, 0x6c, 0x65, 0x5f, 0x73, 0x64, 0x6b, 0x5f, 0x78, 0x38, 0x36,
                    0x2f, 0x67, 0x65, 0x6e, 0x65, 0x72, 0x69, 0x63, 0x5f, 0x78, 0x38, 0x36, 0x3a, 0x34, 0x2e, 0x34,
                    0x2e, 0x32, 0x2f, 0x4b, 0x4b, 0x2f, 0x33, 0x30, 0x37, 0x39, 0x31, 0x38, 0x33, 0x3a, 0x65, 0x6e,
                    0x67, 0x2f, 0x74, 0x65, 0x73, 0x74, 0x2d, 0x6b, 0x65, 0x79, 0x73, 0x12, 0x06, 0x72, 0x61, 0x6e,
                    0x63, 0x68, 0x75, 0x1a, 0x0b, 0x67, 0x65, 0x6e, 0x65, 0x72, 0x69, 0x63, 0x5f, 0x78, 0x38, 0x36,
                    0x2a, 0x07, 0x75, 0x6e, 0x6b, 0x6e, 0x6f, 0x77, 0x6e, 0x32, 0x0e, 0x61, 0x6e, 0x64, 0x72, 0x6f,
                    0x69, 0x64, 0x2d, 0x67, 0x6f, 0x6f, 0x67, 0x6c, 0x65, 0x40, 0x85, 0xb5, 0x86, 0x06, 0x4a, 0x0b,
                    0x67, 0x65, 0x6e, 0x65, 0x72, 0x69, 0x63, 0x5f, 0x78, 0x38, 0x36, 0x50, 0x13, 0x5a, 0x19, 0x41,
                    0x6e, 0x64, 0x72, 0x6f, 0x69, 0x64, 0x20, 0x53, 0x44, 0x4b, 0x20, 0x62, 0x75, 0x69, 0x6c, 0x74,
                    0x20, 0x66, 0x6f, 0x72, 0x20, 0x78, 0x38, 0x36, 0x62, 0x07, 0x75, 0x6e, 0x6b, 0x6e, 0x6f, 0x77,
                    0x6e, 0x6a, 0x0e, 0x67, 0x6f, 0x6f, 0x67, 0x6c, 0x65, 0x5f, 0x73, 0x64, 0x6b, 0x5f, 0x78, 0x38,
                    0x36, 0x70, 0x00, 0x10, 0x00, 0x32, 0x06, 0x33, 0x31, 0x30, 0x32, 0x36, 0x30, 0x3a, 0x06, 0x33,
                    0x31, 0x30, 0x32, 0x36, 0x30, 0x42, 0x0b, 0x6d, 0x6f, 0x62, 0x69, 0x6c, 0x65, 0x3a, 0x4c, 0x54,
                    0x45, 0x3a, 0x48, 0x00, 0x32, 0x05, 0x65, 0x6e, 0x5f, 0x55, 0x53, 0x38, 0xf0, 0xb4, 0xdf, 0xa6,
                    0xb9, 0x9a, 0xb8, 0x83, 0x8e, 0x01, 0x52, 0x0f, 0x33, 0x35, 0x38, 0x32, 0x34, 0x30, 0x30, 0x35,
                    0x31, 0x31, 0x31, 0x31, 0x31, 0x31, 0x30, 0x5a, 0x00, 0x62, 0x10, 0x41, 0x6d, 0x65, 0x72, 0x69,
                    0x63, 0x61, 0x2f, 0x4e, 0x65, 0x77, 0x5f, 0x59, 0x6f, 0x72, 0x6b, 0x70, 0x03, 0x7a, 0x1c, 0x37,
                    0x31, 0x51, 0x36, 0x52, 0x6e, 0x32, 0x44, 0x44, 0x5a, 0x6c, 0x31, 0x7a, 0x50, 0x44, 0x56, 0x61,
                    0x61, 0x65, 0x45, 0x48, 0x49, 0x74, 0x64, 0x2b, 0x59, 0x67, 0x3d, 0xa0, 0x01, 0x00, 0xb0, 0x01, 0x00
                };
                var result = http.PostAsync("https://android.clients.google.com/checkin", 
                    new ByteArrayContent(payload)).Result;
                result.EnsureSuccessStatusCode();
                protoCheckIn =
                    Serializer.Deserialize<AndroidCheckinResponse>(result.Content.ReadAsStreamAsync().Result);
            }

            using (var tcp = new TcpClient("mtalk.google.com", 5228))
            {
                // Body data (protobuf, but i didn't found any .proto files, so...)
                byte[] message1 = {
                    0x0a, 0x0a, 0x61, 0x6e, 0x64, 0x72, 0x6f, 0x69, 0x64, 0x2d, 0x31, 0x39, 0x12, 0x0f, 0x6d, 0x63,
                    0x73, 0x2e, 0x61, 0x6e, 0x64, 0x72, 0x6f, 0x69, 0x64, 0x2e, 0x63, 0x6f, 0x6d, 0x1a
                };
                byte[] message2 = { 0x22 };
                byte[] message3 = { 0x2a };
                byte[] message4 = { 0x32 };
                byte[] message5 = {
                    0x42, 0x0b, 0x0a, 0x06, 0x6e, 0x65, 0x77, 0x5f, 0x76, 0x63, 0x12, 0x01, 0x31, 0x60, 0x00, 0x70,
                    0x01, 0x80, 0x01, 0x02, 0x88, 0x01, 0x01
                };
                byte[] message6 = { 0x29, 0x02 };
                
                byte[] idStringBytes = Encoding.ASCII.GetBytes(protoCheckIn.AndroidId.ToString());
                List<byte> idLen = VarInt.Write(idStringBytes.Length).ToList();

                byte[] tokenStringBytes = Encoding.ASCII.GetBytes(protoCheckIn.SecurityToken.ToString());
                List<byte> tokenLen = VarInt.Write(tokenStringBytes.Length).ToList();

                string hexId = "android-" + protoCheckIn.AndroidId;
                byte[] hexIdBytes = Encoding.ASCII.GetBytes(hexId);
                IEnumerable<byte> hexIdLen = VarInt.Write(hexIdBytes.Length);

                List<byte> body = message1
                    .Concat(idLen)
                    .Concat(idStringBytes)
                    .Concat(message2)
                    .Concat(idLen)
                    .Concat(idStringBytes)
                    .Concat(message3)
                    .Concat(tokenLen)
                    .Concat(tokenStringBytes)
                    .Concat(message4)
                    .Concat(hexIdLen)
                    .Concat(hexIdBytes)
                    .Concat(message5)
                    .ToList();
                IEnumerable<byte> bodyLen = VarInt.Write(body.Count);

                byte[] payload = message6
                    .Concat(bodyLen)
                    .Concat(body)
                    .ToArray();
                //

                using (SslStream ssl = new SslStream(tcp.GetStream(), false,
                    (sender, certificate, chain, errors) => errors == SslPolicyErrors.None)) 
                {
                    ssl.AuthenticateAsClient("mtalk.google.com");
                    ssl.Write(payload);
                    ssl.Flush();
                    ssl.ReadByte(); // skip byte
                    int responseCode = ssl.ReadByte();
                    if (responseCode != 3)  // success if second byte == 3 
                    {
                        throw new InvalidOperationException($"MTalk failed, expected 3, got {responseCode}");
                    }
                }
                
            }

            var appid = GenerateRandomString(11);

            string receipt;

            using (var http = new HttpClient()) {
                http.DefaultRequestHeaders.UserAgent.ParseAdd("Android-GCM/1.5 (generic_x86 KK)");
                http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization",
                    $"AidLogin {protoCheckIn.AndroidId}:{protoCheckIn.SecurityToken}");
                
                var param = new Dictionary<string, string>
                {
                    {"X-scope", "GCM"},
                    {"X-osv", "23"},
                    {"X-subtype", "54740537194"},
                    {"X-app_ver", "443"},
                    {"X-kid", "|ID|1|"},
                    {"X-appid", appid},
                    {"X-gmsv", "13283005"},
                    {"X-cliv", "iid-10084000"},
                    {"X-app_ver_name", "51.2 lite"},
                    {"X-X-kid", "|ID|1|"},
                    {"X-subscription", "54740537194"},
                    {"X-X-subscription", "54740537194"},
                    {"X-X-subtype", "54740537194"},
                    {"app", "com.perm.kate_new_6"},
                    {"sender", "54740537194"},
                    {"device", Convert.ToString(protoCheckIn.AndroidId)},
                    {"cert", "966882ba564c2619d55d0a9afd4327a38c327456"},
                    {"app_ver", "443"},
                    {"info", "g57d5w1C4CcRUO6eTSP7b7VoT8yTYhY"},
                    {"gcm_ver", "13283005"},
                    {"plat", "0"},
                    {"X-messenger2", "1"}
                };
                

                var result1 = http.PostAsync("https://android.clients.google.com/c2dm/register3", 
                    new FormUrlEncodedContent(param)).Result;
                result1.EnsureSuccessStatusCode();
                var body1 = result1.Content.ReadAsStringAsync().Result;
                if (body1.Contains("Error")) {
                    throw new InvalidOperationException($"C2DM registration #1 error ({body1})");
                }
                
                param["X-scope"] = "id";
                param["X-kid"] = "|ID|2|";
                param["X-X-kid"] = "|ID|2|";
                
                var result2 = http.PostAsync("https://android.clients.google.com/c2dm/register3", 
                    new FormUrlEncodedContent(param)).Result;
                result2.EnsureSuccessStatusCode();
                var body2 = result2.Content.ReadAsStringAsync().Result;
                if (body2.Contains("Error")) {
                    throw new InvalidOperationException($"C2DM registration #2 error ({body2})");
                }

                receipt = body2.Substring(13);
            }
            return receipt;
        }
        
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";
        
        private string GenerateRandomString(int length) 
        {
            var sb = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                sb.Append(Alphabet[_random.Next(Alphabet.Length)]);
            }
            return sb.ToString();
        }
    }
}