# PostgreSQL Setup Guide (Fedora Linux + WSL Alternative)

This guide shows how to set up PostgreSQL for this project.

The primary instructions are for Fedora Linux.
A separate WSL alternative is included near the end.

It covers:
- Installing PostgreSQL
- Starting/enabling the service
- Setting user password
- Creating databases
- Fixing local password authentication
- Verifying connectivity

## Primary Setup: Fedora Linux

## 1. Install PostgreSQL

```bash
sudo dnf install -y postgresql-server postgresql-contrib
```

Initialize the database cluster (first time only):

```bash
sudo postgresql-setup --initdb --unit postgresql
```

## 2. Start and enable PostgreSQL

```bash
sudo systemctl enable --now postgresql
sudo systemctl status postgresql
```

Health check:

```bash
pg_isready
```

Expected output includes `accepting connections`.

## 3. Set password for `postgres` user

```bash
sudo -u postgres psql -c "ALTER USER postgres WITH PASSWORD 'postgres';"
```

## 4. Create application databases

Create the databases used in this solution:

```bash
sudo -u postgres createdb SimpleApi
sudo -u postgres createdb appdb
```

If they already exist, PostgreSQL will report it and you can ignore that.

## 5. Ensure localhost uses password auth (important)

Some installations default to `ident`/`peer`, which can reject password login.

Find your `pg_hba.conf` path:

```bash
sudo -u postgres psql -tAc "SHOW hba_file"
```

Edit that file and make sure these lines use `scram-sha-256`:

```conf
local   all             all                                     scram-sha-256
host    all             all             127.0.0.1/32            scram-sha-256
host    all             all             ::1/128                 scram-sha-256
```

Restart PostgreSQL:

```bash
sudo systemctl restart postgresql
```

## 6. Verify connection

```bash
PGPASSWORD=postgres psql -h localhost -U postgres -d SimpleApi -c "SELECT current_database(), current_user;"
```

Expected output shows:
- `current_database = SimpleApi`
- `current_user = postgres`

## 7. Use this connection string in the API

Use in `src/SimpleApi.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=SimpleApi;Username=postgres;Password=postgres"
}
```

## 8. Apply EF Core migrations

From repository root:

```bash
dotnet ef migrations add InitialCreate -p src/SimpleApi.Application/SimpleApi.Application.csproj -s src/SimpleApi.Api/SimpleApi.Api.csproj
dotnet ef database update -p src/SimpleApi.Application/SimpleApi.Application.csproj -s src/SimpleApi.Api/SimpleApi.Api.csproj
```

## 9. Run API + tests

```bash
dotnet run --project src/SimpleApi.Api/SimpleApi.Api.csproj
dotnet test
```

Swagger URL (development):

```text
http://localhost:5000/swagger
https://localhost:5001/swagger
```

## Troubleshooting

- `FATAL: Ident authentication failed`:
  - Update `pg_hba.conf` to `scram-sha-256` for localhost and restart PostgreSQL.
- `systemctl` not working in WSL:
  - Ensure your WSL distro has systemd enabled.
- Port conflict on 5432:
  - Check running services and update `Port=` in `postgresql.conf` if needed.

## Alternative Setup: WSL (Fedora)

Use this if you are running Fedora inside WSL instead of native Linux.

## A1. Open Fedora WSL shell

From Windows PowerShell:

```powershell
wsl
```

If you have multiple distros and need Fedora specifically:

```powershell
wsl -d FedoraLinux-43
```

## A2. Ensure systemd is enabled in WSL

`systemctl` requires systemd in WSL.

Check:

```bash
systemctl status postgresql
```

If systemd is not available, add `/etc/wsl.conf`:

```ini
[boot]
systemd=true
```

Then from Windows PowerShell:

```powershell
wsl --shutdown
wsl
```

## A3. Run the same Fedora setup steps

After entering WSL and confirming systemd works, follow the Fedora primary steps above:
- Install PostgreSQL (`dnf install`)
- Initialize DB cluster (`postgresql-setup --initdb`)
- Enable/start service (`systemctl enable --now postgresql`)
- Set password, create DBs, verify connection

## A4. Validate connectivity from WSL

```bash
PGPASSWORD=postgres psql -h localhost -U postgres -d SimpleApi -c "SELECT current_database(), current_user;"
```

## A5. Use from this project

From the repo root in Windows or WSL, run:

```bash
dotnet ef database update -p src/SimpleApi.Application/SimpleApi.Application.csproj -s src/SimpleApi.Api/SimpleApi.Api.csproj
dotnet test
```
