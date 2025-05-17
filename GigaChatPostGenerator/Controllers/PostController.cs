using GigaChatPostGenerator.Models;
using GigaChatPostGenerator.Services;
using Microsoft.AspNetCore.Mvc;

namespace GigaChatPostGenerator.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PostController : ControllerBase
	{
		private readonly GigaChatService _gigaChatService;

		public PostController(GigaChatService gigaChatService)
		{
			_gigaChatService = gigaChatService;
		}

		[HttpPost("generate")]
		public async Task<IActionResult> GeneratePost([FromBody] PostRequest request)
		{
			try
			{
				var post = await _gigaChatService.GeneratePostAsync(request.Topic);
				return Ok(new { post });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = ex.Message });
			}
		}
	}
}