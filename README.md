# Orcking

MVP em ASP.NET Core MVC/Razor Pages para aplicacao de provas com modelos aleatorios, marca d'agua individual, salvamento de respostas e auditoria antifraude.

## Rodar sem instalar Postgres

Em ambiente `Development`, o arquivo `appsettings.Development.json` usa banco em memoria. Isso permite testar o fluxo do MVP sem instalar Postgres.

```powershell
dotnet run
```

## Rodar com Postgres

Instale Postgres localmente ou use Docker quando disponivel:

```powershell
docker compose up -d
```

Depois altere `UseInMemoryDatabase` para `false` em `appsettings.Development.json`, ou rode com ambiente de producao usando a connection string em `appsettings.json`.

```json
"DefaultConnection": "Host=localhost;Port=5432;Database=orcking;Username=postgres;Password=postgres"
```

O projeto usa `EnsureCreated` no MVP, entao cria as tabelas automaticamente ao iniciar. Em uma proxima etapa, o ideal e trocar para migrations.

## Perfis de teste

- Professor: `professor@orcking.local`
- Aluno: `aluno@orcking.local`

O login atual e demonstrativo por selecao de perfil. A proxima evolucao natural e substituir por ASP.NET Identity.

## Importacao CSV

O professor pode importar questoes com separador ponto e virgula:

```text
Enunciado;A;B;C;D;E;Correta;Peso;Tema;Dificuldade
```

Exemplo:

```text
Quanto e 2+2?;4;3;5;6;8;A;1;Matematica;Facil
```
