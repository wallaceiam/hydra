using Hydra.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hydra
{
    class Program
    {
        static void Main(string[] args)
        {
            var repo = new KrakenExchange();

            var result = repo.GetTradableAssetPairsAsync().Result;

            // get all possibilities
            var possibilities = new List<string>();
            foreach (var key in result.Result.Keys)
            {
                if (result.Result[key].Base.StartsWith("Z") || result.Result[key].Quote.StartsWith("Z"))
                {
                    if (!key.EndsWith(".d"))
                        possibilities.Add(key);
                }
            }

            var paths = new Dictionary<string, object>();
            foreach (var pair in possibilities)
            {
                if (pair.Contains("EUR"))
                {
                    var ab = pair;
                    var aquote = result.Result[ab].Quote;
                    var des = possibilities.Where(x => x != ab && result.Result[x].Quote == aquote);
                    foreach (var de in des)
                    {
                        foreach (var bc in possibilities.Where(x => x != ab && x != de && result.Result[x].Base == result.Result[ab].Base && result.Result[x].Quote != aquote))
                        {
                            foreach (var cd in possibilities.Where(x => x != ab && x != de && x != bc))
                            { 
                                if (result.Result[cd].Base == result.Result[de].Base &&
                                    result.Result[bc].Quote == result.Result[cd].Quote &&
                                    result.Result[bc].Base != result.Result[cd].Base)
                                {
                                    paths.Add(ab + "|" + bc + "|" + cd + "|" + de, null);
                                }
                            }

                        }
                    }
                }
            }

            var priceData = repo.GetTickerInformationAsync(possibilities).Result;

            // work out each path

            foreach(var path in paths.Keys)
            {
                var starting = 100.0M;
                var parts = path.Split('|');
                var ab = parts[0];
                var bc = parts[1];
                var cd = parts[2];
                var de = parts[3];

                //var abPrice = priceData.Result[ab].Ask[0];
                //var bcPrice = priceData.Result[bc].Bid[0];
                //var cdPrice = priceData.Result[cd].Ask[0];
                //var dePrice = priceData.Result[de].Bid[0];
                var abPrice = priceData.Result[ab].Last[0];
                var bcPrice = priceData.Result[bc].Last[0];
                var cdPrice = priceData.Result[cd].Last[0];
                var dePrice = priceData.Result[de].Last[0];

                var abAmount = starting / abPrice;
                var bcAmount = abAmount * bcPrice;
                var cdAmount = bcAmount / cdPrice;
                var ending = cdAmount * dePrice;

                var perc = ((ending - starting) / starting);
                if(perc > 0.05M)
                    Console.WriteLine($"{path}\t\t{ending}\t{perc:P2}");
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
