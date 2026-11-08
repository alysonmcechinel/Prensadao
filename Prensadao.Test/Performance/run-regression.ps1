[CmdletBinding()]
param(
    [ValidateRange(30, 600)]
    [int]$ReadinessTimeoutSeconds = 120
)

. (Join-Path $PSScriptRoot 'performance-runner.ps1')

$policyPath = Join-Path $PSScriptRoot 'baseline\performance-policy.json'
if (-not (Test-Path $policyPath)) {
    throw "Versioned performance policy not found at '$policyPath'."
}

$policy = Get-Content -Raw $policyPath | ConvertFrom-Json
if ($policy.schemaVersion -ne 1 `
    -or $policy.rateRps -le 0 `
    -or $policy.durationSeconds -le 0 `
    -or $policy.maximumDegradationPercent -le 0 `
    -or [string]::IsNullOrWhiteSpace($policy.k6Version)) {
    throw "Versioned performance policy is invalid or incompatible."
}

$baselinePath = Join-Path $PSScriptRoot 'baseline\performance-baseline.local.json'
if (-not (Test-Path $baselinePath)) {
    throw "Accepted performance baseline not found at '$baselinePath'. Run accept-baseline.ps1 first."
}

$baseline = Get-Content -Raw $baselinePath | ConvertFrom-Json
if ($baseline.schemaVersion -ne 1 `
    -or $baseline.profile.rateRps -le 0 `
    -or $baseline.profile.durationSeconds -le 0 `
    -or $baseline.orderCreateP95Ms -le 0 `
    -or $baseline.orderToPreparingP95Ms -le 0 `
    -or $null -eq $baseline.environment) {
    throw "Accepted performance baseline is invalid or incompatible."
}

if ([int]$baseline.profile.rateRps -ne [int]$policy.rateRps `
    -or [int]$baseline.profile.durationSeconds -ne [int]$policy.durationSeconds `
    -or [string]$baseline.environment.k6Version -ne [string]$policy.k6Version) {
    throw "Local performance baseline does not match the versioned performance policy. Accept a new local baseline."
}

$invariantCulture = [System.Globalization.CultureInfo]::InvariantCulture
$degradationMultiplier = 1 + ([double]$policy.maximumDegradationPercent / 100)
$maxOrderCreateP95Ms = [double]$baseline.orderCreateP95Ms * $degradationMultiplier
$maxOrderToPreparingP95Ms = [double]$baseline.orderToPreparingP95Ms * $degradationMultiplier
$k6Environment = @{
    RATE_RPS = [int]$baseline.profile.rateRps
    DURATION_SECONDS = [int]$baseline.profile.durationSeconds
    BASELINE_ORDER_CREATE_P95_MS = ([double]$baseline.orderCreateP95Ms).ToString($invariantCulture)
    BASELINE_ORDER_TO_PREPARING_P95_MS = ([double]$baseline.orderToPreparingP95Ms).ToString($invariantCulture)
    MAX_ORDER_CREATE_P95_MS = $maxOrderCreateP95Ms.ToString($invariantCulture)
    MAX_ORDER_TO_PREPARING_P95_MS = $maxOrderToPreparingP95Ms.ToString($invariantCulture)
}

Invoke-PerformanceProfile `
    -K6Script 'regression.js' `
    -ReadinessTimeoutSeconds $ReadinessTimeoutSeconds `
    -K6Environment $k6Environment `
    -ExpectedEnvironment $baseline.environment

Write-Host "Performance regression passed. Result: $script:ResultsDirectory\regression-summary.json"
