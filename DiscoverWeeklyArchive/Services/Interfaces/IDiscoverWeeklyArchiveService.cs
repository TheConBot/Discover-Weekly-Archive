using System.Threading.Tasks;

namespace DiscoverWeeklyArchive
{
    public interface IDiscoverWeeklyArchiveService
    {
        Task Run();
        Task AddDiscoverWeeklyTracksToArchive();
    }
}