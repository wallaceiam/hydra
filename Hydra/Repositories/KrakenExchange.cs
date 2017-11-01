using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Repositories
{
    public class KrakenExchange
    {
        private readonly string secret;
        private readonly string key;

        public KrakenExchange(string secret, string key)
        {
            this.secret = secret;
            this.key = key;
        }

        //  https://api.kraken.com/0/public/AssetPairs
        public async Task<AssetPairs> GetTradableAssetPairsAsync()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                client.DefaultRequestHeaders.Add("User-Agent", ".Hyrda");

                var settings = new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                };

                var serializer = new DataContractJsonSerializer(typeof(AssetPairs), settings);

                var streamTask = client.GetStreamAsync("https://api.kraken.com/0/public/AssetPairs");
                var repositories = serializer.ReadObject(await streamTask) as AssetPairs;

                return repositories;
            }
        }

        public async Task<TickerInformation> GetTickerInformationAsync(List<string> assetPairs)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                client.DefaultRequestHeaders.Add("User-Agent", ".Hyrda");

                var settings = new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                };

                var serializer = new DataContractJsonSerializer(typeof(TickerInformation), settings);

                var streamTask = client.GetStreamAsync("https://api.kraken.com/0/public/Ticker?pair=" + string.Join(",", assetPairs));
                var repositories = serializer.ReadObject(await streamTask) as TickerInformation;

                return repositories;
            }
        }

        public async Task<object> AddOrderAsync(string pair, string type, string ordertype, decimal price, decimal amount)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                client.DefaultRequestHeaders.Add("User-Agent", ".Hyrda");

                var nonce = DateTime.UtcNow.Ticks;

                var postData = $"nonce={nonce}&pair={pair}&type={type}&ordertype={ordertype}&price={price}&volume={amount}";

                this.AddHeaders(client.DefaultRequestHeaders, nonce, postData, "/0/private/AddOrder");

                var settings = new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                };

                var serializer = new DataContractJsonSerializer(typeof(TickerInformation), settings);

                var content = new StringContent(postData);

                var stream = await client.PostAsync("https://api.kraken.com/0/private/AddOrder", content);
                var repositories = serializer.ReadObject(await stream.Content.ReadAsStreamAsync()) as TickerInformation;

                return repositories;
            }
        }

        private void AddHeaders(HttpRequestHeaders headers, Int64 nonce, string postData, string path)
        {
            headers.Add("API-Key", this.key);

            byte[] base64DecodedSecred = Convert.FromBase64String(this.secret);

            var np = nonce + Convert.ToChar(0) + postData;

            var pathBytes = Encoding.UTF8.GetBytes(path);
            var hash256Bytes = sha256_hash(np);
            var z = new byte[pathBytes.Count() + hash256Bytes.Count()];
            pathBytes.CopyTo(z, 0);
            hash256Bytes.CopyTo(z, pathBytes.Count());

            var signature = getHash(base64DecodedSecred, z);

            headers.Add("API-Sign", Convert.ToBase64String(signature));
        }

        #region Helper methods

        private byte[] sha256_hash(String value)
        {
            using (var hash = SHA256Managed.Create())
                return hash.ComputeHash(Encoding.UTF8.GetBytes(value));
        }

        private byte[] getHash(byte[] keyByte, byte[] messageBytes)
        {
            using (var hmacsha512 = new HMACSHA512(keyByte))
                return hmacsha512.ComputeHash(messageBytes);
        }

        #endregion Helper methods

    }

    [DataContract]
    public class AssetPairs
    {
        //public object error { get; set; }
        [DataMember(Name = "result")]
        public Dictionary<string, AssetPair> Result { get; set; }
    }

    [DataContract]
    public class TickerInformation
    {
        [DataMember(Name = "result")]
        public Dictionary<string, Ticker> Result { get; set; }
    }

    [DataContract]
    public class AssetPair
    {
        [DataMember(Name = "altname")]
        public string AltName { get; set; }
        [DataMember(Name = "aclass_base")]
        public string AClassBase { get; set; }
        [DataMember(Name = "base")]
        public string Base { get; set; }
        [DataMember(Name = "aclass_quote")]
        public string AClassQuote { get; set; }
        [DataMember(Name = "quote")]
        public string Quote { get; set; }
        public string lot { get; set; }
        [DataMember(Name = "pair_decimals")]
        public decimal PairDecimals { get; set; }
        public decimal lot_decimals { get; set; }
        public decimal lot_multiplier { get; set; }
        //"leverage_buy":[

        //],
        //"leverage_sell":[

        //],
        //"fees":[  ],
        //"fees_maker":[  ],
        public string fee_volume_currency { get; set; }
        public decimal margin_call { get; set; }
        public decimal margin_stop { get; set; }
    }

    [DataContract]
    public class Ticker
    {
        [DataMember(Name = "a")]
        public List<decimal> Ask { get; set; }
        [DataMember(Name = "b")]
        public List<decimal> Bid { get; set; }

        [DataMember(Name ="c")]
        public List<decimal> Last { get; set; }

        [DataMember(Name = "v")]
        public List<decimal> Volume { get; set; }
    }
}
