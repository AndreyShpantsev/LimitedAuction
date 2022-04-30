using System.Threading;
using System.Threading.Tasks;

namespace AuctionUpdateService.Interfaces
{
    internal interface IBaseScopedService
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
