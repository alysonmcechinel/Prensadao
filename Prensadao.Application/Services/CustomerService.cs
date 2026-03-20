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
            ValidateCustomerRequest(dto);

            await EnsurePhoneIsAvailableAsync(dto.Phone);

            return await _customerRepository.AddAsync(CreateCustomer(dto));
        }

        public async Task<CustomerResponseDto> GetByIdAsync(int id)
        {
            return CustomerResponseDto.ToDto(await GetCustomerDetailsByIdOrThrowAsync(id));
        }

        public async Task<List<CustomerResponseDto>> GetCustomersAsync() => CustomerResponseDto.ToListDto(await _customerRepository.GetAllWithDetailsAsync());

        public async Task UpdateAsync(CustomerRequestDto dto)
        {
            ValidateCustomerRequest(dto);
            ValidateCustomerId(dto.CustomerId);

            var customer = await GetCustomerByIdOrThrowAsync(dto.CustomerId);
            UpdateCustomer(customer, dto);
            await _customerRepository.UpdateAsync(customer);
        }

        // privates

        private static Customer CreateCustomer(CustomerRequestDto dto)
            => new(dto.Name, dto.Phone, dto.Street, dto.District, dto.Number, dto.City, dto.ReferencePoint, dto.Cep);

        private static void UpdateCustomer(Customer customer, CustomerRequestDto dto)
            => customer.Update(dto.Name, dto.Phone, dto.Street, dto.District, dto.Number, dto.City, dto.ReferencePoint, dto.Cep);

        private static void ValidateCustomerRequest(CustomerRequestDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            dto.Phone.ValidatePhone();
        }

        private static void ValidateCustomerId(int customerId)
        {
            if (customerId <= 0)
                throw new ArgumentException("O ID informado incorretamente.");
        }

        private async Task<Customer> GetCustomerByIdOrThrowAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer is null)
                throw new ArgumentException("Cliente não encontrado.");

            return customer;
        }

        private async Task<Customer> GetCustomerDetailsByIdOrThrowAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdWithDetailsAsync(customerId);

            if (customer is null)
                throw new ArgumentException("Cliente não encontrado.");

            return customer;
        }

        private async Task EnsurePhoneIsAvailableAsync(string phone)
        {
            if (await PhoneExistsAsync(phone))
                throw new InvalidOperationException("Telefone já cadastrado.");
        }

        // Eliding Async/Await: wrapper puro, sem lógica extra (sem state machine desnecessária)
        private Task<bool> PhoneExistsAsync(string phone) => _customerRepository.ExistsByPhoneAsync(phone);
    }
}
