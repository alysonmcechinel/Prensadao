using AutoFixture.Xunit2;
using FakeItEasy;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.Services;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Test.Application;

public class CustomerServiceTest
{
    // Theory = aceita parametros para o teste unitario;
    // Fact = é sem parametros;
    // AutoFakeItEasyData = cria os dados automaticos para os parametros do metodo de teste unitario;
    // Frozen = garante que a mesma instancia seja reutilizada para o parametro (mesmo objeto)

    [Theory, AutoFakeItEasyData]
    public async Task AddCustomer_DeveLancar_TelefoneJaCadastrado(
        [Frozen] ICustomerRepository customerRepository,
        CustomerService customerService,
        CustomerRequestDto dto)
    {
        // Arrange: telefone válido, mas já existente
        dto.Phone = "48999999999";
        A.CallTo(() => customerRepository.PhoneIsExistsAsync(dto.Phone)).Returns(true);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => customerService.AddCustomerAsync(dto));

        // Assert
        Assert.Equal("Telefone já cadastrado.", ex.Message);
        A.CallTo(() => customerRepository.PhoneIsExistsAsync(dto.Phone)).MustHaveHappenedOnceExactly(); // Verifica que consultou existência uma única vez
        A.CallTo(() => customerRepository.AddCustomerAsync(A<Customer>._)).MustNotHaveHappened(); // Não deve tentar inserir
    }

    [Theory, AutoFakeItEasyData]
    public async Task AddCustomer_DeveLancar_NumeroTelefoneInvalido(
        [Frozen] ICustomerRepository customerRepository,
        CustomerService customerService,
        CustomerRequestDto dto)
    {
        // Arrange: telefone inválido, e garantimos que "não existe"
        dto.Phone = "123"; // inválido pelos critérios da service
        A.CallTo(() => customerRepository.PhoneIsExistsAsync(dto.Phone)).Returns(false);

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => customerService.AddCustomerAsync(dto));

        // Assert
        Assert.Equal("Número de telefone inválido.", ex.Message);
        A.CallTo(() => customerRepository.AddCustomerAsync(A<Customer>._)).MustNotHaveHappened(); // Não deve tentar inserir
        A.CallTo(() => customerRepository.PhoneIsExistsAsync(dto.Phone)).MustNotHaveHappened(); // A validação deve falhar antes de consultar o repositório
    }
    
    [Theory, AutoFakeItEasyData]
    public async Task AddCustomer_DeveRetornarId_QuandoSucesso(
        [Frozen] ICustomerRepository customerRepository,
        CustomerService customerService,
        CustomerRequestDto dto,
        int novoId)
    {
        // Arrange: telefone válido e não existente
        dto.Phone = "48988887777";
        A.CallTo(() => customerRepository.PhoneIsExistsAsync(dto.Phone)).Returns(false);
        A.CallTo(() => customerRepository.AddCustomerAsync(A<Customer>._)).Returns(novoId); // Simula ID gerado pelo repositório

        // Act
        var id = await customerService.AddCustomerAsync(dto);

        // Assert
        Assert.Equal(novoId, id);
        A.CallTo(() => customerRepository.AddCustomerAsync(
            A<Customer>.That.Matches(c =>
                c.Phone == dto.Phone &&
                c.Name == dto.Name &&
                c.City == dto.City)))
            .MustHaveHappenedOnceExactly(); // Verifica que inseriu exatamente 1x com os dados esperados
        A.CallTo(() => customerRepository.PhoneIsExistsAsync(dto.Phone)).MustHaveHappenedOnceExactly(); // Verifica a chamada de verificação de existência
    }

    [Theory, AutoFakeItEasyData]
    public async Task GetById_DeveLancar_ClienteNaoEncontrado(
        [Frozen] ICustomerRepository customerRepository,
        CustomerService customerService,
        int idInexistente)
    {
        // Arrange
        A.CallTo(() => customerRepository.GetByIdAsync(idInexistente)).Returns(Task.FromResult<Customer>(null));

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => customerService.GetByIdAsync(idInexistente));

        // Assert
        Assert.Equal("Cliente não encontrado.", ex.Message);
        A.CallTo(() => customerRepository.GetByIdAsync(idInexistente)).MustHaveHappenedOnceExactly();
    }

    [Theory, AutoFakeItEasyData]
    public async Task GetById_DeveRetornar_CustomerResponseDto(
    [Frozen] ICustomerRepository customerRepository,
    CustomerService customerService)
    {
        // Arrange
        var customer = new Customer("John Doe", "48988887777", "Street 1", "District A", "100", "CityX", "Near Park", 123456);
        typeof(Customer).GetProperty("CustomerId")!.SetValue(customer, 1);
        A.CallTo(() => customerRepository.GetByIdAsync(customer.CustomerId)).Returns(Task.FromResult(customer));

        // Act
        var response = await customerService.GetByIdAsync(customer.CustomerId);

        // Assert
        Assert.Equal(customer.CustomerId, response.CustomerId);
        A.CallTo(() => customerRepository.GetByIdAsync(customer.CustomerId)).MustHaveHappenedOnceExactly();
    }

    [Theory, AutoFakeItEasyData]
    public async Task Update_DeveLancar_ClienteNaoEncontrado(
        [Frozen] ICustomerRepository customerRepository,
        CustomerService customerService,
        CustomerRequestDto dto)
    {
        // Arrange: garantir que o cenário falhe por cliente não encontrado
        dto.CustomerId = 1;
        dto.Phone = "48999999999";
        A.CallTo(() => customerRepository.GetByIdAsync(dto.CustomerId)).Returns(Task.FromResult<Customer>(null));

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => customerService.UpdateAsync(dto));

        // Assert
        Assert.Equal("Cliente não encontrado.", ex.Message);
        A.CallTo(() => customerRepository.GetByIdAsync(dto.CustomerId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => customerRepository.UpdateAsync(A<Customer>._)).MustNotHaveHappened(); // Verificar que o método Update do repository não foi chamado
    }
}
