param(
    [ValidateSet('Debug', 'Release')]
    [Parameter()]
    [string]
    $Configuration = 'Debug',

    [switch]
    $Test
)
