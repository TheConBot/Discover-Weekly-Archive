using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace DiscoverWeeklyArchive
{
    public class SpotifyService : ISpotifyService
    {
        private ApplicationConfig appConfig;
        private SpotifyClientConfig clientConfig;
        private SpotifyClient? spotifyClient;
        private OAuthClient oauth;

        public SpotifyService(ApplicationConfig appConfig)
        {
            this.appConfig = appConfig;
            RefreshClients();
        }

        public void RefreshClients()
        {
            if (!string.IsNullOrEmpty(appConfig.SpotifyToken.RefreshToken))
            {
                // We're logged in as a user
                Console.WriteLine($"Initiating Spotify Client as {appConfig.Account.DisplayName}.");
                clientConfig = CreateForUser();
                spotifyClient = new SpotifyClient(clientConfig);

            }
            else if (
                !string.IsNullOrEmpty(appConfig.SpotifyApp.ClientId)
                && !string.IsNullOrEmpty(appConfig.SpotifyApp.ClientSecret)
            )
            {
                clientConfig = CreateForCredentials();
                spotifyClient = new SpotifyClient(clientConfig);
            }
            else
            {
                clientConfig = SpotifyClientConfig.CreateDefault();
            }

            oauth = new OAuthClient(clientConfig);
        }

        private SpotifyClientConfig CreateForUser()
        {
            return SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(new AuthorizationCodeAuthenticator(
                appConfig.SpotifyApp.ClientId!,
                appConfig.SpotifyApp.ClientSecret!,
                new AuthorizationCodeTokenResponse
                {
                    AccessToken = appConfig.SpotifyToken.AccessToken!,
                    CreatedAt = (DateTime)appConfig.SpotifyToken.CreatedAt!,
                    RefreshToken = appConfig.SpotifyToken.RefreshToken!,
                    ExpiresIn = (int)appConfig.SpotifyToken.ExpiresIn!,
                    TokenType = appConfig.SpotifyToken.TokenType!,
                }
                ))
                .WithRetryHandler(new SimpleRetryHandler());
        }

        private SpotifyClientConfig CreateForCredentials()
        {
            return SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(new ClientCredentialsAuthenticator(
                appConfig.SpotifyApp.ClientId!,
                appConfig.SpotifyApp.ClientSecret!,
                string.IsNullOrEmpty(appConfig.SpotifyToken.AccessToken) ? null : new ClientCredentialsTokenResponse
                {
                    AccessToken = appConfig.SpotifyToken.AccessToken!,
                    CreatedAt = (DateTime)appConfig.SpotifyToken.CreatedAt!,
                    ExpiresIn = (int)appConfig.SpotifyToken.ExpiresIn!,
                    TokenType = appConfig.SpotifyToken.TokenType!,
                }
                ))
                .WithRetryHandler(new SimpleRetryHandler());
        }

        public bool IsUserLoggedIn => Client != null && (ClientConfig.Authenticator is AuthorizationCodeAuthenticator);

        public bool AreCredentialsSet => Client != null && ClientConfig.Authenticator != null;

        public SpotifyClientConfig ClientConfig { get => clientConfig; }

        public SpotifyClient? Client { get => spotifyClient; }

        public OAuthClient OAuth { get => oauth; }
    }
}
