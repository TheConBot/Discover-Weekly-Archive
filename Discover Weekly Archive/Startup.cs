using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Coravel;
using System;

namespace Discover_Weekly_Archive
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var task = ApplicationConfig.Load();
            var config = task.GetAwaiter().GetResult();
            services
                .AddSingleton(config)
                .AddSingleton<ISpotifyService, SpotifyService>()
                .AddSingleton<ILoginService, LoginService>()
                .AddSingleton<IDiscoverWeeklyArchiveService, DiscoverWeeklyArchiveService>()
                .AddScheduler();
        }

        public void Configure(IApplicationBuilder app)
        {
            var dwaService = app.ApplicationServices.GetService<IDiscoverWeeklyArchiveService>();
            var task = dwaService.Run();
            task.GetAwaiter().GetResult();
            app.ApplicationServices.UseScheduler(scheduler =>
            {
                //There isn't a clear time as to when Spotify updates the playlist on Mondays so lets just do it the next day -cb
                scheduler.ScheduleAsync(dwaService.AddDiscoverWeeklyTracksToArchive).Weekly().Tuesday();
                Console.WriteLine("Successfully initated scheduler.");
            });
        }
    }
}
