using FakeItEasy;
using FluentAssertions;
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
        // arrange
        var repository = A.Fake<IProductRepository>();  // cria um fake do repositório (mock)
        var productService = new ProductService(repository); // SUT (classe testada) recebendo a depedencia fake
        var dto = new ProductRequestDto
        {
            Name = "X Salada",
            Description = "O melhor x da região",
            Value = 12.30m
        };
        
        A.CallTo(() => repository.ExistsByNameAsync("X Salada")).Returns(Task.FromResult(false)); // quando checar se o nome existe retorna "false" (não existe)
        A.CallTo(() => repository.AddAsync(
            A<Product>.That.Matches(p => p.Name == dto.Name && p.Description == dto.Description && p.Price == dto.Value)
            )).Returns(1); // adiciona um Product cujo os nomes batem com o DTO e retona com o ID 1.

        // act
        var id = await productService.AddProductAsync(dto);

        // assert
        Assert.Equal(1, id);
        A.CallTo(() => repository.ExistsByNameAsync(dto.Name)).MustHaveHappenedOnceExactly(); // verifica que ExistsByNameAsync foi chamado uma única vez com o mesmo nome do DTO
        A.CallTo(() => repository.AddAsync(A<Product>._)).MustHaveHappenedOnceExactly(); // verifica que AddAsync foi chamado uma única vez (com qualquer Product)
    }

    [Fact]
    public async Task AddProduct_ShouldThrow_WhenNameIsEmpty()
    {
        // arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);
        var dto = new ProductRequestDto
        {
            Name = "",
            Description = "O melhor x da região",
            Value = 12.30m
        };

        // act
        var act = () => productService.AddProductAsync(dto);

        // assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.ExistsByNameAsync(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => repository.AddAsync(A<Product>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task AddProduct_ShouldThrow_WhenNameAlreadyExists()
    {
        // arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);
        var dto = new ProductRequestDto
        {
            Name = "X Salada",
            Description = "O melhor x da região",
            Value = 12.30m
        };

        A.CallTo(() => repository.ExistsByNameAsync("X Salada")).Returns(Task.FromResult(true));

        // act
        var act = () => productService.AddProductAsync(dto);

        // assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.ExistsByNameAsync(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.AddAsync(A<Product>._)).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddProduct_ShouldThrow_WhenValueIsNotPositive(decimal value)
    {
        // arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);
        var dto = new ProductRequestDto
        {
            Name = "X Salada",
            Description = "O melhor x da região",
            Value = value
        };

        A.CallTo(() => repository.ExistsByNameAsync("X Salada")).Returns(Task.FromResult(false));

        // act
        var act = async () => await productService.AddProductAsync(dto);

        // assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.ExistsByNameAsync(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => repository.AddAsync(A<Product>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task GetById_ShouldReturnDto_WhenFound()
    {
        // arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);

        var product = new Product("X Salada", 12.30m, "O melhor x da região");
        A.CallTo(() => repository.GetByIdAsync(1)).Returns(Task.FromResult<Product?>(product));

        // act
        var result = await productService.GetByIdAsync(1);

        // assert
        Assert.NotNull(result);
        result.Name.Should().Be("X Salada");
        result.Value.Should().Be(12.30m);
        result.Description.Should().Be("O melhor x da região");
        A.CallTo(() => repository.GetByIdAsync(1)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetById_ShouldThrow_WhenIdIsInvalid()
    {
        // arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);

        // act
        var act = () => productService.GetByIdAsync(0);

        // assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.GetByIdAsync(A<int>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task GetById_ShouldThrow_WhenProductNotFound()
    {
        // arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);
        
        A.CallTo(() => repository.GetByIdAsync(1)).Returns(Task.FromResult<Product?>(null));

        // act
        var act = () => productService.GetByIdAsync(1);

        // assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.GetByIdAsync(1)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Update_ShouldApplyChangesAndPersist_WhenValid()
    {
        // arrange
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

        A.CallTo(() => repository.GetByIdAsync(1)).Returns(Task.FromResult<Product?>(product));
        A.CallTo(() => repository.UpdateAsync(A<Product>._)).Returns(Task.CompletedTask);

        // act
        await productService.UpdateAsync(dto);

        // assert
        product.Name.Should().Be(dto.Name);
        product.Description.Should().Be(dto.Description);
        product.Price.Should().Be(dto.Value);
        product.Enabled.Should().Be(dto.Enabled);

        A.CallTo(() => repository.GetByIdAsync(1)).MustHaveHappenedOnceExactly();        
        A.CallTo(() => repository.UpdateAsync(
            A<Product>.That.Matches(p =>
                p.Name == dto.Name &&
                p.Description == dto.Description &&
                p.Price == dto.Value &&
                p.Enabled == dto.Enabled)))
         .MustHaveHappenedOnceExactly(); // Verifica que chamou Update com um Product que tem os valores esperados
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Update_ShouldThrow_WhenIdMissingOrInvalid(int? id)
    {
        // arrange
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

        // act
        var act = () => productService.UpdateAsync(dto);

        // assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.GetByIdAsync(A<int>._)).MustNotHaveHappened();
        A.CallTo(() => repository.UpdateAsync(A<Product>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Update_ShouldThrow_WhenProductNotFound()
    {
        // arrange
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

        A.CallTo(() => repository.GetByIdAsync(dto.ProductId!.Value)).Returns(Task.FromResult<Product?>(null)!);

        // act
        var act = () => productService.UpdateAsync(dto);

        // assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.GetByIdAsync(A<int>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.UpdateAsync(A<Product>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Enabled_ShouldSetFlagAndPersist_WhenFound()
    {
        // arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);
        var product = new Product("X Salada", 12.30m, "O melhor x da região");
        var dto = new ProductEnabledDto
        {
            ProductId = 1,
            Enabled = false
        };

        A.CallTo(() => repository.GetByIdAsync(dto.ProductId)).Returns(Task.FromResult<Product?>(product)!);
        A.CallTo(() => repository.UpdateAsync(product)).Returns(Task.CompletedTask);

        // act
        await productService.EnabledAsync(dto);

        // assert
        product.Enabled.Should().BeFalse();
        A.CallTo(() => repository.GetByIdAsync(dto.ProductId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.UpdateAsync(A<Product>.That.Matches(p => p.Enabled == dto.Enabled)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Enabled_ShouldThrow_WhenProductNotFound()
    {
        // arrange
        var repository = A.Fake<IProductRepository>();
        var productService = new ProductService(repository);
        var dto = new ProductEnabledDto
        {
            ProductId = 1,
            Enabled = true
        };

        A.CallTo(() => repository.GetByIdAsync(dto.ProductId)).Returns(Task.FromResult<Product?>(null)!);

        // act
        var act = () => productService.EnabledAsync(dto);

        // assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await act());
        A.CallTo(() => repository.GetByIdAsync(A<int>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.UpdateAsync(A<Product>._)).MustNotHaveHappened();
    }
}
