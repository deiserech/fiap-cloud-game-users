<#
.SYNOPSIS
    Aplica migrations usando EF Core CLI (dotnet ef database update).

.DESCRIPTION
    Este script executa `dotnet ef database update` apontando para o projeto que contém o
    DbContext e para o projeto de startup. Pode aplicar uma migration específica ou a mais
    recente se nenhum nome for informado.

.PARAMETER Migration
    Nome da migration a aplicar. Opcional - se ausente, aplica a migration mais recente.

.PARAMETER Project
    Caminho para o arquivo .csproj do projeto que contém o DbContext.

.PARAMETER StartupProject
    Caminho para o arquivo .csproj do projeto de startup (API).

.PARAMETER WhatIf
    Exibe o comando que seria executado sem executá-lo.

.EXAMPLE
    .\apply-migration.ps1 -Migration 20251202_153045_Migration

.EXAMPLE
    .\apply-migration.ps1
#>

param(
    [Parameter(Mandatory=$false)][string]$Migration,
    [Parameter(Mandatory=$false)][string]$Project = "src/FiapCloudGames.Infrastructure/FiapCloudGames.Users.Infrastructure.csproj",
    [Parameter(Mandatory=$false)][string]$StartupProject = "src/FiapCloudGames.Api/FiapCloudGames.Users.Api.csproj",
    [switch]$WhatIf
)

# Resolve repo root (pasta pai de `others`)
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
try {
    $RepoRoot = (Resolve-Path (Join-Path $ScriptDir '..')).Path
} catch {
    $RepoRoot = $ScriptDir
}

# Resolve e valida arquivos de projeto
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

# Verifica se dotnet está disponível
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Host "O comando 'dotnet' não foi encontrado. Instale o .NET SDK e certifique-se de que 'dotnet' está no PATH." -ForegroundColor Red
    exit 2
}

Write-Host "Aplicando migration" -ForegroundColor Green

# Se nenhuma migration foi passada, obter a lista de migrations e escolher a mais recente
if (-not $Migration) {
    Write-Host "Nenhuma migration informada - buscando a migration mais recente..." -ForegroundColor Yellow
    $listArgs = @('ef','migrations','list','--project',$Project,'--startup-project',$StartupProject)
    try {
        $output = & dotnet @listArgs 2>&1
    } catch {
        Write-Host ("Erro ao listar migrations: {0}" -f $_) -ForegroundColor Red
        exit 6
    }

    # Filtra linhas vazias e mensagens de log do dotnet, pega a última linha com nome
    $lines = $output -split "\r?\n" | ForEach-Object { $_.Trim() } | Where-Object { $_ -and ($_ -notmatch '^\s*info:') -and ($_ -notmatch '^\s*warn:') }
    if (-not $lines -or $lines.Count -eq 0) {
        Write-Host "Nenhuma migration encontrada." -ForegroundColor Red
        exit 7
    }

    $latest = $lines | Select-Object -Last 1
    if ($latest -match 'No migrations were found') {
        Write-Host "Nenhuma migration encontrada." -ForegroundColor Red
        exit 7
    }

    $Migration = $latest
    Write-Host "Migration selecionada: $Migration" -ForegroundColor DarkCyan
}
else {
    Write-Host "Migration: $Migration" -ForegroundColor DarkCyan
}
Write-Host "Project: $Project" -ForegroundColor DarkCyan
Write-Host "StartupProject: $StartupProject" -ForegroundColor DarkCyan

if ($WhatIf) {
    if ($Migration) {
        $cmd = "dotnet ef database update `"$Migration`" --project `"$Project`" --startup-project `"$StartupProject`""
    } else {
        $cmd = "dotnet ef database update --project `"$Project`" --startup-project `"$StartupProject`""
    }
    Write-Host "Comando (what-if):" -ForegroundColor Yellow
    Write-Host $cmd -ForegroundColor Gray
    exit 0
}

try {
    $args = @('ef', 'database', 'update')
    if ($Migration) { $args += $Migration }
    $args += @('--project', $Project, '--startup-project', $StartupProject)

    $proc = Start-Process -FilePath "dotnet" -ArgumentList $args -NoNewWindow -Wait -PassThru
    if ($proc.ExitCode -ne 0) {
        Write-Host "dotnet ef retornou código $($proc.ExitCode)." -ForegroundColor Red
        exit $proc.ExitCode
    }
    Write-Host "Migration aplicada com sucesso." -ForegroundColor Green
} catch {
    Write-Host ("Erro ao executar dotnet ef: {0}" -f $_) -ForegroundColor Red
    exit 3
}
