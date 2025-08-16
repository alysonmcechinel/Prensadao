using Prensadao.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Prensadao.Application.DTOs.Responses;

public class CustomerResponseDto
{
    public CustomerResponseDto(int id, string name, long phone, string street, string district, string number, string city, string referencePoint, int cep, List<OrderReponseDto> orders)
    {
        CustomerId = id;
        Name = name;
        Phone = phone;
        Street = street;
        District = district;
        Number = number;
        City = city;
        ReferencePoint = referencePoint;
        Cep = cep;

        Orders = orders;
    }

    [Key]
    public int CustomerId { get; set; }
    public string Name { get; set; }
    public long Phone { get; set; }
    public string Street { get; set; }
    public string District { get; set; }
    public string Number { get; set; }
    public string City { get; set; }
    public string ReferencePoint { get; set; }
    public int Cep { get; set; }

    public List<OrderReponseDto> Orders { get; set; }

    #region Mapeamento
    public static CustomerResponseDto ToDto(Customer customer)
    {
        var custumerOrders = new List<OrderReponseDto>();
        if (customer.Orders != null)
            custumerOrders = customer.Orders.Select(o => OrderReponseDto.ToDto(o)).ToList();

        return new CustomerResponseDto(customer.CustomerId, customer.Name, customer.Phone, customer.Street, customer.District, customer.Number, customer.City, customer.ReferencePoint, customer.Cep, custumerOrders);
    }

    public static List<CustomerResponseDto> ToListDto(List<Customer> customers) => customers.Select(c => ToDto(c)).ToList();
    #endregion
}
