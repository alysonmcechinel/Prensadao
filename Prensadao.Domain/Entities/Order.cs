namespace Prensadao.Domain.Entities;

public class Order
{
    public int OrderId { get; private set; }
    public DateTime DateOrder { get; private set; }
    public int OrderStatus { get; private set; } // TODO: enum
    public bool Delivery { get; private set; }
    public decimal Value { get; private set; }
    public string Observation { get; private set; }
    public int CustomerId { get; private set; }

    // Relationship
    public Customer Customer { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }

}
