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

    public void EnabledProduct(bool enabled) => Enabled = enabled;

    public void Update(string name, bool enabled, decimal value, string description)
    {
        Name = name;
        Enabled = enabled;
        Value = value;
        Description = description;
    }
}
