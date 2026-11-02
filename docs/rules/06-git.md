# Rules: Git

- Agentes só podem executar comandos `git` quando o usuário pedir explicitamente.
- Antes de criar commits, mostre a separação proposta dos arquivos por commit.
- Escreva a descrição da mensagem em inglês, curta, no imperativo e em minúsculas, seguindo o padrão do histórico do projeto.
- Use prefixos convencionais como `docs:`, `fix:`, `add:`, `refactor:` ou `chore:` conforme a natureza da mudança.
- Exemplo: `fix: configure RabbitMQ and local Docker environment`.
- Aguarde confirmação antes de executar staging ou commit.
- Não execute comandos destrutivos ou de reescrita de histórico sem autorização explícita e específica.
- Comandos proibidos sem autorização explícita: `git reset --hard`, `git checkout --`, `git clean`, `git rebase`, `git push --force`.
- Preserve alterações existentes do usuário.
- Não reverta arquivos que você não alterou.
- Mantenha commits focados em uma única preocupação.
