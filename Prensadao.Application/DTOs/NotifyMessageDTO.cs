using Prensadao.Domain.Enums;

namespace Prensadao.Application.DTOs;

public class NotifyMessageDto
{
    public int OrderId { get; set; }
    public OrderStatusEnum OrderStatus { get; set; }
    public bool Delivery { get; set; }
    public string ConsumerName { get; set; } = string.Empty;
    public long Phone { get; set; }
}
