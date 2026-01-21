using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Application.Helpers;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Application.Services
{
    //TODO: implementar FluentValidation
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        // Encapsulando a verificação e usando Eliding Async/Await no wrapper.
        // Aqui o método continua async porque precisamos do await para tomar decisão (if).
        public async Task<int> AddCustomerAsync(CustomerRequestDto dto)
        {
            dto.Phone.ValidatePhone();

            if (await PhoneExistsAsync(dto.Phone))
                throw new InvalidOperationException("Telefone já cadastrado.");

            var customer = new Customer(dto.Name, dto.Phone, dto.Street, dto.District, dto.Number, dto.City, dto.ReferencePoint, dto.Cep);

            return await _customerRepository.AddCustomerAsync(customer);
        }

        public async Task<CustomerResponseDto> GetByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);

            if (customer == null)
                throw new ArgumentException("Cliente não encontrado.");

            return CustomerResponseDto.ToDto(customer);
        }

        public async Task<List<CustomerResponseDto>> GetCustomersAsync() => CustomerResponseDto.ToListDto(await _customerRepository.GetCustomersAsync());

        public async Task UpdateAsync(CustomerRequestDto dto)
        {
            dto.Phone.ValidatePhone();

            if (dto.CustomerId == 0)
                throw new ArgumentException("O ID informado incorretamente.");

            var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
            if (customer == null)
                throw new ArgumentException("Cliente não encontrado.");

            customer.Update(dto.Name, dto.Phone, dto.Street, dto.District, dto.Number, dto.City, dto.ReferencePoint, dto.Cep);
            await _customerRepository.UpdateAsync(customer);
        }

        // privates

        // Eliding Async/Await: wrapper puro, sem lógica extra (sem state machine desnecessária)
        private Task<bool> PhoneExistsAsync(string phone) => _customerRepository.PhoneIsExistsAsync(phone);
    }
}
