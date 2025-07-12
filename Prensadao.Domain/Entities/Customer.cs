using System.Text.Json.Serialization;

namespace Prensadao.Domain.Entities;

public class Customer
{
    public Customer()
    {
        
    }

    public Customer(string name, int phone, string street, string district, string number, string city, string refencePoint, int cep)
    {
        Name = name;
        Phone = phone;
        Street = street;
        District = district;
        Number = number;
        City = city;
        ReferencePoint = refencePoint;
        Cep = cep;
    }

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
    [JsonIgnore]
    public ICollection<Order> Orders { get; set; }
}
