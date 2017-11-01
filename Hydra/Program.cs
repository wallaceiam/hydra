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
            var repo = new KrakenExchange("", "");

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

            var paths = new List<ArbPath>();
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
                            // if(bc.Contains("GBP"))
                            //     continue;

                            foreach (var cd in possibilities.Where(x => x != ab && x != de && x != bc))
                            {
                                if (result.Result[cd].Base == result.Result[de].Base &&
                                    result.Result[bc].Quote == result.Result[cd].Quote &&
                                    result.Result[bc].Base != result.Result[cd].Base)
                                {
                                    paths.Add(
                                        new ArbPath()
                                        {
                                            AtoB = ab,
                                            BtoC = bc,
                                            CtoD = cd,
                                            DtoE = de
                                        });
                                }
                            }

                        }
                    }
                }
            }

            var priceData = repo.GetTickerInformationAsync(possibilities).Result;

            // work out each path

            foreach (var path in paths)
            {
                var starting = path.Starting;

                // var abPrice = priceData.Result[ab].Ask[0];
                // var bcPrice = priceData.Result[bc].Bid[0];
                // var cdPrice = priceData.Result[cd].Ask[0];
                // var dePrice = priceData.Result[de].Bid[0];
                path.AtoBPrice = priceData.Result[path.AtoB].Last[0];
                path.BtoCPrice = priceData.Result[path.BtoC].Last[0];
                path.CtoDPrice = priceData.Result[path.CtoD].Last[0];
                path.DtoEPrice = priceData.Result[path.DtoE].Last[0];

                var abAmount = starting / path.AtoBPrice;
                var bcAmount = abAmount * path.BtoCPrice;
                var cdAmount = bcAmount / path.CtoDPrice;
                path.Ending = cdAmount * path.DtoEPrice;
            }

            foreach (var path in paths.Where(x => x.Perc > 0.02M).OrderByDescending(x => x.Ending))
            {
                Console.WriteLine($"{path.AtoB.PadRight(8, ' ')}->{path.BtoC.PadRight(8, ' ')}->{path.CtoD.PadRight(8, ' ')}->{path.DtoE.PadRight(8, ' ')}:\t{path.Ending:0.00}\t{path.Perc:P2}");
                Console.WriteLine($"{priceData.Result[path.AtoB].Volume[0].ToString().PadRight(8, ' ')}->{priceData.Result[path.BtoC].Volume[0].ToString().PadRight(8, ' ')}->{priceData.Result[path.CtoD].Volume[0].ToString().PadRight(8, ' ')}->{priceData.Result[path.DtoE].Volume[0].ToString().PadRight(8, ' ')}");
            }

            var first = paths.Where(x => x.Perc > 0.02M).OrderByDescending(x => x.Ending).FirstOrDefault();
            if(first != null) 
            {
                // start the trade
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }

    public class ArbPath
    {
        public string AtoB { get; set; }
        public decimal AtoBPrice {get;set;}
        public string BtoC { get; set; }
        public decimal BtoCPrice {get;set;}
        public string CtoD { get; set; }
        public decimal CtoDPrice {get;set;}
        public string DtoE { get; set; }
        public decimal DtoEPrice {get;set;}

        public decimal Starting { get; set; } = 100.0M;
        public decimal Ending { get; set; }
        public decimal Perc { get { return (this.Ending - this.Starting) / this.Starting; } }
    }
}
