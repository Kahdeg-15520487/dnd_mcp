using Microsoft.EntityFrameworkCore;
using NetHackChatGame.Data;
using NetHackChatGame.LlmProxy.Services;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<NetHackDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add HttpClient for tool executor (MCP server calls)
builder.Services.AddHttpClient<IToolExecutor, ToolExecutor>();

// Add services
builder.Services.AddSingleton<ILlmConfigurationService, LlmConfigurationService>();
builder.Services.AddScoped<ILlmService, LlmService>();
builder.Services.AddScoped<IToolExecutor, ToolExecutor>();

// Add controllers
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add CORS - LLM Proxy is only called from SignalR API (server-to-server), but allow browser calls for testing
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:8500", "http://localhost:8501", "http://localhost:3000", "http://localhost:8080")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<NetHackDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
