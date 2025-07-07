using Prensadao.Application.Publish;
using Prensadao.Application.Services;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<RabbitMqStartupService>();
builder.Services.AddSingleton<IRabbitMqConfigService, RabbitMqConfigService>();

builder.Services.AddScoped<IModel>(x =>
{
    var rabbitConfig = x.GetRequiredService<RabbitMqConfigService>();
    return rabbitConfig.CreateChannel();
});

builder.Services.AddScoped<IBus, Bus>();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
