using SpotifyAPI.Web;

namespace Discover_Weekly_Archive
{
    public interface ISpotifyService
    {
        void RefreshClients();

        SpotifyClientConfig ClientConfig { get; }

        SpotifyClient? Client { get; }

        OAuthClient OAuth { get; }

        bool IsUserLoggedIn { get; }

        bool AreCredentialsSet { get; }
    }
}