using AutoFixture.Xunit2;
using FakeItEasy;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.Services;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Test.Application;

public class CustomerServiceTest
{
    [Theory, AutoFakeItEasyData]
    public async Task AddCustomer_DeveLancar_TelefoneJaCadastrado(
        [Frozen] ICustomerRepository customerRepository,
        CustomerService customerService,
        CustomerRequestDto dto)
    {
        // Arrange: telefone válido, mas já existente
        dto.Phone = 48999999999;
        A.CallTo(() => customerRepository.PhoneIsExists(dto.Phone)).Returns(true);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => customerService.AddCustomer(dto));
        Assert.Equal("Telefone já cadastrado.", ex.Message);

        // Não deve tentar inserir
        A.CallTo(() => customerRepository.AddCustomer(A<Customer>._)).MustNotHaveHappened();
        // Verifica que consultou existência uma única vez
        A.CallTo(() => customerRepository.PhoneIsExists(dto.Phone)).MustHaveHappenedOnceExactly();
    }

    [Theory, AutoFakeItEasyData]
    public async Task AddCustomer_DeveLancar_NumeroTelefoneInvalido(
        [Frozen] ICustomerRepository customerRepository,
        CustomerService customerService,
        CustomerRequestDto dto)
    {
        // Arrange: telefone inválido, e garantimos que "não existe"
        dto.Phone = 123; // inválido pelos critérios da service
        A.CallTo(() => customerRepository.PhoneIsExists(dto.Phone)).Returns(false);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => customerService.AddCustomer(dto));
        Assert.Equal("Número de telefone inválido.", ex.Message);

        // Não deve tentar inserir
        A.CallTo(() => customerRepository.AddCustomer(A<Customer>._)).MustNotHaveHappened();
        // Verifica que a consulta aconteceu
        A.CallTo(() => customerRepository.PhoneIsExists(dto.Phone)).MustHaveHappenedOnceExactly();
    }

    [Theory, AutoFakeItEasyData]
    public async Task AddCustomer_DeveRetornarId_QuandoSucesso(
        [Frozen] ICustomerRepository customerRepository,
        CustomerService customerService,
        CustomerRequestDto dto)
    {
        // Arrange: telefone válido e não existente
        dto.Phone = 48988887777;
        A.CallTo(() => customerRepository.PhoneIsExists(dto.Phone)).Returns(false);

        // Simula ID gerado pelo repositório
        const int novoId = 77;
        A.CallTo(() => customerRepository.AddCustomer(A<Customer>._)).Returns(novoId);

        // Act
        var id = await customerService.AddCustomer(dto);

        // Assert
        Assert.Equal(novoId, id);

        // Verifica que inseriu exatamente 1x com os dados esperados
        A.CallTo(() => customerRepository.AddCustomer(
            A<Customer>.That.Matches(c =>
                c.Phone == dto.Phone &&
                c.Name == dto.Name &&
                c.City == dto.City)))
            .MustHaveHappenedOnceExactly();

        // Verifica a chamada de verificação de existência
        A.CallTo(() => customerRepository.PhoneIsExists(dto.Phone)).MustHaveHappenedOnceExactly();
    }

    [Theory, AutoFakeItEasyData]
    public async Task GetById_DeveLancar_ClienteNaoEncontrado(
        [Frozen] ICustomerRepository customerRepository,
        CustomerService customerService)
    {
        // Arrange
        int idInexistente = 999;
        A.CallTo(() => customerRepository.GetById(idInexistente)).Returns(Task.FromResult<Customer>(null));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => customerService.GetById(idInexistente));
        Assert.Equal("Cliente não encontrado.", ex.Message);
    }

    [Theory, AutoFakeItEasyData]
    public async Task GetById_DeveRetornar_CustomerResponseDto(
    [Frozen] ICustomerRepository customerRepository,
    CustomerService customerService)
    {
        // Arrange
        var customer = new Customer("John Doe", 48988887777, "Street 1", "District A", "100", "CityX", "Near Park", 123456);
        typeof(Customer).GetProperty("CustomerId").SetValue(customer, 1);

        A.CallTo(() => customerRepository.GetById(customer.CustomerId)).Returns(Task.FromResult(customer));

        // Act
        var response = await customerService.GetById(customer.CustomerId);

        // Assert
        Assert.Equal(customer.CustomerId, response.CustomerId);
    }

    [Theory, AutoFakeItEasyData]
    public async Task Update_DeveLancar_ClienteNaoEncontrado(
        [Frozen] ICustomerRepository customerRepository,
        CustomerService customerService,
        CustomerRequestDto dto)
    {
        // Arrange: simular customer inexistente
        A.CallTo(() => customerRepository.GetById(dto.CustomerId!.Value)).Returns(Task.FromResult<Customer>(null));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => customerService.Update(dto));
        Assert.Equal("Cliente não encontrado.", ex.Message);

        // Verificar que o método Update do repository não foi chamado
        A.CallTo(() => customerRepository.Update(A<Customer>._)).MustNotHaveHappened();
    }    
}
