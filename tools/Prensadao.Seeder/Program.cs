using Microsoft.EntityFrameworkCore;
using Prensadao.Infra.Persistence;
using Prensadao.Seeder;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

if (!string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "O Prensadao.Seeder só pode ser executado no ambiente Development.");
}

var seedOptions = SeederArguments.Parse(args);
var connectionString = SeederConfiguration.GetConnectionString(
    Environment.GetEnvironmentVariable);
var dbContextOptions = new DbContextOptionsBuilder<PrensadaoDbContext>()
    .UseNpgsql(connectionString)
    .Options;

await using var dbContext = new PrensadaoDbContext(dbContextOptions);
await dbContext.Database.MigrateAsync();

var seeder = new LocalDatabaseSeeder(dbContext);
var result = await seeder.SeedAsync(seedOptions, DateTime.UtcNow);

Console.WriteLine(
    "Seed local concluído: " +
    $"{result.CustomersCreated:N0} clientes, " +
    $"{result.ProductsCreated:N0} produtos, " +
    $"{result.OrdersCreated:N0} pedidos e " +
    $"{result.OrderItemsCreated:N0} itens.");
