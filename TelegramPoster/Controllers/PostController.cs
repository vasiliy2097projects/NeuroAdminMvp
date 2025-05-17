using Microsoft.AspNetCore.Mvc;
using TelegramPoster.Models;
using TelegramPoster.Services;

namespace TelegramPoster.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PostController : ControllerBase
	{
		private readonly TelegramService _telegramService;
		private readonly ILogger<PostController> _logger;

		public PostController(
			TelegramService telegramService,
			ILogger<PostController> logger)
		{
			_telegramService = telegramService;
			_logger = logger;
		}

		[HttpPost("publish")]
		public async Task<IActionResult> PublishPost([FromBody] PostRequest request)
		{
			try
			{
				if (string.IsNullOrEmpty(request.Content))
				{
					return BadRequest("Content cannot be empty");
				}

				var success = await _telegramService.PostToChannelAsync(request.Content);
				if (!success)
				{
					return StatusCode(500, "Failed to publish post to Telegram channel");
				}

				return Ok(new { message = "Post successfully published to Telegram channel" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error publishing post to Telegram channel");
				return StatusCode(500, "An error occurred while publishing the post");
			}
		}
	}
}