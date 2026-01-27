This project is porting the symfony project over to .net

# Setup
```bash
dotnet restore
dotnet tool restore

// if you have issues with nuget packages
dotnet nuget locals all --clear

```

# Migrations

```bash
cd ../Migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```
