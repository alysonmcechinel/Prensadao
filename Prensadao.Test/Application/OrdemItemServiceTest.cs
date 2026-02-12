using AutoFixture.Xunit2;
using FakeItEasy;
using Prensadao.Application.Services;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Test.Application;

public class OrdemItemServiceTest
{
    // Nesses testes foi deixado o (OrderItem ordemItem) e (List<OrderItem> ordemItems) serem criados automaticamente pelo AutoFakeItEasyData
    // porque não tem regras de negócio a serem validadas para a entidade OrderItem, então não há necessidade de customização dos dados.

    [Theory, AutoFakeItEasyData]
    public async Task AddOrderItem_DeveAdicionar_ItemComSucesso(
        [Frozen] IOrderItemRepository orderItemRepository,
        OrderItemService orderItemService,
        OrderItem ordemItem)
    {
        // Act
        await orderItemService.AddOrderItemAsync(ordemItem);

        // Assert
        A.CallTo(() => orderItemRepository.AddOrderItemAsync(ordemItem)).MustHaveHappenedOnceExactly();
    }

    [Theory, AutoFakeItEasyData]
    public async Task GetOrderItems_DeveRetornar_ListaDeItens(
        [Frozen] IOrderItemRepository orderItemRepository,
        OrderItemService orderItemService,
        List<OrderItem> ordemItems)
    {
        // Arrange
        A.CallTo(() =>  orderItemRepository.GetOrderItemsAsync()).Returns(ordemItems);
        
        // Act
        var result = await orderItemService.GetOrderItems();
        
        // Assert
        Assert.Equal(ordemItems, result);
        A.CallTo(() => orderItemRepository.GetOrderItemsAsync()).MustHaveHappenedOnceExactly();
    }
}
