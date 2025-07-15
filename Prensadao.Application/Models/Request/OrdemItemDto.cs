using System.ComponentModel.DataAnnotations;

namespace Prensadao.Application.Models.Request
{
    public class OrderItemDto
    {
        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Value { get; set; }

        [Required]
        public int ProductId { get; set; }
    }
}
