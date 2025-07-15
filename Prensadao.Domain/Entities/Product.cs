using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Prensadao.Domain.Entities;

public class Product
{
    public Product() { }

    public Product(string name, decimal value, string description)
    {
        Name = name;
        Value = value;
        Description = description;
    }

    [Key]
    public int ProductId { get; private set; }
    public string Name { get; private set; }
    public decimal Value { get; private set; }
    public string Description { get; private set; }

    //Relationship
    public ICollection<OrderItem> OrderItems { get; set; }
}
