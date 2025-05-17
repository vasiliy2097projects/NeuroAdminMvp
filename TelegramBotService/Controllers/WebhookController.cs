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

					// Echo the message back
					await _telegramService.SendMessageAsync(chatId, $"You said: {text}");
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