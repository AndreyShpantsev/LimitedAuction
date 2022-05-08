using System.Threading;
using System.Threading.Tasks;

namespace AuctionUpdateService.Interfaces
{
    internal interface IContractCreator
    {
        abstract Task CreateContracts(CancellationToken cancellationToken);
    }
}
