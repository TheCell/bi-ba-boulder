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

# dotnet typescript generator 
To run the dotnet typescript generator run the backend with the http profile. Otherwise the redirect to https will cause the generator to fail.
```bash
dotnet run --launch-profile http
```
