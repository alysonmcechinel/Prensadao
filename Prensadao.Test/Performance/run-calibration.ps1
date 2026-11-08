[CmdletBinding()]
param(
    [ValidateRange(30, 600)]
    [int]$ReadinessTimeoutSeconds = 120
)

. (Join-Path $PSScriptRoot 'performance-runner.ps1')

Invoke-PerformanceProfile -K6Script 'calibration.js' -ReadinessTimeoutSeconds $ReadinessTimeoutSeconds
Write-Host "Calibration completed. Result: $script:ResultsDirectory\calibration-summary.json"
