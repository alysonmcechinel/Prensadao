using NodaTime.Extensions;
using Prensadao.Application.Helpers;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Prensadao.Application.DTOs.Responses
{
    public class OrderResponseDto
    {
        public OrderResponseDto(int orderId, OrderStatusEnum status, bool delivery, decimal totalAmount, string notes, int customerId, string customerName, DateTime createdAt, List<OrderItemResponseDto> ordemItems)
        {
            OrderId = orderId;
            Status = status.GetDescription();
            Delivery = delivery;
            TotalAmount = totalAmount;
            Notes = notes;
            CustomerId = customerId;
            CustomerName = customerName;
            CreatedAt = createdAt.ToSaoPauloDateTimeOffset();

            OrderItems = ordemItems;
        }

        public int? OrderId { get; set; }
        public string Status { get; set; }
        public bool Delivery { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public List<OrderItemResponseDto> OrderItems { get; set; } = [];

        #region Mapeamento
        public static OrderResponseDto ToDto(Order order) => new OrderResponseDto(order.OrderId, order.Status, order.IsDelivery, order.TotalAmount, order.Notes, order.CustomerId, order.Customer.Name, order.CreatedAt, ToListItens(order.OrderItems.ToList()));

        public static List<OrderResponseDto> ToListDto(List<Order> orders)
        {
            var orderResponse = new List<OrderResponseDto>();

            foreach (var order in orders)
            {
                var ordemItens = ToListItens(order.OrderItems.ToList());

                orderResponse.Add(new OrderResponseDto(order.OrderId, order.Status, order.IsDelivery, order.TotalAmount, order.Notes, order.CustomerId, order.Customer.Name, order.CreatedAt, ordemItens));
            }

            return orderResponse;
        }

        private static List<OrderItemResponseDto> ToListItens(List<OrderItem> orderItem) => orderItem.Select(x => new OrderItemResponseDto
        {
            OrderItemId = x.OrderItemId,
            OrderId = x.OrderId,
            ProductId = x.ProductId,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            ProductName = x.Product.Name
        }).ToList();
        #endregion
    }
}
