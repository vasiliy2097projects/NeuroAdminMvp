using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GigaChatPostGenerator.Services
{
	public class GigaChatService
	{
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;
		private string _accessToken;
		private DateTime _tokenExpiration;
		private readonly ILogger<GigaChatService> _logger;

		public GigaChatService(IConfiguration configuration, ILogger<GigaChatService> logger)
		{
			var handler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
			};
			_httpClient = new HttpClient(handler);
			_apiKey = configuration["GigaChat:ApiKey"] ?? "NDc4NzcwZGMtMGI0ZS00OWJkLWIyYjUtOGM4YTYxMWZjYTk1OjQwOGNlZjUwLTZjYzMtNGRkNS05NjkwLTlhMzQyMTcyY2E4MQ==";
			_logger = logger;
		}

		private async Task<string> GetAccessTokenAsync()
		{
			// Если токен существует и не истек, возвращаем его
			if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiration)
			{
				_logger.LogInformation("Using existing token");
				return _accessToken;
			}

			try
			{
				var authUrl = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";

				// Используем FormUrlEncodedContent вместо JSON
				var content = new FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string, string>("scope", "GIGACHAT_API_PERS")
				});

				// Устанавливаем заголовки
				_httpClient.DefaultRequestHeaders.Clear();
				_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _apiKey);
				_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				_httpClient.DefaultRequestHeaders.Add("RqUID", Guid.NewGuid().ToString());

				_logger.LogInformation("Requesting new token");
				_logger.LogInformation("Request URL: {Url}", authUrl);
				_logger.LogInformation("Request Headers: {Headers}", string.Join(", ", _httpClient.DefaultRequestHeaders.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));

				var response = await _httpClient.PostAsync(authUrl, content);
				var responseContent = await response.Content.ReadAsStringAsync();

				_logger.LogInformation("Token response status: {Status}", response.StatusCode);
				_logger.LogInformation("Token response content: {Content}", responseContent);

				if (!response.IsSuccessStatusCode)
				{
					throw new Exception($"Failed to get access token: {response.StatusCode}. Response: {responseContent}");
				}

				var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
				_accessToken = result.GetProperty("access_token").GetString()
					?? throw new Exception("Access token not found in response");

				// Устанавливаем время истечения токена (30 минут)
				_tokenExpiration = DateTime.UtcNow.AddMinutes(30);

				_logger.LogInformation("Received new token");
				return _accessToken;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting access token");
				throw new Exception($"Failed to get access token: {ex.Message}", ex);
			}
		}

		public async Task<string> GeneratePostAsync(string topic)
		{
			try
			{
				var token = await GetAccessTokenAsync();
				var request = new HttpRequestMessage(HttpMethod.Post, "https://gigachat.devices.sberbank.ru/api/v1/chat/completions");
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var prompt = $"Напиши интересный пост на тему: {topic}. Пост должен быть информативным и привлекательным.";
				var requestBody = new
				{
					model = "GigaChat-Pro",
					messages = new[]
					{
						new { role = "system", content = "Ты - помощник для создания постов." },
						new { role = "user", content = prompt }
					},
					temperature = 0.7,
					max_tokens = 1000,
					stream = false,
					top_p = 0.9,
					frequency_penalty = 0.0,
					presence_penalty = 0.0
				};

				var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});

				_logger.LogInformation("Sending request to GigaChat with body: {Body}", jsonContent);

				request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

				var response = await _httpClient.SendAsync(request);
				var responseContent = await response.Content.ReadAsStringAsync();

				_logger.LogInformation("GigaChat response status: {Status}", response.StatusCode);
				_logger.LogInformation("GigaChat response content: {Content}", responseContent);

				if (!response.IsSuccessStatusCode)
				{
					throw new Exception($"GigaChat API error: {response.StatusCode}. Response: {responseContent}");
				}

				var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
				return responseJson.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error generating post");
				throw new Exception($"Error generating post: {ex.Message}", ex);
			}
		}
	}
}