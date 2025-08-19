# Prensadao

## Introdução

O Prensadão é um sistema de gestão de pedidos para uma hamburgueria, desenvolvido em ASP.NET Core, utilizando RabbitMQ para mensageria, PostgreSQL para persistência de dados e testes automatizados com xUnit, FakeItEasy e AutoFixture. O projeto segue boas práticas de arquitetura e organização de código, com foco em DDD e Clean Architecture.

## Tecnologias Utilizadas

 - ASP.NET Core – API REST
 - Entity Framework Core – Acesso a dados e Migrations
 - PostgreSQL – Banco de dados
 - RabbitMQ – Mensageria
 - xUnit – Testes unitários
 - FakeItEasy e AutoFixture – Mocks e geração de dados para testes
 - Docker/Docker Compose – Orquestração de serviços

## Migrações do EF Core

Gerar: <code>dotnet ef migrations add Initial --project ../../Prensadao.Infra.csproj --startup-project ../../../Prensadao/Prensadao.API.csproj</code>

Aplicar: <code>dotnet ef database update --project ../../Prensadao.Infra.csproj --startup-project ../../../Prensadao/Prensadao.API.csproj</code>

## Executando com Docker Compose

Na pasta raiz do projeto, execute:

<code>docker compose up -d --build</code>

## Estrutura do Projeto

 - API – Endpoints REST
 - Application – Serviços e regras de negócio
 - Domain – Entidades e agregados
 - Infrastructure – Persistência e integrações externas
 - Workers – Processamento assíncrono via RabbitMQ
 - Tests – Testes unitários e de integração
