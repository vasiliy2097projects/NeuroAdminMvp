namespace TelegramBotService.Models
{
	public class TelegramUpdate
	{
		public long UpdateId { get; set; }
		public Message? Message { get; set; }
	}

	public class Message
	{
		public long MessageId { get; set; }
		public User From { get; set; } = new();
		public Chat Chat { get; set; } = new();
		public string Text { get; set; } = string.Empty;
	}

	public class User
	{
		public long Id { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string? LastName { get; set; }
		public string? Username { get; set; }
	}

	public class Chat
	{
		public long Id { get; set; }
		public string Type { get; set; } = string.Empty;
		public string? Title { get; set; }
		public string? Username { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string? LastName { get; set; }
	}
}