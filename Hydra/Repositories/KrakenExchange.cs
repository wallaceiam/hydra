using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Repositories
{
    public class KrakenExchange
    {
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
        public decimal air_decimals { get; set; }
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
