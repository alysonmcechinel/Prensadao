using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Enums;
using Prensadao.Infra.Persistence;

namespace Prensadao.Seeder;

public sealed class LocalDatabaseSeeder
{
    public const string DataMarker = "[seed-local-v1]";

    private const int MaximumItemsPerOrder = 4;
    private readonly PrensadaoDbContext _dbContext;

    public LocalDatabaseSeeder(PrensadaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LocalDatabaseSeedResult> SeedAsync(
        LocalDatabaseSeedOptions options,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        if (utcNow.Kind != DateTimeKind.Utc)
            throw new ArgumentException("A data de referência do seed deve estar em UTC.", nameof(utcNow));

        await EnsureSeedWasNotExecutedAsync(cancellationToken);

        await using var transaction = await BeginTransactionWhenSupportedAsync(cancellationToken);
        var random = new Random(options.RandomSeed);

        var customerIds = await CreateCustomersAsync(options.CustomerCount, cancellationToken);
        var products = await CreateProductsAsync(options.ProductCount, cancellationToken);
        var itemCount = await CreateOrdersAsync(
            options,
            utcNow,
            random,
            customerIds,
            products,
            cancellationToken);

        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);

        return new LocalDatabaseSeedResult(
            options.CustomerCount,
            options.ProductCount,
            options.OrderCount,
            itemCount);
    }

    private async Task EnsureSeedWasNotExecutedAsync(CancellationToken cancellationToken)
    {
        var seedAlreadyExists = await _dbContext.Orders
            .AsNoTracking()
            .AnyAsync(order => order.Notes.StartsWith(DataMarker), cancellationToken);

        if (seedAlreadyExists)
        {
            throw new InvalidOperationException(
                $"A base já contém dados identificados por {DataMarker}. " +
                "O seed local não foi repetido para evitar duplicação silenciosa.");
        }
    }

    private async Task<IReadOnlyList<int>> CreateCustomersAsync(
        int customerCount,
        CancellationToken cancellationToken)
    {
        var customers = Enumerable.Range(1, customerCount)
            .Select(index => new Customer(
                $"Cliente seed {index:D6}",
                $"7{index:D10}",
                $"Rua de teste {index % 250:D3}",
                $"Bairro {index % 40:D2}",
                (index % 9999 + 1).ToString(),
                (index % 4) switch
                {
                    0 => "Criciúma",
                    1 => "Içara",
                    2 => "Forquilhinha",
                    _ => "Nova Veneza"
                },
                $"Dados gerados por {DataMarker}",
                88_800_000 + index % 99_999))
            .ToList();

        await _dbContext.Customers.AddRangeAsync(customers, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var customerIds = customers.Select(customer => customer.CustomerId).ToArray();
        _dbContext.ChangeTracker.Clear();
        return customerIds;
    }

    private async Task<IReadOnlyList<SeedProduct>> CreateProductsAsync(
        int productCount,
        CancellationToken cancellationToken)
    {
        var products = Enumerable.Range(1, productCount)
            .Select(index => new Product(
                $"Produto seed {index:D4}",
                8m + index % 50 * 1.75m,
                $"Produto gerado por {DataMarker}"))
            .ToList();

        foreach (var product in products.Where((_, index) => (index + 1) % 10 == 0))
            product.SetEnabled(false);

        await _dbContext.Products.AddRangeAsync(products, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var seededProducts = products
            .Select(product => new SeedProduct(product.ProductId, product.Price))
            .ToArray();
        _dbContext.ChangeTracker.Clear();
        return seededProducts;
    }

    private async Task<int> CreateOrdersAsync(
        LocalDatabaseSeedOptions options,
        DateTime utcNow,
        Random random,
        IReadOnlyList<int> customerIds,
        IReadOnlyList<SeedProduct> products,
        CancellationToken cancellationToken)
    {
        var totalItemCount = 0;

        for (var firstOrderNumber = 1;
             firstOrderNumber <= options.OrderCount;
             firstOrderNumber += options.BatchSize)
        {
            var currentBatchSize = Math.Min(options.BatchSize, options.OrderCount - firstOrderNumber + 1);
            var drafts = new List<OrderDraft>(currentBatchSize);

            for (var offset = 0; offset < currentBatchSize; offset++)
            {
                var orderNumber = firstOrderNumber + offset;
                drafts.Add(CreateOrderDraft(
                    orderNumber,
                    options.HistoryDays,
                    utcNow,
                    random,
                    customerIds,
                    products));
            }

            await _dbContext.Orders.AddRangeAsync(
                drafts.Select(draft => draft.Order),
                cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var orderItems = drafts
                .SelectMany(draft => draft.Items.Select(item => new OrderItem(
                    item.Quantity,
                    item.UnitPrice,
                    draft.Order.OrderId,
                    item.ProductId)))
                .ToList();

            await _dbContext.OrderItems.AddRangeAsync(orderItems, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            totalItemCount += orderItems.Count;
            _dbContext.ChangeTracker.Clear();
        }

        return totalItemCount;
    }

    private static OrderDraft CreateOrderDraft(
        int orderNumber,
        int historyDays,
        DateTime utcNow,
        Random random,
        IReadOnlyList<int> customerIds,
        IReadOnlyList<SeedProduct> products)
    {
        var isDelivery = random.Next(100) < 65;
        var items = CreateItems(random, products);
        var totalAmount = items.Sum(item => item.Quantity * item.UnitPrice);
        var maximumAgeInSeconds = (long)historyDays * 24 * 60 * 60;
        var createdAt = utcNow.AddSeconds(-random.NextInt64(maximumAgeInSeconds));
        var customerId = customerIds[random.Next(customerIds.Count)];
        var order = new Order(
            isDelivery,
            totalAmount,
            $"{DataMarker} Pedido gerado #{orderNumber:D8}",
            customerId,
            createdAt);

        ApplyStatus(order, SelectStatus(random, isDelivery, createdAt, utcNow));
        return new OrderDraft(order, items);
    }

    private static IReadOnlyList<SeedOrderItem> CreateItems(
        Random random,
        IReadOnlyList<SeedProduct> products)
    {
        var itemCount = random.Next(1, Math.Min(MaximumItemsPerOrder, products.Count) + 1);
        var selectedProductIndexes = new HashSet<int>();
        var items = new List<SeedOrderItem>(itemCount);

        while (items.Count < itemCount)
        {
            // A curva favorece os primeiros produtos e produz um ranking de vendas útil para as análises.
            var productIndex = Math.Min(
                (int)(Math.Pow(random.NextDouble(), 2) * products.Count),
                products.Count - 1);

            if (!selectedProductIndexes.Add(productIndex))
                continue;

            var product = products[productIndex];
            var historicalPriceFactor = 0.85m + (decimal)random.NextDouble() * 0.30m;
            var unitPrice = Math.Round(
                product.CurrentPrice * historicalPriceFactor,
                2,
                MidpointRounding.AwayFromZero);

            items.Add(new SeedOrderItem(
                product.ProductId,
                random.Next(1, 6),
                unitPrice));
        }

        return items;
    }

    private static OrderStatusEnum SelectStatus(
        Random random,
        bool isDelivery,
        DateTime createdAt,
        DateTime utcNow)
    {
        var bucket = random.Next(100);

        // Pedidos antigos não permanecem artificialmente na operação da cozinha.
        if (createdAt < utcNow.AddHours(-6))
            return bucket < 90 ? OrderStatusEnum.Finalizado : OrderStatusEnum.Cancelado;

        if (bucket < 40)
            return OrderStatusEnum.Finalizado;
        if (bucket < 50)
            return OrderStatusEnum.Cancelado;
        if (bucket < 65)
            return OrderStatusEnum.Criado;
        if (bucket < 80)
            return OrderStatusEnum.EmPreparacao;
        if (bucket < 90)
            return OrderStatusEnum.Pronto;
        if (isDelivery)
            return OrderStatusEnum.SaiuParaEntrega;

        return OrderStatusEnum.Finalizado;
    }

    private static void ApplyStatus(Order order, OrderStatusEnum targetStatus)
    {
        if (targetStatus == OrderStatusEnum.Cancelado)
        {
            order.Cancel();
            return;
        }

        while (order.Status != targetStatus)
            order.AdvanceStatus();
    }

    private async Task<IDbContextTransaction?> BeginTransactionWhenSupportedAsync(
        CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational())
            return null;

        return await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    private sealed record SeedProduct(int ProductId, decimal CurrentPrice);
    private sealed record SeedOrderItem(int ProductId, int Quantity, decimal UnitPrice);
    private sealed record OrderDraft(Order Order, IReadOnlyList<SeedOrderItem> Items);
}
