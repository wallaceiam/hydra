using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Hydra.Repositories;

namespace Hydra.Services
{
    public class Trader
    {
        private readonly IExchange exchange;
        public Trader(IExchange exchange)
        {
            this.exchange = exchange;
        }

        public async Task Execute(ArbPath arbPath)
        {
            // step 1 buy b with a
            // var result = await this.placeAndWatchOrder(arbPath.AtoB, "buy", "limit", arbPath.AtoBPrice, 50);
            // if(result.Abort)
            //     return;
            // // step 2 sell b to c
            // await this.placeAndWatchOrder(arbPath.BtoC, "sell", "limit", arbPath.BtoCPrice, result.Amount);
            // if(result.Abort)
            //     return;
            // // step 3 buy d with c
            // await this.placeAndWatchOrder(arbPath.CtoD, "buy", "limit", arbPath.CtoDPrice, result.Amount);
            // if(result.Abort)
            //     return;
            // // step 4 sell d to a
            // await this.placeAndWatchOrder(arbPath.DtoE, "sell", "limit", arbPath.DtoEPrice, result.Amount);
        }

        private async Task<object> placeAndWatchOrder(string pair, string type, string ordertype, decimal price, decimal amount)
        {
            var abort = false;
            object orderResult;
            while(true)
            {
                try
                {
                    orderResult = await this.exchange.AddOrderAsync(pair, type, ordertype, price, amount);
                    if(false) {
                        await Task.Delay(2000);
                    } else {
                        break;
                    }
                }
                catch(Exception ex) {
                    await Task.Delay(2000);
                }
                
            }

            if(abort)
                return null;

            object result = null;
            while(true)
            {
                try
                {
                    // result = await this.exchange.CheckOrderAsync(orderResult.TrnIds);
                    if(false) {
                        await Task.Delay(2000);
                    } else {
                        break;
                    }
                }
                catch(Exception ex) {
                    await Task.Delay(2000);
                }
                
            }

            return result;
        }
    }
}