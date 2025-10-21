# AdminSystem
Mock at TWGH

## EF Core Code‑First
*.Net Entities ---> Database*
```
dotnet ef migrations add InitialCreate --project AdminSystem --startup-project AdminSystem -o Temp/Migrations
dotnet ef database update --project AdminSystem --startup-project AdminSystem
```

## EF Core Database‑First
*Database ---> .Net Entities*
```
Scaffold-DbContext "Name=MSSQL.CustomerEntities" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Temp/Application/Entities -ContextDir Temp/Infrastructure/Data -Context CustomerEntities -Force -DataAnnotations
```