using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ApiAggregator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AggregatedDataController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AggregatedDataController> _logger;

        ///NEWS CREDENTIALS
        private string newsApiKey = "";

        ///SPOTIFY CREDENTIALS
        private string client_id = "";
        private string client_secret = "";

        ///WEATHER INPUTS
        private double latitude = 37.98;
        private double longitude = 23.72;

        //SPOTIFY ARTIST INPUT
        private string artistUri = "4Z8W4fKeB5YxbusRsdQVPb";


        public AggregatedDataController(HttpClient httpClient,ILogger<AggregatedDataController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            
        }

        [HttpGet]
        public async Task<IActionResult> GetAggregatedData(string? author = null)
        {
            try
            {
                //URLS
                var weatherApiUrl = $"https://api.open-meteo.com/v1/forecast?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&current_weather=true";
                var newsApiUrl = "https://newsapi.org/v2/top-headlines?country=us&apiKey="+ newsApiKey;
                var spotifyTokenUrl = "https://accounts.spotify.com/api/token";
                
                //FETCHING FUNCTIONS
                var weatherTask = SimpleFetch(weatherApiUrl);
                var newsTask = HeadersFetch(newsApiUrl);
                var spotifyArtistTask = GetSpotifyToken(spotifyTokenUrl);

                ///ALL TASKS
                await Task.WhenAll(weatherTask, newsTask, spotifyArtistTask);

                //DESERIALIZATION CLASSES
                var weatherData = JsonConvert.DeserializeObject<WeatherResponse>(weatherTask.Result);
                var newsData = JsonConvert.DeserializeObject<NewsResponse>(newsTask.Result);
                var spotifyArtist = spotifyArtistTask.Result;

                ///FILTERING
                if (!string.IsNullOrEmpty(author))
                {
                    newsData.Articles = newsData.Articles
                        .Where(article => article.Author?.Equals(author, StringComparison.OrdinalIgnoreCase) ?? false)
                        .ToList();
                }

                ////DATA
                var aggregatedData = new
                {
                    Weather = weatherData,
                    News = newsData,
                    Spotify = spotifyArtist
                };

                return Ok(aggregatedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching aggregated data");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<string> SimpleFetch(string url)
        {
            try
            {
                var response =_httpClient.GetStringAsync(url);
                return response.Result;
            }
            catch (Exception ex) {
                return null;
            }
        }

        private async Task<string> HeadersFetch(string url)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; MyApp/1.0)");
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch news data");
                return null;
            }
        }

        private async Task<SpotifyArtist> GetSpotifyToken(string spotifyTokenUrl)
        {
            var spotifyApiUrl = "https://api.spotify.com/v1/artists/"+ artistUri;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, spotifyTokenUrl)
                {
                    Content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("client_id", client_id),
                        new KeyValuePair<string, string>("client_secret", client_secret)
                    })
                };

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var tokenResponse = await response.Content.ReadAsStringAsync();
                var tokenResponseObj = JsonConvert.DeserializeObject<TokenResponse>(tokenResponse);

                if (tokenResponseObj == null)
                {
                    return null;
                }

                var spotifyData = await GetSpotifyArtistData(tokenResponseObj.access_token, spotifyApiUrl);
                var spotifyArtist = JsonConvert.DeserializeObject<SpotifyArtist>(spotifyData);


                return spotifyArtist;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve Spotify token");
                return null;
            }
        }

        private async Task<string> GetSpotifyArtistData(string accessToken, string apiUrl)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
                var response = await _httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Spotify artist data");
                return null;
            }
        }
    }

public class TokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
    }

    public class WeatherResponse
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Generationtime_ms { get; set; }
        public int Utc_offset_seconds { get; set; }
        public string Timezone { get; set; }
        public string Timezone_abbreviation { get; set; }
        public double Elevation { get; set; }
        public CurrentWeatherUnits Current_weather_units { get; set; }
        public CurrentWeather Current_weather { get; set; }
    }

    public class CurrentWeatherUnits
    {
        public string Time { get; set; }
        public string Interval { get; set; }
        public string Temperature { get; set; }
        public string Windspeed { get; set; }
        public string Winddirection { get; set; }
        public string Is_day { get; set; }
        public string Weathercode { get; set; }
    }

    public class CurrentWeather
    {
        public string Time { get; set; }
        public int Interval { get; set; }
        public double Temperature { get; set; }
        public double Windspeed { get; set; }
        public int Winddirection { get; set; }
        public int Is_day { get; set; }
        public int Weathercode { get; set; }
    }

    public class NewsResponse
    {
        public string Status { get; set; }
        public int TotalResults { get; set; }
        public List<Article> Articles { get; set; }
    }

    public class Article
    {
        public Source Source { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string UrlToImage { get; set; }
        public DateTime PublishedAt { get; set; }
        public string Content { get; set; }
    }

    public class Source
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class SpotifyArtist
    {
        [JsonProperty("external_urls")]
        public ExternalUrls ExternalUrls { get; set; }

        [JsonProperty("followers")]
        public Followers Followers { get; set; }

        [JsonProperty("genres")]
        public List<string> Genres { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("images")]
        public List<Image> Images { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("popularity")]
        public int Popularity { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }
    }

    public class ExternalUrls
    {
        [JsonProperty("spotify")]
        public string Spotify { get; set; }
    }

    public class Followers
    {
        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }

    public class Image
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }
    }
}
