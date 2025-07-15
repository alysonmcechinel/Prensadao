using System.ComponentModel.DataAnnotations;

namespace Prensadao.Application.Models.Request
{
    public class ProductDto
    {
        [Key]
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public string Description { get; set; }
    }
}
