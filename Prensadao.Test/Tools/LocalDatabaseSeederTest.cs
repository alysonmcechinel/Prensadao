using Microsoft.EntityFrameworkCore;
using Prensadao.Domain.Enums;
using Prensadao.Infra.Persistence;
using Prensadao.Seeder;

namespace Prensadao.Test.Tools;

public class LocalDatabaseSeederTest
{
    [Fact]
    public async Task SeedAsync_ShouldCreateConsistentHistoricalData_WhenOptionsAreValid()
    {
        // arrange
        await using var context = CreateContext();
        var options = new LocalDatabaseSeedOptions
        {
            CustomerCount = 12,
            ProductCount = 20,
            OrderCount = 150,
            HistoryDays = 1,
            RandomSeed = 1234,
            BatchSize = 100
        };
        var utcNow = new DateTime(2026, 11, 8, 15, 0, 0, DateTimeKind.Utc);
        var seeder = new LocalDatabaseSeeder(context);

        // act
        var result = await seeder.SeedAsync(options, utcNow);

        // assert
        Assert.Equal(options.CustomerCount, await context.Customers.CountAsync());
        Assert.Equal(options.ProductCount, await context.Products.CountAsync());
        Assert.Equal(options.OrderCount, await context.Orders.CountAsync());
        Assert.Equal(result.OrderItemsCreated, await context.OrderItems.CountAsync());
        Assert.InRange(result.OrderItemsCreated, options.OrderCount, options.OrderCount * 4);

        var orders = await context.Orders
            .Include(order => order.OrderItems)
            .ToListAsync();

        Assert.All(orders, order =>
        {
            Assert.StartsWith(LocalDatabaseSeeder.DataMarker, order.Notes);
            Assert.InRange(order.CreatedAt, utcNow.AddDays(-options.HistoryDays), utcNow);
            Assert.NotEmpty(order.OrderItems);
            Assert.Equal(
                order.TotalAmount,
                order.OrderItems.Sum(item => item.Quantity * item.UnitPrice));
        });
        Assert.Contains(orders, order => order.Status == OrderStatusEnum.Finalizado);
        Assert.Contains(orders, order => order.Status == OrderStatusEnum.Cancelado);
        Assert.Contains(orders, order => order.Status == OrderStatusEnum.Criado);
    }

    [Fact]
    public async Task SeedAsync_ShouldRejectSecondExecution_WhenSeedDataAlreadyExists()
    {
        // arrange
        await using var context = CreateContext();
        var options = new LocalDatabaseSeedOptions
        {
            CustomerCount = 2,
            ProductCount = 2,
            OrderCount = 100,
            HistoryDays = 7,
            RandomSeed = 42,
            BatchSize = 100
        };
        var seeder = new LocalDatabaseSeeder(context);
        var utcNow = new DateTime(2026, 11, 8, 15, 0, 0, DateTimeKind.Utc);
        await seeder.SeedAsync(options, utcNow);

        // act
        var act = () => seeder.SeedAsync(options, utcNow);

        // assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Contains(LocalDatabaseSeeder.DataMarker, exception.Message);
        Assert.Equal(options.OrderCount, await context.Orders.CountAsync());
    }

    [Fact]
    public void Validate_ShouldRejectBatchSmallerThanSupportedMinimum()
    {
        // arrange
        var options = new LocalDatabaseSeedOptions { BatchSize = 99 };

        // act
        var act = options.Validate;

        // assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal(nameof(LocalDatabaseSeedOptions.BatchSize), exception.ParamName);
    }

    [Fact]
    public void Parse_ShouldUseDefaults_WhenArgumentsAreEmpty()
    {
        // arrange
        var arguments = Array.Empty<string>();

        // act
        var result = SeederArguments.Parse(arguments);

        // assert
        Assert.Equal(LocalDatabaseSeedOptions.DefaultCustomerCount, result.CustomerCount);
        Assert.Equal(LocalDatabaseSeedOptions.DefaultProductCount, result.ProductCount);
        Assert.Equal(LocalDatabaseSeedOptions.DefaultOrderCount, result.OrderCount);
        Assert.Equal(LocalDatabaseSeedOptions.DefaultHistoryDays, result.HistoryDays);
    }

    [Fact]
    public void Parse_ShouldOverrideSelectedValues_WhenArgumentsAreValid()
    {
        // arrange
        var arguments = new[]
        {
            "--seed-customers=5000",
            "--seed-orders=200000",
            "--seed-random=123"
        };

        // act
        var result = SeederArguments.Parse(arguments);

        // assert
        Assert.Equal(5_000, result.CustomerCount);
        Assert.Equal(200_000, result.OrderCount);
        Assert.Equal(123, result.RandomSeed);
        Assert.Equal(LocalDatabaseSeedOptions.DefaultProductCount, result.ProductCount);
    }

    [Fact]
    public void Parse_ShouldRejectUnknownOption()
    {
        // arrange
        var arguments = new[] { "--seed-unknown=10" };

        // act
        var act = () => SeederArguments.Parse(arguments);

        // assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Contains("--seed-unknown", exception.Message);
    }

    [Fact]
    public void GetConnectionString_ShouldReturnConfiguredEnvironmentValue()
    {
        // arrange
        const string expectedConnectionString = "Host=local-test;Database=test";

        // act
        var result = SeederConfiguration.GetConnectionString(variableName =>
            variableName == SeederConfiguration.ConnectionStringEnvironmentVariable
                ? expectedConnectionString
                : null);

        // assert
        Assert.Equal(expectedConnectionString, result);
    }

    [Fact]
    public void GetConnectionString_ShouldThrow_WhenEnvironmentValueIsMissing()
    {
        // arrange
        static string? MissingEnvironmentVariable(string _) => null;

        // act
        var act = () => SeederConfiguration.GetConnectionString(MissingEnvironmentVariable);

        // assert
        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Contains(SeederConfiguration.ConnectionStringEnvironmentVariable, exception.Message);
    }

    private static PrensadaoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PrensadaoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PrensadaoDbContext(options);
    }
}
