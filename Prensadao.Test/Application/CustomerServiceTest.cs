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
        // arrange: telefone válido, mas já existente
        dto.Phone = "48999999999";
        A.CallTo(() => customerRepository.ExistsByPhoneAsync(dto.Phone)).Returns(true);

        // act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => customerService.AddCustomerAsync(dto));

        // assert
        Assert.Equal("Telefone já cadastrado.", ex.Message);
        A.CallTo(() => customerRepository.ExistsByPhoneAsync(dto.Phone)).MustHaveHappenedOnceExactly(); // Verifica que consultou existência uma única vez
        A.CallTo(() => customerRepository.AddAsync(A<Customer>._)).MustNotHaveHappened(); // Não deve tentar inserir
    }

    [Theory, AutoFakeItEasyData]
    public async Task AddCustomer_DeveLancar_NumeroTelefoneInvalido(
        [Frozen] ICustomerRepository customerRepository,
        CustomerService customerService,
        CustomerRequestDto dto)
    {
        // arrange: telefone inválido, e garantimos que "não existe"
        dto.Phone = "123"; // inválido pelos critérios da service
        A.CallTo(() => customerRepository.ExistsByPhoneAsync(dto.Phone)).Returns(false);

        // act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => customerService.AddCustomerAsync(dto));

        // assert
        Assert.Equal("Número de telefone inválido.", ex.Message);
        A.CallTo(() => customerRepository.AddAsync(A<Customer>._)).MustNotHaveHappened(); // Não deve tentar inserir
        A.CallTo(() => customerRepository.ExistsByPhoneAsync(dto.Phone)).MustNotHaveHappened(); // A validação deve falhar antes de consultar o repositório
    }
    
    [Theory, AutoFakeItEasyData]
    public async Task AddCustomer_DeveRetornarId_QuandoSucesso(
        [Frozen] ICustomerRepository customerRepository,
        CustomerService customerService,
        CustomerRequestDto dto,
        int novoId)
    {
        // arrange: telefone válido e não existente
        dto.Phone = "48988887777";
        A.CallTo(() => customerRepository.ExistsByPhoneAsync(dto.Phone)).Returns(false);
        A.CallTo(() => customerRepository.AddAsync(A<Customer>._)).Returns(novoId); // Simula ID gerado pelo repositório

        // act
        var id = await customerService.AddCustomerAsync(dto);

        // assert
        Assert.Equal(novoId, id);
        A.CallTo(() => customerRepository.AddAsync(
            A<Customer>.That.Matches(c =>
                c.Phone == dto.Phone &&
                c.Name == dto.Name &&
                c.City == dto.City)))
            .MustHaveHappenedOnceExactly(); // Verifica que inseriu exatamente 1x com os dados esperados
        A.CallTo(() => customerRepository.ExistsByPhoneAsync(dto.Phone)).MustHaveHappenedOnceExactly(); // Verifica a chamada de verificação de existência
    }

    [Theory, AutoFakeItEasyData]
    public async Task GetById_DeveLancar_ClienteNaoEncontrado(
        [Frozen] ICustomerRepository customerRepository,
        CustomerService customerService,
        int idInexistente)
    {
        // arrange
        A.CallTo(() => customerRepository.GetByIdWithDetailsAsync(idInexistente)).Returns(Task.FromResult<Customer?>(null));

        // act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => customerService.GetByIdAsync(idInexistente));

        // assert
        Assert.Equal("Cliente não encontrado.", ex.Message);
        A.CallTo(() => customerRepository.GetByIdWithDetailsAsync(idInexistente)).MustHaveHappenedOnceExactly();
    }

    [Theory, AutoFakeItEasyData]
    public async Task GetById_DeveRetornar_CustomerResponseDto(
    [Frozen] ICustomerRepository customerRepository,
    CustomerService customerService)
    {
        // arrange
        var customer = new Customer("John Doe", "48988887777", "Street 1", "District A", "100", "CityX", "Near Park", 123456);
        typeof(Customer).GetProperty("CustomerId")!.SetValue(customer, 1);
        A.CallTo(() => customerRepository.GetByIdWithDetailsAsync(customer.CustomerId)).Returns(Task.FromResult<Customer?>(customer));

        // act
        var response = await customerService.GetByIdAsync(customer.CustomerId);

        // assert
        Assert.Equal(customer.CustomerId, response.CustomerId);
        A.CallTo(() => customerRepository.GetByIdWithDetailsAsync(customer.CustomerId)).MustHaveHappenedOnceExactly();
    }

    [Theory, AutoFakeItEasyData]
    public async Task Update_DeveLancar_ClienteNaoEncontrado(
        [Frozen] ICustomerRepository customerRepository,
        CustomerService customerService,
        CustomerRequestDto dto)
    {
        // arrange: garantir que o cenário falhe por cliente não encontrado
        dto.CustomerId = 1;
        dto.Phone = "48999999999";
        A.CallTo(() => customerRepository.GetByIdAsync(dto.CustomerId)).Returns(Task.FromResult<Customer>(null));

        // act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => customerService.UpdateAsync(dto));

        // assert
        Assert.Equal("Cliente não encontrado.", ex.Message);
        A.CallTo(() => customerRepository.GetByIdAsync(dto.CustomerId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => customerRepository.UpdateAsync(A<Customer>._)).MustNotHaveHappened(); // Verificar que o método Update do repository não foi chamado
    }
}
