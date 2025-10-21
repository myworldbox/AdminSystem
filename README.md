# AdminSystem
Mock at TWGH

## PostgreSQL Migration
```
dotnet ef migrations add InitialCreate --project AdminSystem --startup-project AdminSystem

dotnet ef database update --project AdminSystem --startup-project AdminSystem
```