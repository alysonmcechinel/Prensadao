using System.ComponentModel.DataAnnotations;

namespace Prensadao.Domain.Entities;

public class Product
{
    public Product() { }

    public Product(string name, decimal value, string description)
    {
        Name = name;
        Value = value;
        Description = description;
        Enabled = true;
    }

    [Key]
    public int ProductId { get; private set; }
    public string Name { get; private set; }
    public bool Enabled { get; private set; }
    public decimal Value { get; private set; }    
    public string Description { get; private set; }

    //Relationship
    public ICollection<OrderItem> OrderItems { get; set; }

    public void DisableProduct() => Enabled = false;
    public void EnabledProduct() => Enabled = true;
}
