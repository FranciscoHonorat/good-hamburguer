# Contribuindo

## Estratégia de branches
- `main`: branch estável de release.
- `feature/<escopo-curto>`: desenvolvimento de funcionalidade.
- `fix/<escopo-curto>`: correções.
- `chore/<escopo-curto>`: manutenção.

## Fluxo recomendado (nível profissional)
1. Atualize referências remotas:
   - `git fetch --all --prune --tags`
2. Rebase da branch base atualizada:
   - `git checkout main`
   - `git pull --rebase origin main`
3. Crie branch de trabalho:
   - `git checkout -b feature/nome-curto`
4. Commits semânticos (Conventional Commits):
   - `feat: ...`
   - `fix: ...`
   - `refactor: ...`
   - `docs: ...`
   - `chore: ...`
5. Antes de abrir PR:
   - `dotnet build`
   - `git fetch --all --prune --tags`
   - `git rebase origin/main`
6. Push com upstream:
   - `git push -u origin feature/nome-curto`

## Convenções de commit
Formato:
`<tipo>(<escopo-opcional>): <resumo no imperativo>`

Exemplos:
- `feat(pedidos): destacar subtotal e total na seleção`
- `fix(security): validar api key com comparação em tempo constante`
- `docs(git): adicionar fluxo com fetch/rebase/tags`

## Releases e versionamento
- SemVer: `MAJOR.MINOR.PATCH`
- Tag anotada obrigatória para release:
  - `git tag -a v0.1.0 -m "release: v0.1.0"`
  - `git push origin v0.1.0`
- Atualize `CHANGELOG.md` a cada release.

## Qualidade mínima
- Build passando.
- Sem segredos versionados.
- `README.md` e `CHANGELOG.md` atualizados quando aplicável.
