using Prensadao.Application.Models.Request;
using Prensadao.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Prensadao.Application.Models.Response
{
    public class OrderReponseDto
    {
        public OrderReponseDto(int orderId, bool delivery, decimal value, string observation, int customerId, string customerName, List<OrderItemResponseDto> ordemItems)
        {
            OrderId = orderId;
            Delivery = delivery;
            Value = value;
            Observation = observation;
            CustomerId = customerId;
            CustomerName = customerName;

            OrderItems = ordemItems;
        }

        [Key]
        public int? OrderId { get; set; }
        public bool Delivery { get; set; }
        public decimal Value { get; set; }
        public string? Observation { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public List<OrderItemResponseDto> OrderItems { get; set; }

        public static List<OrderReponseDto> FromListOrder(List<Order> orders)
        {
            var orderResponse = new List<OrderReponseDto>();

            foreach (var order in orders)
            {
                var ordemItens = order.OrderItems.Select(x => new OrderItemResponseDto
                {
                    OrderItemId = x.OrderItemId,
                    OrderId = x.OrderId,
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    ProductValue = x.Product.Value,
                    ProductName = x.Product.Name
                }).ToList();

                orderResponse.Add(new OrderReponseDto(order.OrderId, order.Delivery, order.Value, order.Observation, order.CustomerId, order.Customer.Name, ordemItens));
            }

            return orderResponse;
        }
    }
}


