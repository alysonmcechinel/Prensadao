# Roadmap e Pendências

Este documento reúne trabalho ainda não concluído. Itens daqui não devem ser descritos como funcionalidades prontas em `FEATURES.md`.

## Tratamento de Erros

- Seguir a proposta e as decisões pendentes registradas em `ERROR_HANDLING.md`.
- Decidir e aplicar o tratamento global de exceções: `RequestContextMiddleware` existe, mas está comentado em `Prensadao/Program.cs`.
- Remover a captura genérica de `Exception` em `PromotionsController` quando o tratamento global estiver ativo, preservando respostas `ProblemDetails` coerentes.

## Promoções com IA

- Adicionar testes para validação e falhas de `AzureOpenAiPromotionService`, sem acessar o serviço real.
- Cobrir o mapeamento de `AiResponseValidationException` no tratamento HTTP escolhido.

## Evoluções da Integração Contínua

- Corrigir os warnings existentes antes de promover warnings de compilação a erros bloqueantes.
- Criar testes de integração e smoke test da API com PostgreSQL e RabbitMQ efêmeros, sem credenciais reais.
- Avaliar revisão automatizada de pull requests por IA em uma iniciativa separada, definindo antes provedor, retenção de dados, conteúdo enviado, custo, permissões e comportamento para forks.

## Critério de Manutenção

Ao concluir um item, remova-o deste arquivo e atualize a documentação do comportamento implementado. Novas pendências devem indicar o resultado esperado, sem antecipar que a funcionalidade já existe.
