using System.ComponentModel.DataAnnotations;

namespace Prensadao.Domain.Entities;

public class Product
{
    public Product() { }

    public Product(string name, decimal price, string description)
    {
        Name = name;
        Price = price;
        Description = description;
        Enabled = true;

        OrderItems = new List<OrderItem>();
    }

    [Key]
    public int ProductId { get; private set; }
    public string Name { get; private set; }
    public bool Enabled { get; private set; }
    public decimal Price { get; private set; }
    public string Description { get; private set; }

    //Relationship
    public ICollection<OrderItem> OrderItems { get; private set; } = [];

    public void SetEnabled(bool enabled) => Enabled = enabled;

    public void UpdateDetails(string name, bool enabled, decimal price, string description)
    {
        Name = name;
        Enabled = enabled;
        Price = price;
        Description = description;
    }
}
