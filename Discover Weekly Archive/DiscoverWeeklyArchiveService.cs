using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using System.Linq;
using Coravel;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace Discover_Weekly_Archive
{
    /// <summary>
    /// This is a tool to replace the functionality that was available via the IFTTT applet where everytime your Discover Weekly is updated, it adds all then new tracks to a seperate playlist to save them as the Discover Weekly playlist overwrites itself every week. For the underlying boilerplate services that enable interaction with Spotify via it's API, I borrowed heavily from https://github.com/JohnnyCrazy/Sp0.
    /// </summary>
    public class DiscoverWeeklyArchiveService : IDiscoverWeeklyArchiveService
    {
        private ISpotifyService spotifyService;
        private ILoginService loginService;
        private readonly IHostApplicationLifetime appLifetime;
        private ApplicationConfig appConfig;

        public DiscoverWeeklyArchiveService(ApplicationConfig appConfig, ISpotifyService spotifyService, ILoginService loginService, IHostApplicationLifetime appLifetime)
        {
            this.appConfig = appConfig;
            this.spotifyService = spotifyService;
            this.appLifetime = appLifetime;
            this.loginService = loginService;
        }

        public async Task Run()
        {
            Console.WriteLine("Starting DWA service...");
            try
            {
                while (!spotifyService.AreCredentialsSet)
                {
                    await ValidateCredentials();
                }
                while (!spotifyService.IsUserLoggedIn)
                {
                    await loginService.AuthWithSpotify();
                }
                if (string.IsNullOrEmpty(appConfig.DiscoverWeeklyArchiveConfig.ArchivePlaylistID))
                {
                    await SetupArchivePlaylist();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
                appLifetime.StopApplication();
            }
        }

        public async Task AddDiscoverWeeklyTracksToArchive()
        {
            Func<SimplePlaylist, bool> DiscoverWeeklyPredicate = playlist => playlist.Name == "Discover Weekly" && playlist.Owner.DisplayName == "Spotify";

            var playlists = await spotifyService.Client.Playlists.CurrentUsers(new PlaylistCurrentUsersRequest() { Limit = 50 });
            var discoverWeeklyID = playlists.Items.FirstOrDefault(DiscoverWeeklyPredicate).Id;
            if (string.IsNullOrEmpty(discoverWeeklyID))
            {
                //For some reason getting the Discover Weekly normally via Playlists.CurrentUsers() wasn't working so this was my workaround, and then it randomly started working...? So adding this here as a backup incase the above randomly breaks -cb
                var search = await spotifyService.Client.Search.Item(new SearchRequest(SearchRequest.Types.Playlist, "Discover+Weekly"));
                discoverWeeklyID = search.Playlists.Items.FirstOrDefault(DiscoverWeeklyPredicate).Id;
                if (string.IsNullOrEmpty(discoverWeeklyID))
                {
                    Console.WriteLine("ERROR: Could not find your Discover Weekly. Make sure you are following it.");
                    //appLifetime.StopApplication();
                }
            }
            var discoverWeekly = await spotifyService.Client.Playlists.Get(discoverWeeklyID);
            await spotifyService.Client.Playlists.AddItems(appConfig.DiscoverWeeklyArchiveConfig.ArchivePlaylistID, new PlaylistAddItemsRequest(discoverWeekly.Tracks.Items.Select(track => (track.Track as FullTrack).Uri).ToList()));
            Console.WriteLine("Added this week's Discover Weekly to the archive. See ya next week!");
        }

        private async Task SetupArchivePlaylist()
        {
            var playlists = await spotifyService.Client.Playlists.CurrentUsers(new PlaylistCurrentUsersRequest() { Limit = 50 });
            Console.WriteLine("Please select the playlist you wish to use as your Discover Weekly Archive.");
            for (int i = 0; i < playlists.Total; i++)
            {
                var playlist = playlists.Items[i];
                Console.WriteLine($"{i}: {playlist.Name}");
            }
            int selectedPlaylistIndex = -1;
            while (selectedPlaylistIndex < 0 || selectedPlaylistIndex >= playlists.Total)
            {
                if (!int.TryParse(Console.ReadLine(), out selectedPlaylistIndex))
                {
                    Console.WriteLine("Invalid input. Please select one of the numbers that are next to the playlist names above.");
                }
            }
            appConfig.DiscoverWeeklyArchiveConfig.ArchivePlaylistID = playlists.Items[selectedPlaylistIndex].Id;
            await appConfig.Save();
            Console.WriteLine($"Successfully selected playlist: {playlists.Items[selectedPlaylistIndex].Name}.");
        }

        private async Task ValidateCredentials()
        {
            if (string.IsNullOrEmpty(appConfig.SpotifyApp.ClientId) || string.IsNullOrEmpty(appConfig.SpotifyApp.ClientSecret))
            {
                Console.WriteLine("Could not find a valid Client ID or a Client Secret. Please enter those values now.");
                Console.WriteLine("Client ID: ");
                appConfig.SpotifyApp.ClientId = Console.ReadLine();
                Console.WriteLine("Client Secret: ");
                appConfig.SpotifyApp.ClientSecret = Console.ReadLine();
                await appConfig.Save();
                spotifyService.RefreshClients();
            }
        }
    }
}
