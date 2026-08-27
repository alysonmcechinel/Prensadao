# Instruções para Claude

Este projeto usa as mesmas regras de manutenção documentadas para Codex.

Antes de alterar código:

1. Leia `AGENTS.md`.
2. Leia `docs/README.md`.
3. Leia `docs/rules/00-principios.md`.
4. Leia as rules relevantes em `docs/rules/`.
5. Leia os arquivos próximos ao ponto de mudança.

Regras essenciais:

- Preserve a arquitetura em camadas.
- Use SRP como princípio principal.
- Não coloque regra de negócio em controllers.
- Não acesse banco, RabbitMQ, Hangfire ou Azure OpenAI diretamente na API.
- Não registre segredos.
- Não edite migrations antigas sem pedido explícito.
- Não use comandos `git` sem pedido explícito do usuário.
- Ao alterar código, valide com `dotnet build Prensadao.sln`; para regra de negócio, contratos, infra, mensageria ou IA, rode também os testes relevantes.
