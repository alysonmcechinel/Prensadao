using Prensadao.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Prensadao.Domain.Entities;

public class Order
{
    public Order() { }

    public Order(bool delivery, decimal value, string observation, int customerId)
    {
        DateOrder = DateTime.UtcNow;
        OrderStatus = OrderStatusEnum.Criado;
        Delivery = delivery;
        Value = value;
        Observation = observation;
        CustomerId = customerId;

        OrderItems = new List<OrderItem>();
    }

    [Key]
    public int OrderId { get; private set; }
    public DateTime DateOrder { get; private set; }
    public OrderStatusEnum OrderStatus { get; private set; }
    public bool Delivery { get; private set; }
    public decimal Value { get; private set; }
    public string Observation { get; private set; }
    public int CustomerId { get; private set; }

    // Relationship
    public Customer Customer { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = [];

    public void UpdateStatus(OrderStatusEnum status)
    {
        switch (status)
        {
            case OrderStatusEnum.Criado:
                OrderStatus = OrderStatusEnum.Criado;
                break;
            case OrderStatusEnum.EmPreparacao:
                OrderStatus = OrderStatusEnum.EmPreparacao;
                break;
            case OrderStatusEnum.Pronto:
                OrderStatus = OrderStatusEnum.Pronto;
                break;
            case OrderStatusEnum.SaiuParaEntrega:
                OrderStatus = Delivery ? OrderStatusEnum.SaiuParaEntrega : OrderStatusEnum.Finalizado;
                break;
            case OrderStatusEnum.Finalizado:
                OrderStatus = OrderStatusEnum.Finalizado;
                break;
            case OrderStatusEnum.Cancelado:
                OrderStatus = OrderStatusEnum.Cancelado;
                break;
            default:
                OrderStatus = OrderStatusEnum.Error;
                throw new ArgumentException("Transição de status inválida.");
        }
    }

    public void NextStatus()
    {
        switch (OrderStatus)
        {
            case OrderStatusEnum.Criado:
                OrderStatus = OrderStatusEnum.EmPreparacao;
                break;
            case OrderStatusEnum.EmPreparacao:
                OrderStatus = OrderStatusEnum.Pronto;
                break;
            case OrderStatusEnum.Pronto:
                OrderStatus = Delivery ? OrderStatusEnum.SaiuParaEntrega : OrderStatusEnum.Finalizado;
                break;
            case OrderStatusEnum.SaiuParaEntrega:
                OrderStatus = OrderStatusEnum.Finalizado;
                break;
            default:
                throw new ArgumentException("Status não pode ser atualizado.");
        }
    }
}
