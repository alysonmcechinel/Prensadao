[CmdletBinding()]
param(
    [ValidateRange(30, 600)]
    [int]$ReadinessTimeoutSeconds = 120
)

. (Join-Path $PSScriptRoot 'performance-runner.ps1')

$calibrationPath = Join-Path $script:ResultsDirectory 'calibration-summary.json'
if (-not (Test-Path $calibrationPath)) {
    throw "Calibration result not found at '$calibrationPath'. Run run-calibration.ps1 first."
}

$calibration = Get-Content -Raw $calibrationPath | ConvertFrom-Json
if ($calibration.schemaVersion -ne 1 -or $null -eq $calibration.proposedRegression) {
    throw "Calibration result does not contain a compatible regression proposal."
}

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

$rateRps = [int]$policy.rateRps
$durationSeconds = [int]$policy.durationSeconds
if ($rateRps -ne [int]$calibration.proposedRegression.rateRps `
    -or $durationSeconds -ne [int]$calibration.proposedRegression.durationSeconds) {
    throw "Calibration proposal does not match the versioned performance policy. Review the policy explicitly before accepting a new baseline."
}

$currentEnvironment = Get-PerformanceEnvironment
if ([string]$currentEnvironment.k6Version -ne [string]$policy.k6Version) {
    throw "The installed k6 version does not match the versioned performance policy."
}

Invoke-PerformanceProfile `
    -K6Script 'baseline-candidate.js' `
    -ReadinessTimeoutSeconds $ReadinessTimeoutSeconds `
    -K6Environment @{
        RATE_RPS = $rateRps
        DURATION_SECONDS = $durationSeconds
    }

$candidatePath = Join-Path $script:ResultsDirectory 'baseline-candidate-summary.json'
$candidate = Get-Content -Raw $candidatePath | ConvertFrom-Json
$expectedIterations = $rateRps * $durationSeconds
if ($candidate.checksRate -ne 1 `
    -or $candidate.httpFailureRate -ge 0.01 `
    -or $candidate.droppedIterations -ne 0 `
    -or $candidate.iterations -lt $expectedIterations `
    -or $candidate.orderCreateP95Ms -le 0 `
    -or $candidate.orderToPreparingP95Ms -le 0) {
    throw "Baseline candidate did not satisfy the functional acceptance criteria."
}

$baseline = [ordered]@{
    schemaVersion = 1
    profile = [ordered]@{
        rateRps = $rateRps
        durationSeconds = $durationSeconds
    }
    orderCreateP95Ms = [double]$candidate.orderCreateP95Ms
    orderToPreparingP95Ms = [double]$candidate.orderToPreparingP95Ms
    capturedAt = [string]$candidate.capturedAt
    environment = $currentEnvironment
}

$baselineDirectory = Join-Path $PSScriptRoot 'baseline'
$baselinePath = Join-Path $baselineDirectory 'performance-baseline.local.json'
New-Item -ItemType Directory -Path $baselineDirectory -Force | Out-Null
$baselineJson = $baseline | ConvertTo-Json -Depth 5
[System.IO.File]::WriteAllText($baselinePath, $baselineJson, [System.Text.UTF8Encoding]::new($false))

Write-Host "Performance baseline accepted explicitly: $baselinePath"
