# Documentação do Prensadão

Este diretório concentra a documentação técnica e operacional do projeto. `AGENTS.md`, na raiz, é o ponto de entrada para agentes; este arquivo é o índice canônico do conteúdo em `docs/`.

## Organização

| Documento | Responsabilidade | Não deve conter |
| --- | --- | --- |
| `ARCHITECTURE.md` | Estado atual das camadas, dependências e limites | Passos operacionais ou planos futuros |
| `FEATURES.md` | Funcionalidades já implementadas | Funcionalidades apenas planejadas |
| `GLOSSARY.md` | Significado canônico dos termos de negócio | Regras técnicas ou detalhes de implementação sem impacto semântico |
| `AI_PROMOTIONS.md` | Contrato e fluxo detalhado da promoção com IA | Regras gerais de arquitetura |
| `RUNBOOK.md` | Execução, configuração, migrations e diagnóstico | Decisões de negócio |
| `ROADMAP.md` | Pendências, débitos conhecidos e decisões futuras | Comportamento apresentado como concluído |
| `rules/` | Regras prescritivas para mudanças | Inventário de funcionalidades |

## Rotas de Leitura

- Visão geral do sistema: `ARCHITECTURE.md` e `FEATURES.md`.
- Regra de negócio, contrato, DTO ou nomenclatura: `GLOSSARY.md`.
- Execução ou diagnóstico local: `RUNBOOK.md`.
- Mudança em arquitetura: `ARCHITECTURE.md`, `rules/00-principios.md` e `rules/01-arquitetura.md`.
- Mudança em persistência: `rules/00-principios.md` e `rules/02-persistencia.md`.
- Mudança em testes: `rules/00-principios.md` e `rules/03-testes.md`.
- Mudança em mensageria ou workers: `rules/00-principios.md` e `rules/04-mensageria.md`.
- Mudança em promoções com IA: `AI_PROMOTIONS.md`, `rules/00-principios.md` e `rules/05-ia-promocoes.md`.
- Operação Git solicitada pelo usuário: `rules/06-git.md`.
- Planejamento e débitos conhecidos: `ROADMAP.md`.

O índice completo de regras e sua aplicabilidade está em `rules/README.md`.

## Manutenção

- Atualize a documentação no mesmo trabalho em que um contrato, fluxo ou comando mudar.
- Registre estado atual em `ARCHITECTURE.md` ou `FEATURES.md` e trabalho futuro em `ROADMAP.md`.
- Evite repetir a mesma orientação em vários arquivos; escolha o documento responsável e crie referências nos demais.
- Use caminhos relativos à raiz do repositório e preserve os nomes reais de tipos, projetos e comandos.
- Quando documentação e código divergirem, valide o código e corrija a documentação ou a implementação conforme o objetivo da tarefa.
