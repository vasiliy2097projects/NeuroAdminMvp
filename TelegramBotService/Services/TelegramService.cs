using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TelegramBotService.Models;

namespace TelegramBotService.Services
{
	public class TelegramService
	{
		private readonly HttpClient _httpClient;
		private readonly TelegramSettings _settings;
		private readonly ILogger<TelegramService> _logger;
		private readonly IConfiguration _configuration;

		public TelegramService(
			HttpClient httpClient,
			IOptions<TelegramSettings> settings,
			IConfiguration configuration,
			ILogger<TelegramService> logger)
		{
			_httpClient = httpClient;
			_settings = settings.Value;
			_configuration = configuration;
			_logger = logger;
		}

		public async Task SendMessageAsync(long chatId, string text)
		{
			try
			{
				var url = $"https://api.telegram.org/bot{_settings.BotToken}/sendMessage";
				var content = new
				{
					chat_id = chatId,
					text = text,
					parse_mode = "HTML"
				};

				var json = JsonSerializer.Serialize(content);
				var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync(url, stringContent);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Failed to send message. Status: {Status}, Response: {Response}",
						response.StatusCode, responseContent);
					throw new Exception($"Failed to send message: {responseContent}");
				}

				_logger.LogInformation("Message sent successfully");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error sending message");
				throw;
			}
		}

		public async Task SendPostGenerationRequestAsync(string topic)
		{
			try
			{
				var gigaChatServiceUrl = _configuration["GigaChatServiceUrl"] ?? "http://localhost:5067";
				var url = $"{gigaChatServiceUrl}/api/post/generate";

				var content = new
				{
					topic = topic
				};

				var json = JsonSerializer.Serialize(content);
				var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync(url, stringContent);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Failed to generate post. Status: {Status}, Response: {Response}",
						response.StatusCode, responseContent);
					throw new Exception($"Failed to generate post: {responseContent}");
				}

				_logger.LogInformation("Post generation request sent successfully");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error sending post generation request");
				throw;
			}
		}

		public async Task SetWebhookAsync(string webhookUrl)
		{
			try
			{
				var url = $"https://api.telegram.org/bot{_settings.BotToken}/setWebhook";
				var content = new
				{
					url = webhookUrl,
					allowed_updates = new[] { "message" }
				};

				var json = JsonSerializer.Serialize(content);
				var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync(url, stringContent);
				var responseContent = await response.Content.ReadAsStringAsync();

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Failed to set webhook. Status: {Status}, Response: {Response}",
						response.StatusCode, responseContent);
					throw new Exception($"Failed to set webhook: {responseContent}");
				}

				_logger.LogInformation("Webhook set successfully to: {WebhookUrl}", webhookUrl);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error setting webhook");
				throw;
			}
		}
	}
}