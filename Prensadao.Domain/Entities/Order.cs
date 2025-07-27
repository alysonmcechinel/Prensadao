using Prensadao.Domain.Enum;
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
    public ICollection<OrderItem> OrderItems { get; set; }

}
