using AutoFixture.Xunit2;
using FakeItEasy;
using Prensadao.Application.Services;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Test.Application;

public class OrdemItemServiceTest
{
    [Theory, AutoFakeItEasyData]
    public async Task AddOrderItem_DeveAdicionarItemComSucesso(
        [Frozen] IOrderItemRepository orderItemRepository,
        OrderItemService orderItemService,
        OrderItem ordemItem)
    {
        // Act
        await orderItemService.AddOrderItem(ordemItem);
        // Assert
        A.CallTo(() => orderItemRepository.AddOrderItem(ordemItem)).MustHaveHappenedOnceExactly();
    }

    [Theory, AutoFakeItEasyData]
    public async Task GetOrderItems_DeveRetornarListaDeItens(
        [Frozen] IOrderItemRepository orderItemRepository,
        OrderItemService orderItemService,
        List<OrderItem> ordemItems)
    {
        // Arrange
        A.CallTo(() => orderItemRepository.GetOrderItems()).Returns(ordemItems);
        
        // Act
        var result = await orderItemService.GetOrderItems();
        
        // Assert
        Assert.Equal(ordemItems, result);
        A.CallTo(() => orderItemRepository.GetOrderItems()).MustHaveHappenedOnceExactly();
    }
}
