using HuntTheWumpus.API.Infrastructure;
using HuntTheWumpus.Domain.Interfaces;
using HuntTheWumpus.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add domain services - using in-memory storage for simplicity
builder.Services.AddSingleton<IGameRepository, InMemoryGameRepository>();
builder.Services.AddScoped<GameService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Serve static files for the frontend
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

// Serve the frontend HTML file for any route that doesn't match an API endpoint
app.MapFallbackToFile("index.html");

app.Run();
