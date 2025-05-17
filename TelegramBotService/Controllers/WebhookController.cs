using Microsoft.AspNetCore.Mvc;
using TelegramBotService.Services;

namespace TelegramBotService.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class WebhookController : ControllerBase
	{
		private readonly TelegramService _telegramService;
		private readonly ILogger<WebhookController> _logger;

		public WebhookController(
			TelegramService telegramService,
			ILogger<WebhookController> logger)
		{
			_telegramService = telegramService;
			_logger = logger;
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] TelegramUpdate update)
		{
			try
			{
				_logger.LogInformation("Received webhook update: {@Update}", update);

				if (update?.Message?.Text != null)
				{
					var chatId = update.Message.Chat.Id;
					var text = update.Message.Text;

					_logger.LogInformation("Processing message: {Text} from chat {ChatId}", text, chatId);

					if (text.StartsWith("/generate"))
					{
						var topic = text.Replace("/generate", "").Trim();
						if (string.IsNullOrEmpty(topic))
						{
							await _telegramService.SendMessageAsync(chatId, "Пожалуйста, укажите тему поста после команды /generate. Например: /generate Искусственный интеллект");
							return Ok(new { status = "ok" });
						}

						await _telegramService.SendMessageAsync(chatId, $"Генерирую пост на тему: {topic}...");
						await _telegramService.SendPostGenerationRequestAsync(topic);
						await _telegramService.SendMessageAsync(chatId, "Пост успешно сгенерирован и опубликован!");
					}
					else
					{
						await _telegramService.SendMessageAsync(chatId, "Используйте команду generate для генерации поста");
					}
				}

				return Ok(new { status = "ok" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing webhook update");
				return StatusCode(500, new { error = "Internal server error" });
			}
		}
	}

	public class TelegramUpdate
	{
		public TelegramMessage? Message { get; set; }
	}

	public class TelegramMessage
	{
		public TelegramChat Chat { get; set; }
		public string? Text { get; set; }
	}

	public class TelegramChat
	{
		public long Id { get; set; }
	}
}