# Rules: Persistência

## Regra
- Configurações EF Core ficam em `Prensadao.Infra/Persistence/Configurations`.
- Repositórios concretos ficam em `Prensadao.Infra/Persistence/Repositories`.
- Interfaces de repositório ficam em `Prensadao.Domain/Repositories`.
- Use migrations apenas quando houver mudança real no modelo persistido.
- Não edite migrations antigas sem pedido explícito.
- Ao adicionar entidade ou relacionamento, atualize DbContext, configuração EF, repositório, DI, DTOs e testes quando aplicável.
- Use `Include` apenas quando o fluxo precisar dos dados relacionados.
- Preserve `decimal` para dinheiro.
- Preserve NodaTime/UTC conforme o padrão atual.
- Repositórios não devem publicar mensagens nem executar regra de negócio.

## Exemplo Bom
Interface no Domain.

```csharp
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<int> AddAsync(Product product);
    Task UpdateAsync(Product product);
}
```

Implementação na Infra.

```csharp
public class ProductRepository : IProductRepository
{
    private readonly PrensadaoDbContext _dbContext;

    public ProductRepository(PrensadaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Product?> GetByIdAsync(int id)
        => _dbContext.Products.FirstOrDefaultAsync(product => product.Id == id);
}
```

Query com detalhes apenas quando o fluxo precisa.

```csharp
public Task<List<Order>> GetAllWithDetailsAsync()
    => _dbContext.Orders
        .Include(order => order.Customer)
        .Include(order => order.Items)
        .ThenInclude(item => item.Product)
        .ToListAsync();
```

## Evite
Regra de negócio dentro do repositório.

```csharp
public async Task AddAsync(Order order)
{
    if (order.Items.Count == 0)
        throw new ArgumentException("Pedido não pode ser feito sem itens.");

    await _bus.Publish(message, RabbitMqConstants.Exchanges.OrderExchange);
    await _dbContext.Orders.AddAsync(order);
}
```

## Por quê?
Repositório deve explicar persistência. Regra de negócio fica nos services ou entidades, e mensageria fica no fluxo de aplicação ou worker.
