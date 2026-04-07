# Site-motoca (Essenz Store)

Loja virtual em `.NET 10` com ASP.NET Core, Identity, SQLite e painel administrativo.

## Requisitos

- .NET SDK 10
- Git

## Configuração local

1. Clone o projeto
2. Copie as variáveis de ambiente:

```powershell
Copy-Item .env.example .env
```

3. Ajuste os valores do `.env` (SMTP, Mercado Pago etc.)
4. Rode o projeto:

```powershell
dotnet restore
dotnet run --project "loja de motocao/loja de motocao.csproj"
```

Acesse:
- Loja: `https://localhost:xxxx/`
- Admin: `https://localhost:xxxx/admin`

## Conta admin padrão (seed)

- E-mail: `admin@essenzstore.com.br`
- Senha: `Admin@123456`

> Troque a senha em produção.

## Segurança de segredos

- O arquivo `.env` está ignorado no `.gitignore`
- Nunca commitar credenciais reais
- Versionar somente `.env.example`

## Publicação (servidor VPS/IIS/Linux)

```powershell
dotnet publish "loja de motocao/loja de motocao.csproj" -c Release -o ./publish
```

Depois subir a pasta `publish/` no servidor e configurar variáveis de ambiente no host.

## Subir para o GitHub (repositório já criado)

```powershell
git init
git add .
git commit -m "chore: preparar projeto para deploy"
git branch -M main
git remote add origin https://github.com/Brunohora10/Site-motoca.git
git push -u origin main
```

Se o remoto já existir:

```powershell
git remote set-url origin https://github.com/Brunohora10/Site-motoca.git
git push -u origin main
```

## Checklist antes de enviar link ao cliente

- [ ] SMTP configurado
- [ ] Mercado Pago configurado
- [ ] HTTPS ativo no domínio
- [ ] Senha admin alterada
- [ ] Backup do banco (`.db`) configurado
- [ ] Teste de compra ponta-a-ponta concluído

## Demo rápido em hospedagem gratuita

Para mostrar para o cliente sem custo inicial, o caminho mais rápido é usar `Render` com o arquivo `render.yaml` já incluído no repositório.

### Como publicar no Render

1. Entrar em `https://render.com`
2. Conectar com o GitHub
3. Escolher `New +` → `Blueprint`
4. Selecionar o repositório `Site-motoca`
5. Confirmar o deploy

### Observação importante

O `render.yaml` atual usa `SQLite` em arquivo temporário (`/tmp/essenzstore.db`).
Isso serve para **demo/apresentação**, mas **não é ideal para produção**, porque os dados podem ser perdidos após reinício da instância.

Para produção real, usar:
- `Azure App Service + Azure SQL`, ou
- `Render + PostgreSQL`
