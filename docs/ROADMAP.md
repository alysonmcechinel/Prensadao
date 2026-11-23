# Roadmap e Pendências

Este documento reúne trabalho ainda não concluído. Itens daqui não devem ser descritos como funcionalidades prontas em `FEATURES.md`.

## Diagnóstico de Desempenho

- Reproduzir as falhas de transação sob carga e identificar sua causa antes de ajustar persistência ou concorrência. O handler global registra a exceção original com o identificador da requisição.
- Avaliar coleta automática de logs do ambiente k6 e instrumentação com `ActivitySource`/OpenTelemetry em etapas separadas, conforme a necessidade do diagnóstico.

## Promoções com IA

- Adicionar testes para validação e falhas de `AzureOpenAiPromotionService`, sem acessar o serviço real.

## Evoluções da Integração Contínua

- Corrigir os warnings existentes antes de promover warnings de compilação a erros bloqueantes.
- Criar testes de integração e smoke test da API com PostgreSQL e RabbitMQ efêmeros, sem credenciais reais.
- Avaliar revisão automatizada de pull requests por IA em uma iniciativa separada, definindo antes provedor, retenção de dados, conteúdo enviado, custo, permissões e comportamento para forks.

## Critério de Manutenção

Ao concluir um item, remova-o deste arquivo e atualize a documentação do comportamento implementado. Novas pendências devem indicar o resultado esperado, sem antecipar que a funcionalidade já existe.
