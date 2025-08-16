using Prensadao.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Prensadao.Application.DTOs.Requests
{
    public class ProductRequestDto
    {
        [Key]
        public int ProductId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public decimal Value { get; set; }
        public string Description { get; set; }
    }

    public class ProductEnabledDto
    {
        public int ProductId { get; set; }
        public bool Enabled { get; set; }
    }
}