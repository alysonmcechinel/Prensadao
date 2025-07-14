using Prensadao.Application.Interfaces;
using Prensadao.Application.Models;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<int> AddCustomer(CustomerDto dto)
        {
            var customer = new Customer(dto.Name, dto.Phone, dto.Street, dto.District, dto.Number, dto.City, dto.ReferencePoint, dto.Cep);

            return await _customerRepository.AddCustomer(customer);
        }

        public async Task<List<Customer>> GetCustomers() => await _customerRepository.GetCustomers();
    }
}
