namespace TelegramBotService.Models
{
	public class Slot
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string SourceChannelId { get; set; } = string.Empty;
		public string SourceChannelTitle { get; set; } = string.Empty;
		public string TargetChannelId { get; set; } = string.Empty;
		public string TargetChannelTitle { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? LastPublishedAt { get; set; }
	}
}