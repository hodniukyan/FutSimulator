using BrasfootAPI.Data;
using Microsoft.EntityFrameworkCore;
using MiniBrasfoot.Api.Endpoints;
using MiniBrasfoot.Api.Middleware;
using MiniBrasfoot.Application.Interfaces;
using MiniBrasfoot.Application.Services;
using MiniBrasfoot.Domain.Rules;
using MiniBrasfoot.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:5173"])
        .AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connection = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Configure ConnectionStrings__DefaultConnection.");
    var configuredVersion = builder.Configuration["Database:ServerVersion"];
    // Preserve detection of the existing server. An explicit version supports offline startup.
    var version = string.IsNullOrWhiteSpace(configuredVersion)
        ? ServerVersion.AutoDetect(connection) : ServerVersion.Parse(configuredVersion);
    options.UseMySql(connection, version);
});
builder.Services.AddScoped<IGameStore, EfGameStore>();
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<MatchService>();
builder.Services.AddSingleton<MatchSimulator>();

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithSummary("Disponibilidade da API; não verifica a conexão com o banco.");
app.MapTeamEndpoints();
app.MapPlayerEndpoints();
app.MapMatchEndpoints();
app.Run();

public partial class Program;
