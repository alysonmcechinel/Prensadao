using Prensadao.Domain.Enum;

namespace Prensadao.Application.DTOs
{
    public class NotifyMessageDTO
    {
        public int OrderId { get; set; }
        public OrderStatusEnum OrderStatus { get; set; }
        public bool Delivery { get; set; }
        public string ConsumerName { get; set; }
        public long Phone { get; set; }
    }
}
