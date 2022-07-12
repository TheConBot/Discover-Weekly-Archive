using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscoverWeeklyArchive
{
    public interface ILoginService
    {
        Task AuthWithSpotify();
    }
}