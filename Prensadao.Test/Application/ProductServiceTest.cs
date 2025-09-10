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
    public async Task AddProduct_ShouldReturnId_WhenValid()
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
    public async Task AddProduct_ShouldThrow_WhenNameAlreadyExists()
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
    public async Task AddProduct_ShouldThrow_WhenValueIsNotPositive()
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
    public async Task GetById_ShouldReturnDto_WhenFound()
    {
        // Arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);

        var product = new Product("X Salada", 12.30m, "O melhor x da região");
        A.CallTo(() => repository.GetById(1)).Returns(Task.FromResult<Product?>(product));

        // act
        var result = await productService.GetById(1);

        // Assert
        Assert.NotNull(result );
        A.CallTo(() => repository.GetById(1)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetById_ShouldThrow_WhenIdIsInvalid()
    {
        // Arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);

        // Act
        var act = () => productService.GetById(0);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.GetById(A<int>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task GetById_ShouldThrow_WhenProductNotFound()
    {
        // Arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);
        
        A.CallTo(() => repository.GetById(1)).Returns(Task.FromResult<Product?>(null));

        // Act
        var act = () => productService.GetById(1);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.GetById(1)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Update_ShouldApplyChangesAndPersist_WhenValid()
    {
        // Arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);

        var product = new Product("X Salada", 12.30m, "O melhor x da região");
        var dto = new ProductRequestDto
        {
            ProductId = 1,
            Name = "X-Salada",
            Description = "O melhor x salada da região",
            Enabled = true,
            Value = 10.50m
        };

        A.CallTo(() => repository.GetById(1)).Returns(Task.FromResult<Product?>(product));
        A.CallTo(() => repository.Update(A<Product>._)).Returns(Task.CompletedTask);

        // Act
        await productService.Update(dto);

        // Assert
        A.CallTo(() => repository.GetById(1)).MustHaveHappenedOnceExactly();        
        A.CallTo(() => repository.Update(
            A<Product>.That.Matches(p =>
                p.Name == dto.Name &&
                p.Description == dto.Description &&
                p.Value == dto.Value &&
                p.Enabled == dto.Enabled)))
         .MustHaveHappenedOnceExactly(); // Verifica que chamou Update com um Product que tem os valores esperados
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Update_ShouldThrow_WhenIdMissingOrInvalid(int? id)
    {
        // Arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);

        var dto = new ProductRequestDto
        {
            ProductId = id,
            Name = "X-Salada",
            Description = "O melhor x salada da região",
            Enabled = true,
            Value = 10.50m
        };

        // Act
        var act = () => productService.Update(dto);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.GetById(A<int>._)).MustNotHaveHappened();
        A.CallTo(() => repository.Update(A<Product>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Update_ShouldThrow_WhenProductNotFound()
    {
        // Arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);

        var dto = new ProductRequestDto
        {
            ProductId = 1,
            Name = "X-Salada",
            Description = "O melhor x salada da região",
            Enabled = true,
            Value = 10.50m
        };

        A.CallTo(() => repository.GetById(dto.ProductId!.Value)).Returns(Task.FromResult<Product?>(null)!);

        // Act
        var act = () => productService.Update(dto);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.GetById(A<int>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.Update(A<Product>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Enabled_ShouldSetFlagAndPersist_WhenFound()
    {
        // Arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);
        var product = new Product("X Salada", 12.30m, "O melhor x da região");
        var dto = new ProductEnabledDto
        {
            ProductId = 1,
            Enabled = true
        };

        A.CallTo(() => repository.GetById(dto.ProductId)).Returns(Task.FromResult<Product?>(product)!);
        A.CallTo(() => repository.Update(product)).Returns(Task.CompletedTask);

        // Act
        await productService.Enabled(dto);

        // Assert
        A.CallTo(() => repository.GetById(dto.ProductId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.Update(
            A<Product>.That.Matches(p =>
            p.Enabled == dto.Enabled)));
    }

    [Fact]
    public async Task Enabled_ShouldThrow_WhenProductNotFound()
    {
        // Arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);
        var dto = new ProductEnabledDto
        {
            ProductId = 1,
            Enabled = true
        };

        A.CallTo(() => repository.GetById(dto.ProductId)).Returns(Task.FromResult<Product?>(null)!);

        // Act
        var act = () => productService.Enabled(dto);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.GetById(A<int>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.Update(A<Product>._)).MustNotHaveHappened();
    }
}
