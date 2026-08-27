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
        OrderItem orderItem)
    {
        // arrange

        // act
        await orderItemService.AddOrderItemAsync(orderItem);

        // assert
        A.CallTo(() => orderItemRepository.AddAsync(orderItem)).MustHaveHappenedOnceExactly();
    }

    [Theory, AutoFakeItEasyData]
    public async Task GetOrderItems_DeveRetornar_ListaDeItens(
        [Frozen] IOrderItemRepository orderItemRepository,
        OrderItemService orderItemService,
        List<OrderItem> orderItems)
    {
        // arrange
        A.CallTo(() => orderItemRepository.GetAllAsync()).Returns(orderItems);
        
        // act
        var result = await orderItemService.GetOrderItemsAsync();
        
        // assert
        Assert.Equal(orderItems, result);
        A.CallTo(() => orderItemRepository.GetAllAsync()).MustHaveHappenedOnceExactly();
    }
}
