using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Octantis.Discord.Api
{
    public class RestApi
    {
        private static readonly string ApiBase = "https://discord.com/api/v9";

        private readonly IOptions<DiscordSettings> _settings;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<RestApi> _logger;
        private readonly HttpClient _httpClient;

        public RestApi(IOptions<DiscordSettings> settings, JsonSerializerOptions jsonOptions, ILogger<RestApi> logger, HttpClient httpClient)
        {
            _settings = settings;
            _jsonOptions = jsonOptions;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken) where T: class
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bot", _settings.Value.Token);
            var result = await _httpClient.GetAsync(ApiBase + url, cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch, error code '{StatusCode}'", result.StatusCode);
                return null;
            }

            var jsonText = await result.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonSerializer.Deserialize<T>(jsonText, _jsonOptions);
            _logger.LogTrace("GET {Url}: {Json}", ApiBase + url, jsonText);
            return data;
        }
    }
}