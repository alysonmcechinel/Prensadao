namespace Prensadao.Domain.Entities;

public class Customer
{
    public int CustomerId { get; private set; }
    public string Name { get; private set; }
    public int Phone { get; private set; }
    public string Street { get; private set; }
    public string District { get; private set; }
    public string Number { get; private set; }
    public string City { get; private set; }
    public string ReferencePoint { get; private set; }
    public int Cep { get; private set; }

    // Relationship
    public ICollection<Order> Orders { get; set; }
}
