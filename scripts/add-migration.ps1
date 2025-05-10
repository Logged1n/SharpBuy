param (
    [string]$MigrationName,
    [string]$StartupProject = "../src/Web.API",
    [string]$Project = "../src/Infrastructure",
    [string]$OutputDir = "Database/Migrations"
)

if (-not $MigrationName) {
    Write-Host "❌ Provide migration name as a parameter: -MigrationName"
    exit 1
}

dotnet ef migrations add $MigrationName `
    --startup-project $StartupProject `
    --project $Project `
    --output-dir $OutputDir
