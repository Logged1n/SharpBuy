param (
    [string]$MigrationName = "Initial",
    [string]$StartupProject = "../src/Web.API",
    [string]$Project = "../src/Infrastructure",
    [string]$OutputDir = "Database/Migrations"
)

dotnet ef migrations add $MigrationName `
    --startup-project $StartupProject `
    --project $Project `
    --output-dir $OutputDir
