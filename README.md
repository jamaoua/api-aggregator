# API Aggregator

This project is an API Aggregation service built with ASP.NET Core. It fetches and combines data from multiple sources, including weather, news, and Spotify artist information.

## Features
- Retrieves current weather data using Open-Meteo API.
- Fetches top news headlines from NewsAPI.
- Gets Spotify artist data using the Spotify API.
- Supports optional filtering of news articles by author.

## Prerequisites
- .NET 6.0 or later
- API keys for NewsAPI and Spotify
- A registered Spotify application to obtain client credentials

## Installation
1. Clone the repository:
   ```sh
   git clone https://github.com/your-repo/api-aggregator.git
   cd api-aggregator
   ```
2. Install dependencies:
   ```sh
   dotnet restore
   ```
3. Update API keys in `AggregatedDataController.cs`:
   ```csharp
   private string newsApiKey = "YOUR_NEWSAPI_KEY";
   private string client_id = "YOUR_SPOTIFY_CLIENT_ID";
   private string client_secret = "YOUR_SPOTIFY_CLIENT_SECRET";
   ```
   Add your API key in the appropriate place before running the project.
4. Build the project:
   ```sh
   dotnet build
   ```

## Usage
Run the application with:
```sh
   dotnet run
```
The API will be available at:
```
http://localhost:5000/api/aggregateddata
```

### Endpoints
#### Get Aggregated Data
```
GET /api/aggregateddata?author=optional_author
```
- **Parameters**:
  - `author` (optional): Filters news articles by the given author name.
- **Response**:
  ```json
  {
    "Weather": { ... },
    "News": { ... },
    "Spotify": { ... }
  }
  ```

## Error Handling
If any API request fails, the error is logged, and the service attempts to continue returning available data.

## License
This project is open-source under the MIT License.

---
Feel free to modify this file based on additional configurations or deployment instructions!

