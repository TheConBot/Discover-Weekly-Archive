using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Coravel;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace DiscoverWeeklyArchive
{
    public class Startup
    {
        public string ContentRootPath { get; set; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            ContentRootPath = env.ContentRootPath;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var task = ApplicationConfig.Load(ContentRootPath);
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
                //dwaService.AddDiscoverWeeklyTracksToArchive();
                Console.WriteLine("Successfully initated scheduler.");
            });
        }
    }
}
