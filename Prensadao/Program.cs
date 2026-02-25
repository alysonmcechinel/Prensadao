using Hangfire;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Prensadao.Application;
using Prensadao.Infra;

var builder = WebApplication.CreateBuilder(args);

// Ignora logs de queries do EF Core || limpar o console
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseMiddleware<RequestContextMiddleware>();

app.UseHttpsRedirection();

app.UseHangfireDashboard();

app.UseAuthorization();

app.MapControllers();

app.Run();
