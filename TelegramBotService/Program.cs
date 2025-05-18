using TelegramBotService.Models;
using TelegramBotService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Telegram settings
builder.Services.Configure<TelegramSettings>(
	builder.Configuration.GetSection("Telegram"));

// Add HttpClient
builder.Services.AddHttpClient();

// Add TelegramService
builder.Services.AddScoped<TelegramService>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

//builder.WebHost.ConfigureKestrel(options =>
//{
//	//options.ListenAnyIP(3001); // HTTP
//	options.ListenAnyIP(3000, listenOptions =>
//	{
//		listenOptions.UseHttps();
//	});
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

// Добавляем обработку корневого маршрута
app.MapGet("/", async context =>
{
	context.Response.ContentType = "text/html";
	await context.Response.SendFileAsync("wwwroot/index.html");
});

// Set up webhook
var webhookUrl = builder.Configuration["WebhookUrl"];
if (!string.IsNullOrEmpty(webhookUrl))
{
	try
	{
		using var scope = app.Services.CreateScope();
		var telegramService = scope.ServiceProvider.GetRequiredService<TelegramService>();
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

		logger.LogInformation("Setting up webhook to: {WebhookUrl}", webhookUrl);
		await telegramService.SetWebhookAsync(webhookUrl);
		logger.LogInformation("Webhook successfully set");
	}
	catch (Exception ex)
	{
		var logger = app.Services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "Failed to set webhook");
	}
}

app.Run();