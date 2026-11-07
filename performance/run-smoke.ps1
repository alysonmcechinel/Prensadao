param(
    [ValidateRange(10, 600)]
    [int]$ReadinessTimeoutSeconds = 120
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$composeArguments = @(
    '-p', 'prensadao-performance',
    '-f', (Join-Path $repositoryRoot 'docker-compose.yml'),
    '-f', (Join-Path $repositoryRoot 'docker-compose.performance.yml')
)
$readinessUrl = 'http://localhost:18080/health/ready'
$resultsDirectory = Join-Path $PSScriptRoot 'results'

function Assert-DockerEngineIsAvailable {
    $previousErrorActionPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $dockerInformation = & docker version --format '{{.Server.Version}}' 2>&1
        $dockerExitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    if ($dockerExitCode -ne 0) {
        throw "Docker Engine is not available. Start Docker Desktop before running the smoke test. Details: $dockerInformation"
    }
}

function Invoke-PerformanceCompose {
    param(
        [Parameter(Mandatory)]
        [string[]]$Arguments
    )

    & docker compose @composeArguments @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "docker compose failed with exit code $LASTEXITCODE."
    }
}

function Wait-PerformanceApi {
    $deadline = (Get-Date).AddSeconds($ReadinessTimeoutSeconds)

    while ((Get-Date) -lt $deadline) {
        try {
            $response = Invoke-WebRequest -Uri $readinessUrl -UseBasicParsing -TimeoutSec 3
            if ($response.StatusCode -eq 200) {
                return
            }
        }
        catch {
            Start-Sleep -Seconds 2
        }
    }

    throw "The performance API did not become ready within $ReadinessTimeoutSeconds seconds."
}

Assert-DockerEngineIsAvailable
Push-Location $repositoryRoot
try {
    New-Item -ItemType Directory -Path $resultsDirectory -Force | Out-Null

    Invoke-PerformanceCompose -Arguments @('up', '-d', '--build', 'api')
    Wait-PerformanceApi
    Invoke-PerformanceCompose -Arguments @('--profile', 'load-test', 'run', '--rm', 'k6')

    Write-Host "Smoke test passed. Result: $resultsDirectory\smoke-summary.json"
}
finally {
    & docker compose @composeArguments down --volumes --remove-orphans
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "The performance environment could not be completely removed."
    }

    Pop-Location
}
