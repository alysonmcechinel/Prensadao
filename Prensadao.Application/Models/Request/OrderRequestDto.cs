using Prensadao.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Prensadao.Application.Models.Request;

public class OrderRequestDto
{
    [Key]
    public int? OrderId { get; set; }

    [Required]
    public bool Delivery { get; set; }

    public decimal Value { get; set; }
    
    public string? Observation { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public List<OrderItemRequestDto> OrderItems { get; set; }    
}

public class OrderItemRequestDto 
{
    public int Quantity { get; set; }
    public int ProductId { get; set; }
    public decimal Value { get; set; }
}


