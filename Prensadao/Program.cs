using Prensadao.Application;
using Prensadao.Application.Publish;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IBus, Bus>();

// RabbitMQ
var connectionFactory = new ConnectionFactory { HostName = "localhost" };
using var connection = connectionFactory.CreateConnection();
var channel = connection.CreateModel();

var rabbitMqConfig = new RabbitMqConfig(channel);
rabbitMqConfig.Config();

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
