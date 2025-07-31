using System.ComponentModel.DataAnnotations;

namespace Prensadao.Domain.Entities;

public class OrderItem
{
    public OrderItem() { }

    public OrderItem(int quantity, decimal value, int orderId, int productId)
    {
        Quantity = quantity;
        Value = value;
        OrderId = orderId;
        ProductId = productId;
    }

    [Key]
    public int OrderItemId { get; private set; }
    public int Quantity { get; private set; }
    //TODO: Alterar para UnitPrice
    public decimal Value { get; private set; }
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }

    // Relationship
    public Order Order { get; private set; }
    public Product Product { get; private set; }

    public decimal GetTotal() => Quantity * Value;
}
