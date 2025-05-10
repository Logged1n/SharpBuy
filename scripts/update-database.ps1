param (
    [string]$StartupProject = "../src/Web.API",
    [string]$Project = "../src/Infrastructure"
)

dotnet ef database update `
    --startup-project $StartupProject `
    --project $Project
