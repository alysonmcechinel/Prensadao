# Rules: Testes

## Regra
- Testes usam xUnit, FakeItEasy, AutoFixture e FluentAssertions.
- Novos testes devem ficar em `Prensadao.Test/<Area>/`.
- Nomeie arquivos conforme o item testado, como `ProductServiceTest.cs`.
- Para novos testes, prefira `Method_ShouldExpectedBehavior_WhenCondition`.
- Mantenha o padrão local quando editar arquivos existentes.
- Cubra sucesso, validações de entrada, exceções de regra e chamadas esperadas a repositórios/mensageria.
- Não dependa de PostgreSQL, RabbitMQ ou Azure OpenAI reais em testes unitários.
- Para mudanças em serviços, contratos, DTOs, workers, repositórios ou IA, adicione ou atualize testes relevantes.
- Ao finalizar mudanças de código, rode `dotnet build Prensadao.sln`.
- Para mudanças de regra de negócio ou infraestrutura, rode também `dotnet test Prensadao.Test/Prensadao.Test.csproj` ou testes relevantes.

## Exemplo Bom
Nome claro e foco em uma regra.

```csharp
[Fact]
public async Task AddProduct_ShouldThrow_WhenValueIsNotPositive()
{
    var repository = A.Fake<IProductRepository>();
    var service = new ProductService(repository);

    var dto = new ProductRequestDto
    {
        Name = "X Salada",
        Value = 0,
        Description = "Produto de teste"
    };

    var act = () => service.AddProductAsync(dto);

    await act.Should().ThrowAsync<ArgumentException>()
        .WithMessage("O valor do produto deve ser maior que 0");
}
```

Verificação de chamada esperada.

```csharp
A.CallTo(() => repository.UpdateAsync(product))
    .MustHaveHappenedOnceExactly();
```

## Evite
Teste unitário dependendo de banco, RabbitMQ ou Azure reais.

```csharp
[Fact]
public async Task GeneratePromotion_ShouldCallRealAzureOpenAi()
{
    var service = new AzureOpenAiPromotionService(realOptions);

    var result = await service.SuggestPromotionAsync(products, "Friday", 5, 25, CancellationToken.None);

    result.PromotionName.Should().NotBeEmpty();
}
```

## Por quê?
Testes unitários devem ser rápidos, previsíveis e educativos. Integrações reais podem existir em outro nível, mas não devem travar o ciclo básico de desenvolvimento.
