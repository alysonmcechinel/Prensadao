using AutoFixture.Xunit2;
using FakeItEasy;
using Prensadao.Application.Models.Request;
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
        Assert.Equal("Numero de telefone invalido.", ex.Message);

        // Não deve tentar inserir
        A.CallTo(() => customerRepository.AddCustomer(A<Customer>._)).MustNotHaveHappened();
        // Verifica que a consulta aconteceu
        A.CallTo(() => customerRepository.PhoneIsExists(dto.Phone)).MustHaveHappenedOnceExactly();
    }

    // 3) SUCESSO: retorna o ID e chama AddCustomer exatamente 1x
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
}
