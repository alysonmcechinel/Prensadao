using FakeItEasy;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.Services;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Test.Application;

public class ProductServiceTest
{
    // A: é uma classe estática utilitária que serve como ponto de entrada da API.
    // _: é utilizado como coringa de argumento: que significa qualquer valor desse tipo seria o equivalente a (Moq: It.IsAny<T>()) ou (NSubstitute: Arg.Any<T>())

    [Fact]
    public async Task AddProduct_when_valid()
    {
        // Arrange
        var repository = A.Fake<IProductRepository>();  // cria um fake do repositório (mock)
        var productService = new ProductService(repository); // SUT (classe testada) recebendo a depedencia fake
        var dto = new ProductRequestDto
        {
            Name = "X Salada",
            Description = "O melhor x da região",
            Value = 12.30m
        };
        
        A.CallTo(() => repository.NameAlreadyExists("X Salada")).Returns(Task.FromResult(false)); // quando checar se o nome existe retorna "false" (não existe)
        A.CallTo(() => repository.AddProduct(
            A<Product>.That.Matches(p => p.Name == dto.Name && p.Description == dto.Description && p.Value == dto.Value)
            )).Returns(1); // adiciona um Product cujo os nomes batem com o DTO e retona com o ID 1.

        // Act
        var id = await productService.AddProduct(dto);

        // Assert
        Assert.Equal(1, id);
        A.CallTo(() => repository.NameAlreadyExists(dto.Name)).MustHaveHappenedOnceExactly(); // verifica que NameAlreadyExists foi chamado uma única vez com o mesmo nome do DTO
        A.CallTo(() => repository.AddProduct(A<Product>._)).MustHaveHappenedOnceExactly(); // verifica que AddProduct foi chamado uma única vez (com qualquer Product)
    }

    [Fact]
    public async Task AddProduct_ShouldThrow_WhenNameIsEmpty()
    {
        // Arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);
        var dto = new ProductRequestDto
        {
            Name = "",
            Description = "O melhor x da região",
            Value = 12.30m
        };

        // Act
        var act = () => productService.AddProduct(dto);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.NameAlreadyExists(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => repository.AddProduct(A<Product>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task AddProduct_SholdThrow_WhenNameAlreadyExists()
    {
        // Arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);
        var dto = new ProductRequestDto
        {
            Name = "X Salada",
            Description = "O melhor x da região",
            Value = 12.30m
        };

        A.CallTo(() => repository.NameAlreadyExists("X Salada")).Returns(Task.FromResult(true));

        // Act
        var act = () => productService.AddProduct(dto);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.NameAlreadyExists(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.AddProduct(A<Product>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task AddProduct_SholdThrow_WhenValueEqualToZero()
    {
        // Arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);
        var dto = new ProductRequestDto
        {
            Name = "X Salada",
            Description = "O melhor x da região",
            Value = 0
        };

        A.CallTo(() => repository.NameAlreadyExists("X Salada")).Returns(Task.FromResult(false));

        // Act
        var act = () => productService.AddProduct(dto);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.NameAlreadyExists(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.AddProduct(A<Product>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task GetById_When_Success()
    {
        // Arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);

        var product = new Product( "X Salada", 12.30m, "O melhor x da região");
        A.CallTo(() => repository.GetById(A<int>.That.Matches(id => id == 1))).Returns(Task.FromResult<Product?>(product));

        // act
        var result = await productService.GetById(1);

        // Assert
        Assert.NotNull(result );
        A.CallTo(() => repository.GetById(1)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetById_When_InvalidID()
    {
        // Arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);

        // Act
        var act = () => productService.GetById(0);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.GetById(0)).MustNotHaveHappened();
    }

    [Fact]
    public async Task GetById_When_ProductIsNull()
    {
        // Arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);
        
        A.CallTo(() => repository.GetById(A<int>.That.Matches(id => id == 1))).Returns(Task.FromResult<Product?>(null));

        // Act
        var act = () => productService.GetById(1);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.GetById(1)).MustHaveHappenedOnceExactly();
    }
}
