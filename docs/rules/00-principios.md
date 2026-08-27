# Rules: Princípios

## Prioridade
A prioridade deste projeto é clareza, estudo e manutenção segura.

Use princípios como guia, não como desculpa para criar abstrações desnecessárias.

## SRP
SRP, o princípio da responsabilidade única, é o princípio mais importante neste projeto.

Cada classe, método ou módulo deve ter um motivo principal para mudar.

Exemplos:

- Controller muda por causa de contrato HTTP.
- Service muda por causa de regra de negócio.
- Repository muda por causa de persistência.
- Worker muda por causa de processamento assíncrono.
- DTO muda por causa de entrada/saída da API.

## DRY
Evite duplicação quando ela representar a mesma regra de negócio repetida.

Não extraia abstração cedo demais só porque duas linhas parecem parecidas.

## SOLID
Aplique SOLID de forma pragmática:

- Prefira interfaces para serviços externos, repositórios e integrações.
- Injete dependências por construtor.
- Evite classes que saibam demais.
- Evite métodos que fazem validação, persistência, mensageria e mapeamento ao mesmo tempo.

## Exemplo Bom
Cada parte tem uma responsabilidade clara.

```csharp
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<int> AddProductAsync(ProductRequestDto dto)
    {
        ValidateProduct(dto);
        return await _productRepository.AddAsync(CreateProduct(dto));
    }
}
```

## Evite
Um método fazendo tudo ao mesmo tempo.

```csharp
public async Task<IActionResult> PostAsync(ProductRequestDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Name))
        return BadRequest("Nome inválido.");

    var product = new Product(dto.Name, dto.Value, dto.Description);
    await _dbContext.Products.AddAsync(product);
    await _dbContext.SaveChangesAsync();
    await _bus.Publish(product, "product.created");

    return Ok(product.Id);
}
```

## Comentários Didáticos
Este é um projeto de estudo. Comentários são permitidos quando explicam:

- Por que uma decisão foi tomada.
- Como um conceito funciona.
- Qual problema uma regra evita.
- Detalhes de arquitetura, mensageria, transação, EF Core ou IA.

Evite comentários que apenas repetem a linha.

## Por quê?
SRP mantém o projeto legível e testável. DRY evita repetir regras importantes. SOLID ajuda a manter dependências claras, mas deve ser aplicado com pragmatismo para não esconder o aprendizado atrás de abstrações prematuras.
