using System.Threading;
using System.Threading.Tasks;

namespace AuctionUpdateService.Interfaces
{
    internal interface IAuctionUpdater
    {
        abstract Task UpdateAuctions(CancellationToken cancellationToken);
    }
}
