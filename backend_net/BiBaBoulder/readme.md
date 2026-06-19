This is the .NET 10 backend for Bi Ba Boulder using EF Core and CQRS pattern.

# Setup
```bash
dotnet restore
dotnet tool restore

// if you have issues with nuget packages
dotnet nuget locals all --clear

## Certificates
1. Install/Trust: Run mkcert -install (only needs to be done once).
2. Generate Certs: Run mkcert localhost in your project folder to get .pem files.

### 1
// if choco is not installed yet install it via Powershell
Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
// install mkcert first
choco install mkcert
mkcert -install

### 2
cd C:\dev\git\bi-ba-boulder\ssl
mkcert localhost
```

# Migrations

```bash
cd ../Migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

# dotnet typescript generator 
To run the dotnet typescript generator run the backend with the http profile. Otherwise the redirect to https will cause the generator to fail.
```bash
dotnet run --launch-profile http
```
