using System.Threading.Tasks;

namespace Discover_Weekly_Archive
{
    public interface IDiscoverWeeklyArchiveService
    {
        Task Run();
        Task AddDiscoverWeeklyTracksToArchive();
    }
}