using System.ComponentModel.DataAnnotations;

namespace Prensadao.Application.Models;

public class OrderDto
{
    [Key]
    public int? OrderId { get; set; }
    public bool Delivery { get; set; }
    public decimal Value { get; set; }
    public string? Observation { get; set; }
    public int CustomerId { get; set; }

    public List<OrderItemDto> Items { get; set; }
}

public class OrderItemDto
{
    public int Quantity { get; set; }
    public decimal Value { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
}
