using System.ComponentModel.DataAnnotations;

namespace Prensadao.Application.DTOs.Requests;

public class OrderRequestDto
{
    public int? OrderId { get; set; }

    [Required]
    public bool Delivery { get; set; }
    
    public string? Observation { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public List<OrderItemRequestDto> OrderItems { get; set; } = [];
}

public class OrderItemRequestDto 
{
    public int Quantity { get; set; }
    public int ProductId { get; set; }
}


