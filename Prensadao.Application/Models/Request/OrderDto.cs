using System.ComponentModel.DataAnnotations;

namespace Prensadao.Application.Models.Request;

public class OrderDto
{
    [Key]
    public int? OrderId { get; set; }

    [Required]
    public bool Delivery { get; set; }

    [Required]
    public decimal Value { get; set; }

    public string? Observation { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public List<OrderItemDto> Items { get; set; }
}


