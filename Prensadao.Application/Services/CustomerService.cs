using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
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
            ValidatePhone(dto.Phone);

            if (await PhoneExistsAsync(dto.Phone))
                throw new InvalidOperationException("Telefone já cadastrado.");

            var customer = new Customer(dto.Name, dto.Phone, dto.Street, dto.District, dto.Number, dto.City, dto.ReferencePoint, dto.Cep);

            return await _customerRepository.AddCustomerAsync(customer);
        }

        public async Task<CustomerResponseDto> GetById(int id)
        {
            var customer = await _customerRepository.GetById(id);

            if (customer == null)
                throw new Exception("Cliente não encontrado.");

            return CustomerResponseDto.ToDto(customer);
        }

        public async Task<List<CustomerResponseDto>> GetCustomers() => CustomerResponseDto.ToListDto(await _customerRepository.GetCustomers());

        public async Task Update(CustomerRequestDto dto)
        {
            ValidatePhone(dto.Phone);

            if (!dto.CustomerId.HasValue)
                throw new Exception("O ID informado incorretamente.");

            var customer = await _customerRepository.GetById(dto.CustomerId!.Value);

            if (customer == null)
                throw new Exception("Cliente não encontrado.");

            customer.Update(dto.Name, dto.Phone, dto.Street, dto.District, dto.Number, dto.City, dto.ReferencePoint, dto.Cep);
            await _customerRepository.Update(customer);
        }

        // privates

        // Eliding Async/Await: wrapper puro, sem lógica extra (sem state machine desnecessária)
        private Task<bool> PhoneExistsAsync(string phone) => _customerRepository.PhoneIsExists(phone);

        // Validação síncrona (não tem porque ser async)
        private void ValidatePhone(string phone)
        {
            phone = phone.Trim();
            if (!(phone.Length == 10 || phone.Length == 11))
            {
                throw new ArgumentException("Número de telefone inválido.");
            }
        }

    }
}
