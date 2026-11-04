# Fundação de CI e revisão de pull requests

> Este documento registra as decisões confirmadas para a primeira etapa de integração contínua do projeto.
> Estado: confirmado pelo usuário e em implementação desde 03/11/2026.

## Situação

- O Prensadão é um projeto público de portfólio, desenvolvido por um único mantenedor e ainda sem ambiente publicado.
- A documentação técnica e as regras de manutenção já existem, mas `.github/workflows/` não possuía uma validação contínua.
- A decisão confirmada foi criar primeiro uma fundação pública e determinística. Smoke tests e revisão interpretativa por IA serão evoluções separadas.
- O baseline local possui 104 testes aprovados e warnings de compilação preexistentes.

## Problema e sucesso

Hoje um pull request não apresenta evidência automática de que a solução compila e os testes continuam aprovados. Isso reduz a segurança das mudanças e não demonstra, no histórico público, um processo consistente de engenharia.

A primeira etapa funcionou quando:

- todo pull request para `master` executar o mesmo build e conjunto de testes;
- uma falha no check impedir o merge;
- o contexto, a validação, os riscos e a documentação da mudança forem registrados no PR;
- nenhuma credencial ou integração externa for necessária para executar a validação.

O resultado será revisado depois dos três primeiros pull requests reais integrados com esse fluxo.

## Limites

Incluído:

- workflow de build e testes no GitHub Actions;
- template de pull request;
- check obrigatório e proteção da branch `master`;
- documentação operacional e badge público do CI;
- execução segura de contribuições externas somente após aprovação do mantenedor.

Fora do escopo:

- subir API, PostgreSQL, RabbitMQ, Hangfire ou Azure OpenAI no workflow;
- bloquear warnings ou impor cobertura mínima;
- publicar ou implantar a aplicação;
- enviar diffs para um provedor de IA;
- exigir aprovação humana enquanto houver apenas um mantenedor.

## Desenho escolhido

O repositório terá um único workflow `CI`, executado em pull requests para `master` e depois de merges nessa branch. O job `build-and-test` restaurará dependências, compilará a solução em `Release` e executará `Prensadao.Test` em um runner Linux.

O workflow usará permissões somente de leitura, nenhuma credencial e apenas ações oficiais fixadas por SHA completo. Um ruleset de repositório exigirá pull request, check atualizado e conversas resolvidas, sem bypass do proprietário.

A alternativa mais completa adicionaria serviços efêmeros, smoke tests, cobertura bloqueante e revisão por IA. Ela somente passa a ser adequada depois que a fundação estiver estável e quando houver um problema observado que os testes unitários não detectem.

## Decisões principais

1. **O check público será `CI / build-and-test`.** Esse nome será o contrato usado pelo ruleset de `master`.
2. **A execução usará .NET 8 em `ubuntu-latest`.** Isso mantém o workflow próximo do ambiente Linux usado pelo container e detecta problemas de portabilidade.
3. **Warnings permanecerão informativos.** Eles só se tornarão bloqueantes após a correção do passivo atual.
4. **O evento privilegiado `pull_request_target` não será usado.** Pull requests externos não receberão secrets nem permissão de escrita e dependerão de aprovação manual para iniciar o workflow.
5. **A primeira execução bem-sucedida precede a ativação do ruleset.** O GitHub precisa conhecer o check antes que ele possa ser selecionado como obrigatório.
6. **A revisão por IA é uma iniciativa posterior.** Provedor, retenção, conteúdo enviado, custo, permissões e forks deverão ser decididos antes dessa integração.

## Recortes da implementação

| Recorte | Entrega | Estado |
| --- | --- | --- |
| Workflow de CI | Restore, build e 104 testes em cada PR para `master`. | Em implementação |
| Contrato de PR | Template com contexto, validação, impactos, riscos e documentação. | Em implementação |
| Proteção de `master` | Ruleset sem bypass e check obrigatório. | Aguarda primeira execução no GitHub |
| Evidência pública | Badge e instruções operacionais no repositório. | Em implementação |

Ordem: Workflow de CI → Contrato de PR → primeira execução no GitHub → Proteção de `master` → validação do bloqueio.

## Validação

- Rodar `dotnet build Prensadao.sln` e `dotnet test Prensadao.Test/Prensadao.Test.csproj` localmente.
- Confirmar no primeiro PR que `CI / build-and-test` termina com sucesso sem secrets ou infraestrutura.
- Após ativar o ruleset, comprovar em um commit temporário que um teste falhando bloqueia o merge e que a correção libera o check.
- Confirmar que push direto, force push e exclusão de `master` estão bloqueados.
- Confirmar que um workflow originado de fork externo aguarda aprovação manual.

## Fontes

- `docs/RUNBOOK.md`: comandos oficiais e configuração local.
- `docs/rules/03-testes.md`: limites e convenções dos testes automatizados.
- [GitHub Actions para .NET](https://docs.github.com/en/actions/tutorials/build-and-test-code/net): fluxo oficial de restore, build e testes.
- [Uso seguro do GitHub Actions](https://docs.github.com/en/actions/reference/security/secure-use): permissões mínimas e ações fixadas por SHA.
- [Rulesets de repositório](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets): proteção de branches e checks obrigatórios.
- [Aprovação de workflows de forks](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/approve-runs-from-forks): execução controlada de contribuições externas.
