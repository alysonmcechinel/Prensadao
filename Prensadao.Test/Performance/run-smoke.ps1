param(
    [ValidateRange(10, 600)]
    [int]$ReadinessTimeoutSeconds = 120,

    [switch]$OpenReport
)

. (Join-Path $PSScriptRoot 'performance-runner.ps1')

Invoke-PerformanceProfile -K6Script 'smoke.js' -ReadinessTimeoutSeconds $ReadinessTimeoutSeconds -OpenReport:$OpenReport
Write-Host "Smoke test passed. Result: $script:ResultsDirectory\smoke-summary.json"
