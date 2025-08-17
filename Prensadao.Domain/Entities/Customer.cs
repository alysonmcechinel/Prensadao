using System.ComponentModel.DataAnnotations;

namespace Prensadao.Domain.Entities;

public class Customer
{
    public Customer() { }

    public Customer(string name, long phone, string street, string district, string number, string city, string refencePoint, int cep)
    {
        Name = name;
        Phone = phone;
        Street = street;
        District = district;
        Number = number;
        City = city;
        ReferencePoint = refencePoint;
        Cep = cep;

        Orders = new List<Order>();
    }

    [Key]
    public int CustomerId { get; private set; }
    public string Name { get; private set; }
    public long Phone { get; private set; }
    public string Street { get; private set; }
    public string District { get; private set; }
    public string Number { get; private set; }
    public string City { get; private set; }
    public string ReferencePoint { get; private set; }
    public int Cep { get; private set; }

    // Relationship
    public ICollection<Order> Orders { get; set; } = [];

    public void Update(string name, long phone, string street, string district, string number, string city, string refencePoint, int cep)
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
}
