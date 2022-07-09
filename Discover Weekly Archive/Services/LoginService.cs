using System.Threading;
using System;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System.Collections.Generic;

namespace Discover_Weekly_Archive
{
    public class LoginService : ILoginService
    {
        private ApplicationConfig appConfig;
        private ISpotifyService spotifyService;
        private const int AUTH_SERVER_PORT = 5000;
        private const int AUTH_SERVER_TIMEOUT_LENGTH = 60 * 5;
        private readonly static Uri authServerAddress = new Uri($"http://localhost:{AUTH_SERVER_PORT}/callback");

        public LoginService(ApplicationConfig config, ISpotifyService spotify)
        {
            appConfig = config;
            spotifyService = spotify;
        }

        public async Task AuthWithSpotify()
        {
            var state = Guid.NewGuid().ToString();
            var loginURI = GenerateLoginURI(
                new List<string>()
                {
                    Scopes.PlaylistReadPrivate,
                    Scopes.PlaylistModifyPublic,
                    Scopes.PlaylistModifyPrivate,
                    Scopes.PlaylistReadCollaborative
                },
                state);
            Utility.OpenBrowser(loginURI.AbsoluteUri);
            var logginError = await WaitForLogin(state);
            if (logginError != null)
            {
                Console.WriteLine($"Login failed: {logginError}");
                return;
            }
            Console.WriteLine($"Now logged in as {appConfig.Account.DisplayName}.");

        }

        private Uri GenerateLoginURI(IList<string> scopes, string state)
        {
            var request = new LoginRequest(authServerAddress, appConfig.SpotifyApp.ClientId, LoginRequest.ResponseType.Code)
            {
                Scope = scopes,
                State = state
            };
            return request.ToUri();
        }

        private Task<string?> WaitForLogin(string state)
        {
            var tcs = new TaskCompletionSource<string?>();

            var server = new EmbedIOAuthServer(authServerAddress, AUTH_SERVER_PORT);
            server.AuthorizationCodeReceived += async (sender, response) =>
            {
                await server.Stop();
                if (response.State != state)
                {
                    tcs.SetResult("Given state parameter was not correct.");
                    return;
                }

                var tokenResponse = await spotifyService.OAuth.RequestToken(
                  new AuthorizationCodeTokenRequest(
                    appConfig.SpotifyApp.ClientId!,
                    appConfig.SpotifyApp.ClientSecret!,
                    response.Code,
                    server.BaseUri
                  )
                );
                appConfig.SpotifyToken.AccessToken = tokenResponse.AccessToken;
                appConfig.SpotifyToken.RefreshToken = tokenResponse.RefreshToken;
                appConfig.SpotifyToken.CreatedAt = tokenResponse.CreatedAt;
                appConfig.SpotifyToken.ExpiresIn = tokenResponse.ExpiresIn;
                appConfig.SpotifyToken.TokenType = tokenResponse.TokenType;
                await appConfig.Save();

                spotifyService.RefreshClients();
                var me = await spotifyService.Client.UserProfile.Current();

                appConfig.Account.Id = me.Id;
                appConfig.Account.DisplayName = me.DisplayName;
                appConfig.Account.Uri = me.Uri;

                server.Dispose();
                tcs.SetResult(null);
            };

            var ct = new CancellationTokenSource(TimeSpan.FromSeconds(AUTH_SERVER_TIMEOUT_LENGTH));
            ct.Token.Register(() =>
            {
                server.Stop();
                server.Dispose();
                tcs.TrySetCanceled();
            }, useSynchronizationContext: false);

            server.Start();

            return tcs.Task;
        }
    }
}