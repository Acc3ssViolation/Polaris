using System.Net.Http;
using System.Text;
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

        public async Task<R?> PostAsync<T, R>(string url, T data, CancellationToken cancellationToken) where T: class where R: class
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bot", _settings.Value.Token);
            var jsonRequest = JsonSerializer.Serialize<T>(data, _jsonOptions);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var result = await _httpClient.PostAsync(ApiBase + url, content, cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                _logger.LogTrace("POST {Url}: {Json}", ApiBase + url, jsonRequest);
                _logger.LogError("Failed to fetch, error code '{StatusCode}'", result.StatusCode);
                return null;
            }

            var jsonResponse = await result.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<R>(jsonResponse, _jsonOptions);
            _logger.LogTrace("POST {Url}: {Json} -> {Json}", ApiBase + url, jsonRequest, jsonResponse);
            return responseData;
        }
    }
}