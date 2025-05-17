using TelegramPoster.Models;
using TelegramPoster.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TelegramPoster API",
        Version = "v1",
        Description = "API for posting messages to Telegram channel"
    });
});

// Configure Telegram settings
builder.Services.Configure<TelegramSettings>(
    builder.Configuration.GetSection("Telegram"));

// Register services
builder.Services.AddHttpClient();
builder.Services.AddScoped<TelegramService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TelegramPoster API V1");
    });
}

app.UseAuthorization();
app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
