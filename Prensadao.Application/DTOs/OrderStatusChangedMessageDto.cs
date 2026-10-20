using Prensadao.Domain.Enums;

namespace Prensadao.Application.DTOs;

public class OrderStatusChangedMessageDto
{
    public required int OrderId { get; set; }
    public required OrderStatusEnum Status { get; set; }
    public required bool IsDelivery { get; set; }
}
