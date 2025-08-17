using Prensadao.Application.Helpers;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Prensadao.Application.DTOs.Responses
{
    public class OrderResponseDto
    {
        public OrderResponseDto(int orderId, OrderStatusEnum orderStatus, bool delivery, decimal value, string observation, int customerId, string customerName, DateTime dateOrder, List<OrderItemResponseDto> ordemItems)
        {
            OrderId = orderId;
            OrderStatus = orderStatus.GetDescription();
            Delivery = delivery;
            Value = value;
            Observation = observation;
            CustomerId = customerId;
            CustomerName = customerName;
            DateOrder = dateOrder.ToBrasil();

            OrderItems = ordemItems;
        }

        [Key]
        public int? OrderId { get; set; }
        public string OrderStatus { get; set; }
        public bool Delivery { get; set; }
        public decimal Value { get; set; }
        public string? Observation { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime DateOrder { get; set; }
        public List<OrderItemResponseDto> OrderItems { get; set; }

        #region Mapeamento
        public static OrderResponseDto ToDto(Order order) => new OrderResponseDto(order.OrderId, order.OrderStatus, order.Delivery, order.Value, order.Observation, order.CustomerId, order.Customer.Name, order.DateOrder, ToListItens(order.OrderItems.ToList()));

        public static List<OrderResponseDto> ToListDto(List<Order> orders)
        {
            var orderResponse = new List<OrderResponseDto>();

            foreach (var order in orders)
            {
                var ordemItens = ToListItens(order.OrderItems.ToList());

                orderResponse.Add(new OrderResponseDto(order.OrderId, order.OrderStatus, order.Delivery, order.Value, order.Observation, order.CustomerId, order.Customer.Name, order.DateOrder, ordemItens));
            }

            return orderResponse;
        }

        private static List<OrderItemResponseDto> ToListItens(List<OrderItem> orderItem) => orderItem.Select(x => new OrderItemResponseDto
        {
            OrderItemId = x.OrderItemId,
            OrderId = x.OrderId,
            ProductId = x.ProductId,
            Quantity = x.Quantity,
            ProductValue = x.Product.Value,
            ProductName = x.Product.Name
        }).ToList();
        #endregion
    }
}


