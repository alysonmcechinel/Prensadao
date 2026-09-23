# Índice de Regras

Leia sempre `00-principios.md`. Depois, selecione as regras conforme os arquivos ou comportamentos afetados.

| Mudança | Regra aplicável |
| --- | --- |
| Camadas, controllers, services, DI ou dependências entre projetos | `01-arquitetura.md` |
| EF Core, PostgreSQL, entidades persistidas, repositórios ou migrations | `02-persistencia.md` |
| Código de produção que exige cobertura ou qualquer arquivo em `Prensadao.Test/` | `03-testes.md` |
| RabbitMQ, contratos de mensagem, publishers, consumers ou workers | `04-mensageria.md` |
| Prompt, Azure OpenAI, geração ou validação de promoções | `05-ia-promocoes.md` |
| Qualquer comando ou operação Git solicitada pelo usuário | `06-git.md` |

Mais de uma regra pode se aplicar à mesma mudança. Em caso de conflito, preserve primeiro segurança, direção de dependências e comportamento público existente, e confirme decisões que alterem o escopo pedido.
