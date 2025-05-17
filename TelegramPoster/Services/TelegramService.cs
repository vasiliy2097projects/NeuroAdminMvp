using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TelegramPoster.Models;

namespace TelegramPoster.Services
{
	public class TelegramService
	{
		private readonly HttpClient _httpClient;
		private readonly TelegramSettings _settings;
		private readonly ILogger<TelegramService> _logger;

		public TelegramService(
			HttpClient httpClient,
			IOptions<TelegramSettings> settings,
			ILogger<TelegramService> logger)
		{
			_httpClient = httpClient;
			_settings = settings.Value;
			_logger = logger;
		}

		public async Task<bool> PostToChannelAsync(string message)
		{
			try
			{
				var channelId = _settings.ChannelId;
				if (!channelId.StartsWith("-100"))
				{
					channelId = $"-100{channelId.TrimStart('-')}";
				}

				var url = $"https://api.telegram.org/bot{_settings.BotToken}/sendMessage";
				_logger.LogDebug("Sending request to URL: {Url}", url);

				var content = new
				{
					chat_id = channelId,
					text = message,
					parse_mode = "HTML",
					disable_web_page_preview = true
				};

				var json = JsonSerializer.Serialize(content);
				_logger.LogDebug("Request content: {Content}", json);

				var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
				var requestContent = await stringContent.ReadAsStringAsync();
				_logger.LogDebug("String content: {Content}", requestContent);

				_logger.LogDebug("Sending POST request to Telegram API...");
				var response = await _httpClient.PostAsync(url, stringContent);
				var responseContent = await response.Content.ReadAsStringAsync();
				_logger.LogDebug("Response status: {Status}", response.StatusCode);
				_logger.LogDebug("Response content: {Content}", responseContent);

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Failed to send message to Telegram channel. Status: {Status}, Response: {Response}",
						response.StatusCode, responseContent);
					return false;
				}

				_logger.LogInformation("Message successfully sent to Telegram channel");
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error sending message to Telegram channel");
				return false;
			}
		}
	}
}