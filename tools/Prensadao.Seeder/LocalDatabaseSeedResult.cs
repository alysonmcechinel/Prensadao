namespace Prensadao.Seeder;

public sealed record LocalDatabaseSeedResult(
    int CustomersCreated,
    int ProductsCreated,
    int OrdersCreated,
    int OrderItemsCreated);
