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

    public void SetStatus(OrderStatusEnum status)
    {
        switch (status)
        {
            case OrderStatusEnum.Criado:
                Status = OrderStatusEnum.Criado;
                break;
            case OrderStatusEnum.EmPreparacao:
                Status = OrderStatusEnum.EmPreparacao;
                break;
            case OrderStatusEnum.Pronto:
                Status = OrderStatusEnum.Pronto;
                break;
            case OrderStatusEnum.SaiuParaEntrega:
                Status = IsDelivery ? OrderStatusEnum.SaiuParaEntrega : OrderStatusEnum.Finalizado;
                break;
            case OrderStatusEnum.Finalizado:
                Status = OrderStatusEnum.Finalizado;
                break;
            case OrderStatusEnum.Cancelado:
                Status = OrderStatusEnum.Cancelado;
                break;
            default:
                Status = OrderStatusEnum.Error;
                throw new ArgumentException("Transição de status inválida.");
        }
    }

    public void AdvanceStatus()
    {
        switch (Status)
        {
            case OrderStatusEnum.Criado:
                Status = OrderStatusEnum.EmPreparacao;
                break;
            case OrderStatusEnum.EmPreparacao:
                Status = OrderStatusEnum.Pronto;
                break;
            case OrderStatusEnum.Pronto:
                Status = IsDelivery ? OrderStatusEnum.SaiuParaEntrega : OrderStatusEnum.Finalizado;
                break;
            case OrderStatusEnum.SaiuParaEntrega:
                Status = OrderStatusEnum.Finalizado;
                break;
            default:
                throw new ArgumentException("Status não pode ser atualizado.");
        }
    }
}
