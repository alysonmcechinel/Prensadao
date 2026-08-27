# Rules: Git

- Agentes só podem executar comandos `git` quando o usuário pedir explicitamente.
- Antes de criar commits, mostre a separação proposta dos arquivos por commit.
- Sugira mensagens curtas no imperativo, com prefixos como `docs:`, `fix:`, `add:` ou `refactor:` quando fizer sentido.
- Aguarde confirmação antes de executar staging ou commit.
- Não execute comandos destrutivos ou de reescrita de histórico sem autorização explícita e específica.
- Comandos proibidos sem autorização explícita: `git reset --hard`, `git checkout --`, `git clean`, `git rebase`, `git push --force`.
- Preserve alterações existentes do usuário.
- Não reverta arquivos que você não alterou.
- Mantenha commits focados em uma única preocupação.
