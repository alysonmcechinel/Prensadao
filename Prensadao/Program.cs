using Hangfire;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Prensadao.Application;
using Prensadao.API.Extensions;
using Prensadao.Infra;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

// Aqui esta as configurações => do modulo de aplicação (application) e do modulo Infrastructure (Infra)
builder.Services
    .AddAplications()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddApiErrorHandling();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseHangfireDashboard();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/ready");

await app.RunConfiguredModeAsync(args);
