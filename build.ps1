param(
    [ValidateSet('Debug', 'Release')]
    [Parameter()]
    [string]
    $Configuration = 'Debug',

    [switch]
    $Test
)

$tasks = @(
    'Build'
    if ($Test) { 'Test' }
)

Invoke-Build $tasks -Configuration $Configuration