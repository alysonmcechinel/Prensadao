using Prensadao.Application.Interfaces;
using Prensadao.Application.Models.Request;
using Prensadao.Application.Models.Response;
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

        public async Task<int> AddCustomer(CustomerRequestDto dto)
        {
            bool phoneIsExists = await _customerRepository.PhoneIsExists(dto.Phone);
            if (phoneIsExists)
                throw new Exception("Telefone já cadastrado.");

            if (!PhoneIsValid(dto.Phone))
                throw new Exception("Numero de telefone invalido.");

            var customer = new Customer(dto.Name, dto.Phone, dto.Street, dto.District, dto.Number, dto.City, dto.ReferencePoint, dto.Cep);

            return await _customerRepository.AddCustomer(customer);
        }

        public async Task<CustomerResponseDto> GetById(int id)
        {
            var customer = await _customerRepository.GetById(id);

            if (customer == null)
                throw new Exception("Cliente não encontrado");

            return CustomerResponseDto.ToDto(customer);
        }

        public async Task<List<CustomerResponseDto>> GetCustomers() => CustomerResponseDto.ToListDto(await _customerRepository.GetCustomers());

        public async Task Update(CustomerRequestDto dto)
        {
            var customer = await _customerRepository.GetById(dto.CustomerId);

            if (customer == null)
                throw new Exception("Cliente não encontrado");

            if (!PhoneIsValid(dto.Phone))
                throw new Exception("Numero de telefone invalido.");

            customer.Update(dto.Name, dto.Phone, dto.Street, dto.District, dto.Number, dto.City, dto.ReferencePoint, dto.Cep);
            await _customerRepository.Update(customer);
        }

        // privates

        public bool PhoneIsValid(long phone)
        {
            string phoneText = phone.ToString();
            return phoneText.Length == 10 || phoneText.Length == 11;
        }
    }
}
