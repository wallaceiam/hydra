using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.Repositories
{
    public interface IExchange
    {
        Task<AssetPairs> GetTradableAssetPairsAsync();
        Task<TickerInformation> GetTickerInformationAsync(List<string> assetPairs);

        Task<object> AddOrderAsync(string pair, string type, string ordertype, decimal price, decimal amount);

    }
}
