using Prensadao.Domain.Enums;

namespace Prensadao.Application.DTOs.Requests;

public class UpdateStatusDto
{
    public int OrderId { get; set; }
    public OrderStatusEnum OrderStatus { get; set; }
}
