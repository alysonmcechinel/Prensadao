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
}
