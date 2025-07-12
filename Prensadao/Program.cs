using Prensadao.Application;
using Prensadao.Application.Interfaces;
using Prensadao.Application.Publish;
using Prensadao.Application.Services;
using Prensadao.Domain.Repositories;
using Prensadao.Infra;
using Prensadao.Infra.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Aqui esta as configurações => do modulo de aplicação (application) e do modulo Infrastructure (Infra)
builder.Services
    .AddAplications()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PrensadaoDbContext>();
    InfrastructureModule.Seed(dbContext);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
