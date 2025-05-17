using GigaChatPostGenerator.Models;
using GigaChatPostGenerator.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace GigaChatPostGenerator.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PostController : ControllerBase
	{
		private readonly GigaChatService _gigaChatService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<PostController> _logger;

		public PostController(
			GigaChatService gigaChatService,
			IHttpClientFactory httpClientFactory,
			ILogger<PostController> logger)
		{
			_gigaChatService = gigaChatService;
			_httpClientFactory = httpClientFactory;
			_logger = logger;
		}

		[HttpPost("generate")]
		public async Task<IActionResult> GeneratePost([FromBody] PostRequest request)
		{
			try
			{
				// Generate post using GigaChat
				var post = await _gigaChatService.GeneratePostAsync(request.Topic);
				_logger.LogInformation("Post generated successfully");

				// Post to Telegram using TelegramPoster service
				var httpClient = _httpClientFactory.CreateClient();
				var telegramPosterUrl = "http://localhost:5167/api/post/publish";
				var content = new StringContent(
					JsonSerializer.Serialize(new { content = post }),
					Encoding.UTF8,
					"application/json"
				);

				var response = await httpClient.PostAsync(telegramPosterUrl, content);
				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Failed to post to Telegram channel");
					return StatusCode(500, new { error = "Failed to post to Telegram channel" });
				}

				_logger.LogInformation("Post successfully published to Telegram channel");
				return Ok(new { post });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in GeneratePost");
				return StatusCode(500, new { error = ex.Message });
			}
		}
	}
}