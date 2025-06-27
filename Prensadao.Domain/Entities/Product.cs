namespace Prensadao.Domain.Entities;

public class Product
{
    public int ProductId { get; private set; }
    public string Name { get; private set; }
    public decimal Value { get; private set; }
    public string Description { get; private set; }

    //Relationship
    public ICollection<OrderItem> OrderItems { get; set; }
}
