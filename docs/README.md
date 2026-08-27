# Documentação do Prensadão

Este diretório concentra a documentação técnica do projeto e as regras usadas por humanos, Codex e Claude para manter o sistema com segurança.

## Índice
- `ARCHITECTURE.md`: camadas, dependências, módulos de DI e limites de responsabilidade.
- `FEATURES.md`: funcionalidades existentes e comportamentos principais.
- `AI_PROMOTIONS.md`: fluxo de promoções com Azure OpenAI.
- `RUNBOOK.md`: comandos locais, Docker, migrations, testes e configuração.
- `rules/00-principios.md`: SRP, DRY, SOLID pragmático e comentários didáticos.
- `rules/`: guardrails por área.

## Como Usar
- Para entender o sistema, leia `ARCHITECTURE.md` e `FEATURES.md`.
- Para rodar ou diagnosticar localmente, leia `RUNBOOK.md`.
- Para mexer em IA, leia `AI_PROMOTIONS.md` e `rules/05-ia-promocoes.md`.
- Antes de qualquer mudança, leia `rules/00-principios.md`.
- Para orientar agentes, mantenha `AGENTS.md`, `CLAUDE.md` e `rules/` sincronizados.

## Pendências Conhecidas
- O middleware `RequestContextMiddleware` existe, mas está comentado em `Prensadao/Program.cs`.
- Ainda não há testes específicos para `PromotionGeneratorService`, `AzureOpenAiPromotionService` ou `AiResponseValidationException`.
