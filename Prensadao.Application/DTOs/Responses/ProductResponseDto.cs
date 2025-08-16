using Prensadao.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Prensadao.Application.DTOs.Responses;

public class ProductResponseDto
{
    public ProductResponseDto(int id, string name, bool enabled, decimal value, string description)
    {
        ProductId = id;
        Name = name;
        Enabled = enabled;
        Value = value;
        Description = description;
    }

    [Key]
    public int ProductId { get; set; }
    public string Name { get; set; }
    public bool Enabled { get; set; }
    public decimal Value { get; set; }
    public string Description { get; set; }

    #region Mapeamentos
    public static ProductResponseDto ToDto(Product product)
    {
        return new ProductResponseDto(product.ProductId, product.Name, product.Enabled, product.Value, product.Description);
    }

    public static List<ProductResponseDto> ToListDto(List<Product> products) => products.Select(p => ToDto(p)).ToList();
    #endregion
}
