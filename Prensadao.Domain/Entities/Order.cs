using Prensadao.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Prensadao.Domain.Entities;

public class Order
{
    public Order() { }

    public Order(bool isDelivery, decimal totalAmount, string notes, int customerId, DateTime createdAt)
    {
        CreatedAt = createdAt;
        Status = OrderStatusEnum.Criado;
        IsDelivery = isDelivery;
        TotalAmount = totalAmount;
        Notes = notes;
        CustomerId = customerId;

        OrderItems = new List<OrderItem>();
    }

    [Key]
    public int OrderId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public OrderStatusEnum Status { get; private set; }
    public bool IsDelivery { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Notes { get; private set; }
    public int CustomerId { get; private set; }

    // Relationship
    public Customer Customer { get; private set; }
    public ICollection<OrderItem> OrderItems { get; private set; } = [];

    public void ChangeStatus(OrderStatusEnum status)
    {
        if (!Enum.IsDefined(status))
            throw new ArgumentException("Status do pedido inválido.");

        if (status == OrderStatusEnum.Cancelado)
            throw new ArgumentException("Cancelamento não faz parte do avanço normal do pedido.");

        if (Status == status)
            return;

        var nextStatus = GetNextStatus();
        if (status != nextStatus)
            throw new ArgumentException($"Transição de status inválida: {Status} para {status}.");

        Status = status;
    }

    public void AdvanceStatus() => ChangeStatus(GetNextStatus());

    public void Cancel()
    {
        if (Status is not OrderStatusEnum.Criado and not OrderStatusEnum.EmPreparacao)
            throw new ArgumentException($"Pedido não pode ser cancelado no status atual: {Status}.");

        Status = OrderStatusEnum.Cancelado;
    }

    private OrderStatusEnum GetNextStatus() => Status switch
    {
        OrderStatusEnum.Criado => OrderStatusEnum.EmPreparacao,
        OrderStatusEnum.EmPreparacao => OrderStatusEnum.Pronto,
        OrderStatusEnum.Pronto when IsDelivery => OrderStatusEnum.SaiuParaEntrega,
        OrderStatusEnum.Pronto => OrderStatusEnum.Finalizado,
        OrderStatusEnum.SaiuParaEntrega => OrderStatusEnum.Finalizado,
        _ => throw new ArgumentException($"Pedido no status {Status} não pode avançar.")
    };
}
