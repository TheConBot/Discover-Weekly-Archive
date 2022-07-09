using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discover_Weekly_Archive
{
    public interface ILoginService
    {
        Task AuthWithSpotify();
    }
}