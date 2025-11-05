using Microsoft.EntityFrameworkCore;
using NetHackChatGame.Data;
using NetHackChatGame.McpServer.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Configure database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=nethack_chat_game;Username=postgres;Password=postgres";

builder.Services.AddDbContext<NetHackDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register game services
builder.Services.AddScoped<IGameStateService, GameStateService>();
// builder.Services.AddScoped<GameMcpTools>();

// TODO: Add MCP support once we understand the SDK API
// builder.Services.AddMcp(options =>
// {
//     options.ServerInfo = new ServerInfo
//     {
//         Name = "NetHack Chat Game MCP Server",
//         Version = "1.0.0"
//     };
//     
//     // Register MCP tools
//     options.AddToolsFrom<GameMcpTools>();
// });

// Add CORS - MCP Server is only called from LLM Proxy (server-to-server), but allow browser calls for testing
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:8500", "http://localhost:8502", "http://localhost:3000", "http://localhost:8080")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Always enable CORS for testing/debugging
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.MapControllers();

app.MapMcp();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .WithName("HealthCheck");

app.Run();
