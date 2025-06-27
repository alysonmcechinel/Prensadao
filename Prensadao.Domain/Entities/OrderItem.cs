namespace Prensadao.Domain.Entities;

public class OrderItem
{
    public int OrderItemId { get; private set; }
    public int Quantity { get; private set; }
    public decimal Value { get; private set; }
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }

    // Relationship
    public Order Order { get; private set; }
    public Product Product { get; private set; }
}
