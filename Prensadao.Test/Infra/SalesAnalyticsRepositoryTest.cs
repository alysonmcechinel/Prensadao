using Microsoft.EntityFrameworkCore;
using Prensadao.Domain.Entities;
using Prensadao.Infra.Persistence;
using Prensadao.Infra.Persistence.Repositories;

namespace Prensadao.Test.Infra;

public class SalesAnalyticsRepositoryTest
{
    [Fact]
    public async Task GetTopProductsSoldAsync_ShouldExcludeInactiveProducts()
    {
        // arrange
        var options = new DbContextOptionsBuilder<PrensadaoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new PrensadaoDbContext(options);
        var customer = new Customer(
            "Cliente",
            "11999999999",
            "Rua",
            "Bairro",
            "1",
            "Cidade",
            "Referência",
            12345678);
        var activeProduct = new Product("X Salada", 25m, "Lanche ativo");
        var inactiveProduct = new Product("X Bacon", 30m, "Lanche inativo");
        inactiveProduct.SetEnabled(false);

        context.AddRange(customer, activeProduct, inactiveProduct);
        await context.SaveChangesAsync();

        var order = new Order(
            false,
            3025m,
            string.Empty,
            customer.CustomerId,
            new DateTime(2026, 10, 17, 15, 0, 0, DateTimeKind.Utc));

        context.Add(order);
        await context.SaveChangesAsync();

        context.AddRange(
            new OrderItem(1, 25m, order.OrderId, activeProduct.ProductId),
            new OrderItem(100, 30m, order.OrderId, inactiveProduct.ProductId));
        await context.SaveChangesAsync();

        var repository = new SalesAnalyticsRepository(context);
        var fromInclusiveUtc = new DateTime(2026, 10, 17, 3, 0, 0, DateTimeKind.Utc);
        var toExclusiveUtc = new DateTime(2026, 10, 18, 3, 0, 0, DateTimeKind.Utc);

        // act
        var result = await repository.GetTopProductsSoldAsync(
            fromInclusiveUtc,
            toExclusiveUtc,
            1,
            CancellationToken.None);

        // assert
        var product = Assert.Single(result);
        Assert.Equal(activeProduct.ProductId, product.ProductId);
    }

    [Fact]
    public async Task GetBestPromotionDayOfWeekAsync_ShouldUseBusinessDateAndExcludeEndBoundary()
    {
        // arrange
        var options = new DbContextOptionsBuilder<PrensadaoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new PrensadaoDbContext(options);
        var customer = new Customer(
            "Cliente",
            "11999999999",
            "Rua",
            "Bairro",
            "1",
            "Cidade",
            "Referência",
            12345678);
        var product = new Product("X Salada", 25m, "Lanche");
        product.SetEnabled(false);

        context.AddRange(customer, product);
        await context.SaveChangesAsync();

        var saturdayInSaoPaulo = new Order(
            false,
            25m,
            string.Empty,
            customer.CustomerId,
            new DateTime(2026, 10, 18, 2, 30, 0, DateTimeKind.Utc));
        var excludedSunday = new Order(
            false,
            2500m,
            string.Empty,
            customer.CustomerId,
            new DateTime(2026, 10, 18, 3, 0, 0, DateTimeKind.Utc));

        context.AddRange(saturdayInSaoPaulo, excludedSunday);
        await context.SaveChangesAsync();

        context.AddRange(
            new OrderItem(1, 25m, saturdayInSaoPaulo.OrderId, product.ProductId),
            new OrderItem(100, 25m, excludedSunday.OrderId, product.ProductId));
        await context.SaveChangesAsync();

        var repository = new SalesAnalyticsRepository(context);
        var fromInclusiveUtc = new DateTime(2026, 10, 17, 3, 0, 0, DateTimeKind.Utc);
        var toExclusiveUtc = new DateTime(2026, 10, 18, 3, 0, 0, DateTimeKind.Utc);

        // act
        var result = await repository.GetBestPromotionDayOfWeekAsync(
            fromInclusiveUtc,
            toExclusiveUtc,
            CancellationToken.None);

        // assert
        Assert.Equal(DayOfWeek.Saturday.ToString(), result);
    }
}
