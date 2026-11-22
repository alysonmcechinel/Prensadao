Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:PerformanceRoot = $PSScriptRoot
$script:RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$script:ResultsDirectory = Join-Path $PSScriptRoot 'results'
$script:ComposeArguments = @(
    '-p', 'prensadao-performance',
    '-f', (Join-Path $script:RepositoryRoot 'docker-compose.yml'),
    '-f', (Join-Path $PSScriptRoot 'docker-compose.performance.yml')
)
$script:ReadinessUrl = 'http://localhost:18080/health/ready'

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
        throw "Docker Engine is not available. Start Docker Desktop before running performance tests. Details: $dockerInformation"
    }
}

function Invoke-PerformanceCompose {
    param(
        [Parameter(Mandatory)]
        [string[]]$Arguments
    )

    & docker compose @script:ComposeArguments @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "docker compose failed with exit code $LASTEXITCODE."
    }
}

function Wait-PerformanceApi {
    param(
        [Parameter(Mandatory)]
        [int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        try {
            $response = Invoke-WebRequest -Uri $script:ReadinessUrl -UseBasicParsing -TimeoutSec 3
            if ($response.StatusCode -eq 200) {
                return
            }
        }
        catch {
            Start-Sleep -Seconds 2
        }
    }

    throw "The performance API did not become ready within $TimeoutSeconds seconds."
}

function Get-PerformanceEnvironment {
    $runtimeInformation = & docker run --rm --entrypoint dotnet prensadao/api:dev --list-runtimes 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Could not inspect the .NET runtime used by the performance image: $runtimeInformation"
    }

    $runtimeLine = $runtimeInformation | Where-Object { $_ -match '^Microsoft\.NETCore\.App\s+([^\s]+)' } | Select-Object -First 1
    if (-not $runtimeLine -or $runtimeLine -notmatch '^Microsoft\.NETCore\.App\s+([^\s]+)') {
        throw "Could not identify Microsoft.NETCore.App in the performance image."
    }

    $processor = [Environment]::GetEnvironmentVariable('PROCESSOR_IDENTIFIER')
    if ([string]::IsNullOrWhiteSpace($processor)) {
        $processor = [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()
    }

    return [ordered]@{
        k6Version = '2.3.0'
        dotnetRuntimeVersion = $Matches[1]
        operatingSystem = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
        processor = $processor
    }
}

function Assert-PerformanceEnvironmentCompatible {
    param(
        [Parameter(Mandatory)]
        [psobject]$ExpectedEnvironment
    )

    $currentEnvironment = Get-PerformanceEnvironment
    foreach ($propertyName in @('k6Version', 'dotnetRuntimeVersion', 'operatingSystem', 'processor')) {
        $expectedValue = [string]$ExpectedEnvironment.$propertyName
        $currentValue = [string]$currentEnvironment[$propertyName]
        if ($expectedValue -ne $currentValue) {
            throw "Performance baseline is incompatible: '$propertyName' changed from '$expectedValue' to '$currentValue'. Run calibration and explicitly accept a new baseline."
        }
    }
}

function Invoke-PerformanceProfile {
    param(
        [Parameter(Mandatory)]
        [string]$K6Script,

        [ValidateRange(10, 600)]
        [int]$ReadinessTimeoutSeconds = 120,

        [hashtable]$K6Environment = @{},

        [psobject]$ExpectedEnvironment,

        [switch]$OpenReport
    )

    Assert-DockerEngineIsAvailable
    $profileName = [System.IO.Path]::GetFileNameWithoutExtension($K6Script)
    $timestamp = [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmssfff')
    $reportFileName = "$profileName-$timestamp-report.html"
    $reportPath = Join-Path $script:ResultsDirectory $reportFileName
    Push-Location $script:RepositoryRoot
    try {
        New-Item -ItemType Directory -Path $script:ResultsDirectory -Force | Out-Null

        Invoke-PerformanceCompose -Arguments @('up', '-d', '--build', 'api')
        Wait-PerformanceApi -TimeoutSeconds $ReadinessTimeoutSeconds

        if ($null -ne $ExpectedEnvironment) {
            Assert-PerformanceEnvironmentCompatible -ExpectedEnvironment $ExpectedEnvironment
        }

        $runEnvironment = $K6Environment.Clone()
        $runEnvironment['K6_WEB_DASHBOARD'] = 'true'
        # Export the report without keeping an HTTP server alive after the test.
        $runEnvironment['K6_WEB_DASHBOARD_PORT'] = '-1'
        $runEnvironment['K6_WEB_DASHBOARD_EXPORT'] = "/results/$reportFileName"

        $runArguments = @('--profile', 'load-test', 'run', '--rm')
        foreach ($environmentName in ($runEnvironment.Keys | Sort-Object)) {
            $runArguments += @('-e', "$environmentName=$($runEnvironment[$environmentName])")
        }
        $runArguments += @('k6', 'run', "/scripts/$K6Script")

        Invoke-PerformanceCompose -Arguments $runArguments
        if (-not (Test-Path -LiteralPath $reportPath -PathType Leaf)) {
            Write-Warning "k6 did not generate an HTML report. Short runs may not collect enough data; use calibration or regression to generate graphs."
        }
    }
    finally {
        & docker compose @script:ComposeArguments down --volumes --remove-orphans
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "The performance environment could not be completely removed."
        }

        Pop-Location

        if (Test-Path -LiteralPath $reportPath -PathType Leaf) {
            Write-Host "k6 HTML report: $reportPath"
            if ($OpenReport) {
                try {
                    Start-Process -FilePath $reportPath | Out-Null
                }
                catch {
                    Write-Warning "Could not open the HTML report automatically. Open it manually at '$reportPath'. Details: $_"
                }
            }
        }
    }
}
