<#
.SYNOPSIS
    Gera uma migration usando EF Core CLI e adiciona data/hora no nome.

.DESCRIPTION
    Script para simplificar a criação de migrations com o nome prefixado pela data/hora
    (formato yyyyMMddHHmmss). Usa `dotnet ef migrations add` apontando para o projeto
    de infraestrutura (onde as migrations ficam) e para o projeto de startup (API).

.PARAMETER Name
    Descrição curta da migration (ex: AddGameTable). Opcional. Se não for fornecido,
    o script usará um nome padrão baseado apenas no timestamp (ex: 20251202_153045_Migration).

.PARAMETER Project
    Caminho para o arquivo .csproj do projeto que contém o DbContext.

.PARAMETER StartupProject
    Caminho para o arquivo .csproj do projeto de startup (usualmente a API).

.PARAMETER OutputDir
    Diretório de saída (relativo ao projeto da migration) para colocar as migrations.

.PARAMETER WhatIf
    Exibe o comando que seria executado sem executá-lo.

.EXAMPLE
    .\generate-migration.ps1 -Name AddPromotionTable

.EXAMPLE
    .\generate-migration.ps1 -Name AddPromotionTable -Project src/FiapCloudGames.Infrastructure/FiapCloudGames.Infrastructure.csproj -StartupProject src/FiapCloudGames.Api/FiapCloudGames.Api.csproj -OutputDir Migrations
#>

param(
    [Parameter(Mandatory=$false)][string]$Name,
    [Parameter(Mandatory=$false)][string]$Project = "src/FiapCloudGames.Infrastructure/FiapCloudGames.Users.Infrastructure.csproj",
    [Parameter(Mandatory=$false)][string]$StartupProject = "src/FiapCloudGames.Api/FiapCloudGames.Users.Api.csproj",
    [Parameter(Mandatory=$false)][string]$OutputDir = "Migrations",
    [switch]$WhatIf
)

# Resolve caminhos relativos em relação à raiz do repositório (pasta pai de `others`)
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
try {
    $RepoRoot = (Resolve-Path (Join-Path $ScriptDir '..')).Path
} catch {
    $RepoRoot = $ScriptDir
}

# Se os caminhos padrões não forem absolutos, tente convertê-los para caminhos absolutos
if (-not [System.IO.Path]::IsPathRooted($Project)) {
    $projectCandidate = Join-Path $RepoRoot $Project
    if (Test-Path $projectCandidate) {
        $Project = (Resolve-Path $projectCandidate).Path
    } else {
        Write-Host "Project file não encontrado: $projectCandidate" -ForegroundColor Red
        exit 4
    }
}

if (-not [System.IO.Path]::IsPathRooted($StartupProject)) {
    $startupCandidate = Join-Path $RepoRoot $StartupProject
    if (Test-Path $startupCandidate) {
        $StartupProject = (Resolve-Path $startupCandidate).Path
    } else {
        Write-Host "Startup project não encontrado: $startupCandidate" -ForegroundColor Red
        exit 5
    }
}

function Show-Usage {
    Write-Host "Uso:" -ForegroundColor Cyan
    Write-Host "  .\\generate-migration.ps1 [-Name AddGameTable]" -ForegroundColor Yellow
    Write-Host "Opções:" -ForegroundColor Cyan
    Write-Host "  -Name (opcional)       : Descrição curta da migration (ex: AddGameTable). Se ausente, usa timestamp." -ForegroundColor Yellow
    Write-Host "  -Project               : Caminho para o projeto onde ficarão as migrations (padrão: $Project)" -ForegroundColor Yellow
    Write-Host "  -StartupProject        : Projeto de startup (padrão: $StartupProject)" -ForegroundColor Yellow
    Write-Host "  -OutputDir             : Diretório de saída para as migrations (padrão: Migrations)" -ForegroundColor Yellow
    Write-Host "  -WhatIf                : Mostra o comando sem executá-lo" -ForegroundColor Yellow
}

# Se nenhum nome for passado, usamos nome padrão baseado apenas no timestamp
if (-not $Name) {
    Write-Host "Nenhum nome informado - usando nome padrão com timestamp." -ForegroundColor Yellow
    $Name = "Migration"
}

# Verifica se dotnet está disponível
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Host "O comando 'dotnet' não foi encontrado. Instale o .NET SDK e certifique-se de que 'dotnet' está no PATH." -ForegroundColor Red
    exit 2
}

# Gera timestamp e sanitiza o nome informado
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$clean = $Name -replace '\s+', '_' -replace '[^0-9A-Za-z_]', ''
$migrationName = "${timestamp}_${clean}"

Write-Host "Gerando migration: $migrationName" -ForegroundColor Green
Write-Host "Project: $Project" -ForegroundColor DarkCyan
Write-Host "StartupProject: $StartupProject" -ForegroundColor DarkCyan
Write-Host "OutputDir: $OutputDir" -ForegroundColor DarkCyan

if ($WhatIf) {
    $cmd = "dotnet ef migrations add `"$migrationName`" --project `"$Project`" --startup-project `"$StartupProject`" --output-dir `"$OutputDir`""
    Write-Host "Comando (what-if):" -ForegroundColor Yellow
    Write-Host $cmd -ForegroundColor Gray
    exit 0
}

try {
    $args = @('ef', 'migrations', 'add', $migrationName, '--project', $Project, '--startup-project', $StartupProject, '--output-dir', $OutputDir)
    $proc = Start-Process -FilePath "dotnet" -ArgumentList $args -NoNewWindow -Wait -PassThru
    if ($proc.ExitCode -ne 0) {
        Write-Host "dotnet ef retornou código $($proc.ExitCode)." -ForegroundColor Red
        exit $proc.ExitCode
    }
    Write-Host "Migration criada com sucesso: $migrationName" -ForegroundColor Green
} catch {
    Write-Host ("Erro ao executar dotnet ef: {0}" -f $_) -ForegroundColor Red
    exit 3
}
