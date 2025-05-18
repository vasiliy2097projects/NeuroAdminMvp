using Microsoft.AspNetCore.Mvc;
using TelegramBotService.Models;
using TelegramBotService.Services;

namespace TelegramBotService.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class SlotsController : ControllerBase
	{
		private readonly TelegramService _telegramService;
		private readonly ILogger<SlotsController> _logger;
		private static List<Slot> _slots = new(); // Временное хранилище, потом заменим на базу данных

		public SlotsController(
			TelegramService telegramService,
			ILogger<SlotsController> logger)
		{
			_telegramService = telegramService;
			_logger = logger;
		}

		[HttpGet]
		public IActionResult GetSlots()
		{
			return Ok(_slots);
		}

		[HttpPost]
		public IActionResult CreateSlot([FromBody] Slot slot)
		{
			try
			{
				slot.Id = _slots.Count + 1;
				slot.CreatedAt = DateTime.UtcNow;
				_slots.Add(slot);
				return Ok(slot);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating slot");
				return StatusCode(500, new { error = "Internal server error" });
			}
		}

		[HttpPut("{id}")]
		public IActionResult UpdateSlot(int id, [FromBody] Slot updatedSlot)
		{
			try
			{
				var slot = _slots.FirstOrDefault(s => s.Id == id);
				if (slot == null)
				{
					return NotFound();
				}

				slot.Name = updatedSlot.Name;
				slot.SourceChannelId = updatedSlot.SourceChannelId;
				slot.SourceChannelTitle = updatedSlot.SourceChannelTitle;
				slot.TargetChannelId = updatedSlot.TargetChannelId;
				slot.TargetChannelTitle = updatedSlot.TargetChannelTitle;
				slot.IsActive = updatedSlot.IsActive;

				return Ok(slot);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating slot");
				return StatusCode(500, new { error = "Internal server error" });
			}
		}

		[HttpDelete("{id}")]
		public IActionResult DeleteSlot(int id)
		{
			try
			{
				var slot = _slots.FirstOrDefault(s => s.Id == id);
				if (slot == null)
				{
					return NotFound();
				}

				_slots.Remove(slot);
				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting slot");
				return StatusCode(500, new { error = "Internal server error" });
			}
		}
	}
}