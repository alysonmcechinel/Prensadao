using Prensadao.Domain.Entities;

namespace Prensadao.Application.DTOs.Responses;

public class CustomerResponseDto
{
    public CustomerResponseDto(int id, string name, string phone, string street, string district, string number, string city, string referencePoint, int cep, List<OrderResponseDto> orders)
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

    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? ReferencePoint { get; set; }
    public int Cep { get; set; }

    public List<OrderResponseDto> Orders { get; set; } = [];

    #region Mapeamento
    public static CustomerResponseDto ToDto(Customer customer)
    {
        var customerOrders = new List<OrderResponseDto>();
        if (customer.Orders != null)
            customerOrders = customer.Orders.Select(o => OrderResponseDto.ToDto(o)).ToList();

        return new CustomerResponseDto(customer.CustomerId, customer.Name, customer.Phone, customer.Street, customer.District, customer.Number, customer.City, customer.ReferencePoint, customer.Cep, customerOrders);
    }

    public static List<CustomerResponseDto> ToListDto(List<Customer> customers) => customers.Select(c => ToDto(c)).ToList();
    #endregion
}
