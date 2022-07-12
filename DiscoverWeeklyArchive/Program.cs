using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Coravel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DiscoverWeeklyArchive
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            //var host = CreateHostBuilder(args).Build();
            //host.Run();
            //var services = host.Services;
            //var dwaService = services.GetService<IDiscoverWeeklyArchiveService>();
            //await dwaService.Run();
            //services.UseScheduler(scheduler =>
            //{
            //    //There isn't a clear time as to when Spotify updates the playlist on Mondays so lets just do it the next day -cb
            //    scheduler.ScheduleAsync(dwaService.AddDiscoverWeeklyTracksToArchive).Weekly().Tuesday();
            //    Console.WriteLine("Successfully initated scheduler.");
            //});
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
