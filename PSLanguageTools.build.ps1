param(
    [ValidateSet('Debug', 'Release')]
    [string]
    $Configuration = 'Debug',

    [ValidateSet('netcoreapp3.1', 'netstandard2.0')]
    [string[]]
    $BuildTarget = @('netcoreapp3.1','netstandard2.0'),

    [ValidateSet('netcoreapp3.1', 'net5.0', 'net6.0', 'net471')]
    [string[]]
    $TestTarget = @(
        'net6.0'
        if ((-not $IsMacOS) -or ((uname -m) -eq 'x86_64' -and ((sysctl -in sysctl.proc_translated) -ne '1'))){ 'net5.0', 'netcoreapp3.1' }
        if ($IsWindows -ne $false) { 'net471' }
    )
)

task Build {
    foreach ($target in $BuildTarget)
    {
        exec { dotnet build "$PSScriptRoot/src/PSLanguageTools.csproj" -c $Configuration -f $target }
    }
}

task Test TestXUnit

task TestXUnit {
    foreach ($target in $TestTarget)
    {
        exec { dotnet test "$PSScriptRoot/test/XUnit" -c $Configuration -f $target }
    }
}

task . Build,Test

