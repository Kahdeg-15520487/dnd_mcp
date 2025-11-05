using NetHackChatGame.SignalRApi.Hubs;
using NetHackChatGame.SignalRApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add SignalR
builder.Services.AddSignalR();

// Add HttpClient for LLM Proxy
builder.Services.AddHttpClient<IGameOrchestrator, GameOrchestrator>();

// Add services
builder.Services.AddScoped<IGameOrchestrator, GameOrchestrator>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:8500", "http://localhost:8080", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for SignalR
    });
});

// Add health checks
builder.Services.AddHealthChecks();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();

// Map SignalR hub
app.MapHub<ChatHub>("/chatHub");

// Map health check
app.MapHealthChecks("/health");

app.Run();
