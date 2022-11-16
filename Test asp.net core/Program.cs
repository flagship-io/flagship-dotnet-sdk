using Flagship.Config;
using Flagship.Main;
using Test_asp.net_core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

Fs.Start("", "", new DecisionApiConfig()
{
    HitCacheImplementation = new RedisHitCache("127.0.0.1:6379"),
    TrackingMangerConfig = new TrackingManagerConfig(Flagship.Enums.CacheStrategy.PERIODIC_CACHING, 5, TimeSpan.FromSeconds(30))
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
