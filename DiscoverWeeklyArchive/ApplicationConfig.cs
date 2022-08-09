using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DiscoverWeeklyArchive
{
    public class ApplicationConfig
    {
        public static string AppConfigFilePath { get; set; }

        public AccountConfig Account { get; } = new AccountConfig();

        public SpotifyAppConfig SpotifyApp { get; } = new SpotifyAppConfig();

        public SpotifyTokenConfig SpotifyToken { get; } = new SpotifyTokenConfig();

        public DiscoverWeeklyArchiveConfig DiscoverWeeklyArchiveConfig { get; } = new DiscoverWeeklyArchiveConfig();


        public async Task Save()
        {
            await File.WriteAllTextAsync(AppConfigFilePath, JsonConvert.SerializeObject(this), Encoding.UTF8);
        }

        public void Delete()
        {
            File.Delete(AppConfigFilePath);
        }

        public static async Task<ApplicationConfig> Load(string path)
        {
            AppConfigFilePath = Path.Combine(path, "config.json");
            if (!File.Exists(AppConfigFilePath))
            {
                var config = new ApplicationConfig();
                await config.Save();
                return config;
            }
            var configContent = await File.ReadAllTextAsync(AppConfigFilePath);
            return JsonConvert.DeserializeObject<ApplicationConfig>(configContent);
        }
    }

    public class AccountConfig
    {
        public string? Id { get; set; } = default!;

        public string? DisplayName { get; set; } = default!;

        public string? Uri { get; set; } = default!;
    }

    public class SpotifyAppConfig
    {
        public string? ClientId { get; set; } = default!;

        public string? ClientSecret { get; set; } = default!;
    }

    public class SpotifyTokenConfig
    {
        public string? AccessToken { get; set; } = default!;

        public string? RefreshToken { get; set; } = default!;

        public int? ExpiresIn { get; set; } = default!;

        public string? TokenType { get; set; } = default!;

        public DateTime? CreatedAt { get; set; } = default!;
    }

    public class DiscoverWeeklyArchiveConfig
    {
        public string? ArchivePlaylistID { get; set; } = default!;
    }
}
