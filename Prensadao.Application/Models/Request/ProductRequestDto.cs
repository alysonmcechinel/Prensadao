using Prensadao.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Prensadao.Application.Models.Request
{
    public class ProductRequestDto
    {
        [Key]
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Enabled { get; set; }
        public decimal Value { get; set; }
        public string Description { get; set; }        
    }
}
